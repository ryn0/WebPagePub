using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using log4net;
using Microsoft.EntityFrameworkCore;
using WebPagePub.Core;
using WebPagePub.Core.Utilities;
using WebPagePub.Data.Constants;
using WebPagePub.Data.DbContextInfo.Interfaces;
using WebPagePub.Data.Models;
using WebPagePub.Data.Models.Transfer;
using WebPagePub.Data.Repositories.Interfaces;

namespace WebPagePub.Data.Repositories.Implementations
{
    public class SitePageRepository : ISitePageRepository
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public SitePageRepository(
            IApplicationDbContext context,
            ISitePageAuditRepository sitePageAuditRepository)
        {
            this.Context = context;
            this.SitePageAuditRepository = sitePageAuditRepository;
        }

        public IApplicationDbContext Context { get; private set; }

        private ISitePageAuditRepository SitePageAuditRepository { get; set; }

        public async Task<SitePage> CreateAsync(SitePage model)
        {
            try
            {
                model.WordCount = TextUtilities.GetWordCount(model.Content);

                this.Context.SitePage.Add(model);
                this.Context.SaveChanges();

                await this.LogToAuditTable(model);

                return model;
            }
            catch (Exception ex)
            {
                Log.Fatal(ex);

                throw new Exception(StringConstants.DBErrorMessage, ex.InnerException);
            }
        }

        public IList<SitePage> SearchAdvanced(
            string? term,
            IEnumerable<string>? tags,
            int? sitePageSectionId,
            bool? isLive,
            DateTime? publishedFromUtc,
            DateTime? publishedToUtc,
            int pageNumber,
            int quantityPerPage,
            out int total)
        {
            try
            {
                term = (term ?? string.Empty).Trim();
                var hasTerm = !string.IsNullOrWhiteSpace(term);
                var now = DateTime.UtcNow;

                // normalize tag filters (match by Name OR Key)
                var tagList = (tags ?? Enumerable.Empty<string>())
                    .Select(t => t?.Trim())
                    .Where(t => !string.IsNullOrWhiteSpace(t))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();

                var termLower = term.ToLowerInvariant();

                var q = this.Context.SitePage
                    .Include(x => x.SitePageSection)
                    .Include(x => x.Author)
                    .Include(x => x.SitePageTags)
                    .ThenInclude(x => x.Tag)
                    .AsQueryable();

                // filter by section
                if (sitePageSectionId.HasValue && sitePageSectionId.Value > 0)
                {
                    q = q.Where(x => x.SitePageSectionId == sitePageSectionId.Value);
                }

                // filter by IsLive if requested
                if (isLive.HasValue)
                {
                    q = q.Where(x => x.IsLive == isLive.Value);
                }

                // filter by publish date
                if (publishedFromUtc.HasValue)
                {
                    q = q.Where(x => x.PublishDateTimeUtc >= publishedFromUtc.Value);
                }

                if (publishedToUtc.HasValue)
                {
                    q = q.Where(x => x.PublishDateTimeUtc <= publishedToUtc.Value);
                }

                // filter by text term across common page properties
                if (hasTerm)
                {
                    q = q.Where(x =>
                        x.Title.ToLower().Contains(termLower) ||
                        x.Content.ToLower().Contains(termLower) ||
                        x.PageHeader.ToLower().Contains(termLower) ||
                        x.MetaDescription.ToLower().Contains(termLower) ||
                        x.BreadcrumbName.ToLower().Contains(termLower) ||
                        x.Key.ToLower().Contains(termLower) ||
                        x.ReviewItemName.ToLower().Contains(termLower));
                }

                // filter by tags (ANY match)
                if (tagList.Count > 0)
                {
                    // normalize inputs
                    var names = tagList
                        .Where(t => !string.IsNullOrWhiteSpace(t))
                        .Select(t => t.Trim())
                        .ToList();

                    var nameLowers = names
                        .Select(t => t.ToLowerInvariant())
                        .ToList();

                    var keyLowers = names
                        .Select(t => t.UrlKey()) // << correct usage
                        .Select(t => t.ToLowerInvariant())
                        .ToList();

                    q = q.Where(x => x.SitePageTags.Any(pt =>
                        nameLowers.Contains(pt.Tag.Name.ToLower()) ||
                        keyLowers.Contains(pt.Tag.Key.ToLower())));
                }

                // Count before paging
                total = q.Count();

                // Order newest first (publish date if set, else create date)
                q = q.OrderByDescending(x =>
                        x.PublishDateTimeUtc == DateTime.MinValue
                            ? x.CreateDate
                            : x.PublishDateTimeUtc)
                     .ThenByDescending(x => x.CreateDate);

                // paging
                var results = q
                    .Skip((pageNumber - 1) * quantityPerPage)
                    .Take(quantityPerPage)
                    .ToList();

                return results;
            }
            catch (Exception ex)
            {
                Log.Fatal(ex);
                throw new Exception(StringConstants.DBErrorMessage, ex.InnerException ?? ex);
            }
        }

        public async Task<PagedResult<SitePage>> PagedSearchAsync(string term, int pageNumber, int pageSize)
        {
            if (pageNumber < 1)
            {
                pageNumber = 1;
            }

            if (pageSize < 1)
            {
                pageSize = 10;
            }

            term = (term ?? string.Empty).Trim();

            // Base (no Includes for scoring; we rehydrate after paging)
            IQueryable<SitePage> baseQ = this.Context.SitePage;

            // If no term, keep your original "newest first" behavior with Includes
            if (string.IsNullOrWhiteSpace(term))
            {
                var totalNoTerm = await baseQ.CountAsync();
                var itemsNoTerm = await this.Context.SitePage
                    .Include(x => x.SitePageSection)
                    .Include(x => x.Author)
                    .Include(x => x.SitePageTags).ThenInclude(t => t.Tag)
                    .OrderByDescending(x => x.PublishDateTimeUtc > x.CreateDate ? x.PublishDateTimeUtc : x.CreateDate)
                    .ThenByDescending(x => x.CreateDate)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                return new PagedResult<SitePage>
                {
                    TotalCount = totalNoTerm,
                    Items = itemsNoTerm
                };
            }

            var likeAny = $"%{term}%";
            var likeStart = $"{term}%";
            var termLen = term.Length; // non-zero here

            // Filter candidates (includes tag name/key matches like your original)
            var candidates = baseQ.Where(x =>
                   EF.Functions.Like(x.Title ?? string.Empty, likeAny)
                || EF.Functions.Like(x.Content ?? string.Empty, likeAny)
                || EF.Functions.Like(x.PageHeader ?? string.Empty, likeAny)
                || EF.Functions.Like(x.MetaDescription ?? string.Empty, likeAny)
                || EF.Functions.Like(x.BreadcrumbName ?? string.Empty, likeAny)
                || EF.Functions.Like(x.Key ?? string.Empty, likeAny)
                || EF.Functions.Like(x.ReviewItemName ?? string.Empty, likeAny)
                || x.SitePageTags.Any(t =>
                       EF.Functions.Like(t.Tag.Name ?? string.Empty, likeAny) ||
                       EF.Functions.Like(t.Tag.Key ?? string.Empty, likeAny)));

            var total = await candidates.CountAsync();

            // Score by weighted occurrence counts + big title bonus; tie-break by recency
            var scoredIds = await candidates
                .Select(x => new
                {
                    x.SitePageId,

                    TitleCnt = (double)((x.Title ?? string.Empty).Length - (x.Title ?? string.Empty).Replace(term, string.Empty).Length) / termLen,
                    HeaderCnt = (double)((x.PageHeader ?? string.Empty).Length - (x.PageHeader ?? string.Empty).Replace(term, string.Empty).Length) / termLen,
                    CrumbCnt = (double)((x.BreadcrumbName ?? string.Empty).Length - (x.BreadcrumbName ?? string.Empty).Replace(term, string.Empty).Length) / termLen,
                    KeyCnt = (double)((x.Key ?? string.Empty).Length - (x.Key ?? string.Empty).Replace(term, string.Empty).Length) / termLen,
                    MetaCnt = (double)((x.MetaDescription ?? string.Empty).Length - (x.MetaDescription ?? string.Empty).Replace(term, string.Empty).Length) / termLen,
                    ReviewCnt = (double)((x.ReviewItemName ?? string.Empty).Length - (x.ReviewItemName ?? string.Empty).Replace(term, string.Empty).Length) / termLen,
                    ContentCnt = (double)((x.Content ?? string.Empty).Length - (x.Content ?? string.Empty).Replace(term, string.Empty).Length) / termLen,

                    TitleHit = EF.Functions.Like(x.Title ?? string.Empty, likeAny),
                    StartsHit = EF.Functions.Like(x.Title ?? string.Empty, likeStart),

                    Recency = x.PublishDateTimeUtc > x.CreateDate ? x.PublishDateTimeUtc : x.CreateDate
                })
                .Select(r => new
                {
                    r.SitePageId,
                    TotalScore =
                        (r.TitleCnt * 100.0) +
                        (r.HeaderCnt * 20.0) +
                        (r.CrumbCnt * 15.0) +
                        (r.KeyCnt * 20.0) +
                        (r.MetaCnt * 10.0) +
                        (r.ReviewCnt * 10.0) +
                        (r.ContentCnt * 5.0)
                        + (r.TitleHit ? 5000.0 : 0.0)
                        + (r.StartsHit ? 1500.0 : 0.0),
                    r.Recency
                })
                .OrderByDescending(r => r.TotalScore)
                .ThenByDescending(r => r.Recency)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(r => r.SitePageId)
                .ToListAsync();

            // Rehydrate with Includes and preserve the scored order
            var map = await this.Context.SitePage
                .Where(x => scoredIds.Contains(x.SitePageId))
                .Include(x => x.SitePageSection)
                .Include(x => x.Author)
                .Include(x => x.SitePageTags).ThenInclude(t => t.Tag)
                .ToDictionaryAsync(x => x.SitePageId);

            var orderedItems = scoredIds.Where(map.ContainsKey).Select(id => map[id]).ToList();

            return new PagedResult<SitePage>
            {
                TotalCount = total,
                Items = orderedItems
            };
        }

        public IList<SitePage> GetPage(int pageNumber, int sitePageSectionId, int quantityPerPage, out int total)
        {
            try
            {
                var model = this.Context.SitePage
                                   .Where(x => x.SitePageSectionId == sitePageSectionId)
                                   .OrderByDescending(page => page.CreateDate)
                                   .Skip(quantityPerPage * (pageNumber - 1))
                                   .Take(quantityPerPage)
                                   .Include(x => x.SitePageSection)
                                   .ToList();

                total = this.Context.SitePage.Where(x => x.SitePageSectionId == sitePageSectionId).Count();

                return model;
            }
            catch (Exception ex)
            {
                Log.Fatal(ex);

                throw new Exception(StringConstants.DBErrorMessage, ex.InnerException);
            }
        }

        public IList<SitePage> GetLivePage(int pageNumber, int quantityPerPage, out int total)
        {
            var now = DateTime.UtcNow;

            try
            {
                var model = this.Context.SitePage
                                   .Where(x => x.IsLive == true && x.PublishDateTimeUtc < now)
                                   .Include(x => x.Photos)
                                   .Include(x => x.Author)
                                   .Include(x => x.SitePageSection)
                                   .Include(x => x.SitePageTags)
                                   .Include("SitePageTags.Tag")
                                   .OrderByDescending(blog => blog.PublishDateTimeUtc)
                                   .Skip(quantityPerPage * (pageNumber - 1))
                                   .Take(quantityPerPage)
                                   .ToList();

                total = this.Context.SitePage.Where(x => x.IsLive == true && x.PublishDateTimeUtc < now).Count();

                return model;
            }
            catch (Exception ex)
            {
                Log.Fatal(ex);

                throw new Exception(StringConstants.DBErrorMessage, ex.InnerException);
            }
        }

        public IList<SiteMapDisplaySection> GetAllLinksAndTitles()
        {
            var now = DateTime.UtcNow;

            try
            {
                var pageItems = this.Context.SitePage
                                       .Where(x => x.IsLive && x.PublishDateTimeUtc < now)
                                       .Select(x => new SiteMapDisplayItem
                                       {
                                           PageTitle = x.Title,
                                           RelativePath = UrlBuilder.BlogUrlPath(x.SitePageSection.Key, x.Key)
                                       })
                                       .ToList();

                var model = this.GroupItemsIntoSections(pageItems);

                return model;
            }
            catch (Exception ex)
            {
                Log.Fatal(ex);

                throw new Exception(StringConstants.DBErrorMessage, ex.InnerException);
            }
        }

        public SitePage GetPreviouslyCreatedEntry(DateTime createDateow, int sitePageId, int sitePageSectionId)
        {
            try
            {
                var model = this.Context.SitePage
                                   .Where(x => x.IsSectionHomePage == false &&
                                               x.SitePageSectionId == sitePageSectionId &&
                                               x.SitePageId != sitePageId &&
                                               x.CreateDate <= createDateow)
                                   .OrderByDescending(x => x.CreateDate)
                                   .Include(x => x.Photos)
                                   .Include(x => x.Author)
                                   .Include(x => x.SitePageSection)
                                   .Include(x => x.SitePageTags)
                                   .Include("SitePageTags.Tag")
                                   .FirstOrDefault();

                return model;
            }
            catch (Exception ex)
            {
                Log.Fatal(ex);

                throw new Exception(StringConstants.DBErrorMessage, ex.InnerException);
            }
        }

        public SitePage GetNextCreatedEntry(DateTime createDate, int sitePageId, int sitePageSectionId)
        {
            try
            {
                var model = this.Context.SitePage
                                   .Where(x => x.IsSectionHomePage == false &&
                                               x.SitePageSectionId == sitePageSectionId &&
                                               x.SitePageId != sitePageId &&
                                               x.CreateDate >= createDate)
                                   .OrderBy(x => x.CreateDate)
                                   .Include(x => x.Photos)
                                   .Include(x => x.Author)
                                   .Include(x => x.SitePageSection)
                                   .Include(x => x.SitePageTags)
                                   .Include("SitePageTags.Tag")
                                   .FirstOrDefault();

                return model;
            }
            catch (Exception ex)
            {
                Log.Fatal(ex);

                throw new Exception(StringConstants.DBErrorMessage, ex.InnerException);
            }
        }

        public SitePage GetPreviousEntry(DateTime currentSitePagePublishDateTimeUtc, DateTime now, int sitePageSectionId)
        {
            try
            {
                var model = this.Context.SitePage
                                   .Where(x => x.PublishDateTimeUtc < now &&
                                               x.PublishDateTimeUtc < currentSitePagePublishDateTimeUtc &&
                                               x.IsLive == true && x.IsSectionHomePage == false &&
                                               x.SitePageSectionId == sitePageSectionId)
                                   .OrderByDescending(x => x.PublishDateTimeUtc)
                                   .Include(x => x.Photos)
                                   .Include(x => x.Author)
                                   .Include(x => x.SitePageSection)
                                   .Include(x => x.SitePageTags)
                                   .Include("SitePageTags.Tag")
                                   .FirstOrDefault();

                return model;
            }
            catch (Exception ex)
            {
                Log.Fatal(ex);

                throw new Exception(StringConstants.DBErrorMessage, ex.InnerException);
            }
        }

        public SitePage GetNextEntry(DateTime currentSitePagePublishDateTimeUtc, DateTime now, int sitePageSectionId)
        {
            try
            {
                var model = this.Context.SitePage
                                   .Where(x => x.PublishDateTimeUtc < now &&
                                               x.PublishDateTimeUtc > currentSitePagePublishDateTimeUtc &&
                                               x.IsLive == true && x.IsSectionHomePage == false &&
                                               x.SitePageSectionId == sitePageSectionId)
                                   .OrderBy(x => x.PublishDateTimeUtc)
                                   .Include(x => x.Photos)
                                   .Include(x => x.Author)
                                   .Include(x => x.SitePageSection)
                                   .Include(x => x.SitePageTags)
                                   .Include("SitePageTags.Tag")
                                   .FirstOrDefault();

                return model;
            }
            catch (Exception ex)
            {
                Log.Fatal(ex);

                throw new Exception(StringConstants.DBErrorMessage, ex.InnerException);
            }
        }

        public IList<SitePage> GetLivePageByTag(string tagKey, int pageNumber, int quantityPerPage, out int total)
        {
            var now = DateTime.UtcNow;

            try
            {
                var model = this.Context.SitePage
                                   .Where(x => x.IsLive == true &&
                                               x.PublishDateTimeUtc < now &&
                                               (x.SitePageTags.FirstOrDefault(y => y.Tag.Key == tagKey) != null))
                                   .Include(x => x.SitePageSection)
                                   .Include(x => x.Photos)
                                   .Include(x => x.SitePageTags)
                                   .Include(x => x.Author)
                                   .Include("SitePageTags.Tag")
                                   .OrderByDescending(blog => blog.PublishDateTimeUtc)
                                   .Skip(quantityPerPage * (pageNumber - 1))
                                   .Take(quantityPerPage)
                                   .ToList();

                total = this.Context.SitePage.Where(x => x.IsLive == true &&
                                               x.PublishDateTimeUtc < now &&
                                               (x.SitePageTags.FirstOrDefault(y => y.Tag.Key == tagKey) != null)).Count();

                return model;
            }
            catch (Exception ex)
            {
                Log.Fatal(ex);

                throw new Exception(StringConstants.DBErrorMessage, ex.InnerException);
            }
        }

        public void Dispose()
        {
            this.Context.Dispose();
        }

        public SitePage Get(int sitePageId)
        {
            try
            {
                return this.Context.SitePage
                              .Include(x => x.SitePageSection)
                              .Include(x => x.Photos)
                              .Include(x => x.SitePageTags)
                              .Include(x => x.Author)
                              .Include("SitePageTags.Tag")
                              .FirstOrDefault(x => x.SitePageId == sitePageId);
            }
            catch (Exception ex)
            {
                throw new Exception(StringConstants.DBErrorMessage, ex.InnerException);
            }
        }

        public SitePage Get(string key)
        {
            try
            {
                return this.Context.SitePage
                              .Include(x => x.SitePageSection)
                              .Include(x => x.Photos)
                              .Include(x => x.SitePageTags)
                              .Include(x => x.Author)
                              .Include("SitePageTags.Tag")
                              .FirstOrDefault(x => x.Key == key);
            }
            catch (Exception ex)
            {
                Log.Fatal(ex);

                throw new Exception(StringConstants.DBErrorMessage, ex.InnerException);
            }
        }

        public async Task<bool> UpdateAsync(SitePage model)
        {
            try
            {
                if (model.IsSectionHomePage)
                {
                    foreach (var page in this.Context.SitePage
                                                .Where(x => x.SitePageSectionId == model.SitePageSectionId)
                                                .ToList())
                    {
                        page.IsSectionHomePage = false;

                        if (page.SitePageId == model.SitePageId)
                        {
                            page.IsSectionHomePage = true;
                        }
                    }
                }

                model.WordCount = TextUtilities.GetWordCount(model.Content);

                this.Context.SitePage.Update(model);
                this.Context.SaveChanges();

                await this.LogToAuditTable(model);

                return true;
            }
            catch (Exception ex)
            {
                Log.Fatal(ex);

                throw new Exception(StringConstants.DBErrorMessage, ex.InnerException);
            }
        }

        public bool Delete(int sitePageId)
        {
            try
            {
                var entry = this.Context.SitePage.Find(sitePageId);

                this.Context.SitePage.Remove(entry);
                this.Context.SaveChanges();

                return true;
            }
            catch (Exception ex)
            {
                Log.Fatal(ex);

                return false;
            }
        }

        public SitePage Get(int sitePageSectionId, string key)
        {
            try
            {
                return this.Context.SitePage
                              .Include(x => x.SitePageSection)
                              .Include(x => x.Photos)
                              .Include(x => x.SitePageTags)
                              .Include(x => x.Author)
                              .Include("SitePageTags.Tag")
                              .FirstOrDefault(x => x.SitePageSectionId == sitePageSectionId && x.Key == key);
            }
            catch (Exception ex)
            {
                throw new Exception(StringConstants.DBErrorMessage, ex.InnerException);
            }
        }

        public IList<SitePage> GetLivePagesForSection(int sitePageSectionId)
        {
            try
            {
                return this.Context.SitePage
                              .Include(x => x.SitePageSection)
                              .Include(x => x.Photos)
                              .Include(x => x.SitePageTags)
                              .Include(x => x.Author)
                              .Include("SitePageTags.Tag")
                              .Where(x => x.SitePageSectionId == sitePageSectionId && x.IsLive == true)
                              .OrderByDescending(x => x.PublishDateTimeUtc)
                              .ToList();
            }
            catch (Exception ex)
            {
                throw new Exception(StringConstants.DBErrorMessage, ex.InnerException);
            }
        }

        public IList<SitePage> GetLivePageBySection(int sectionId, int pageNumber, int quantityPerPage, out int total)
        {
            var now = DateTime.UtcNow;
            try
            {
                // Calculate total count first
                total = this.Context.SitePage
                    .Where(x => x.IsLive == true &&
                                x.PublishDateTimeUtc < now &&
                                x.SitePageSectionId == sectionId &&
                                x.IsSectionHomePage == false)
                    .Count();

                // Fetch paginated results
                var model = this.Context.SitePage
                    .Where(x => x.IsLive == true &&
                                x.PublishDateTimeUtc < now &&
                                x.SitePageSectionId == sectionId)
                    .Include(x => x.SitePageSection)
                    .Include(x => x.Photos)
                    .Include(x => x.SitePageTags)
                    .Include(x => x.Author)
                    .Include("SitePageTags.Tag")
                    .OrderByDescending(blog => blog.PublishDateTimeUtc)
                    .Skip((pageNumber - 1) * quantityPerPage)
                    .Take(quantityPerPage)
                    .ToList();

                // Ensure we only return the requested number of items
                return model.Take(quantityPerPage).ToList();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex);
                throw new Exception(StringConstants.DBErrorMessage, ex);
            }
        }

        public SitePage GetSectionHomePage(int sitePageSectionId)
        {
            try
            {
                return this.Context.SitePage
                              .FirstOrDefault(x => x.IsSectionHomePage == true &&
                                                   x.SitePageSectionId == sitePageSectionId);
            }
            catch (Exception ex)
            {
                Log.Fatal(ex);

                throw new Exception(StringConstants.DBErrorMessage, ex.InnerException);
            }
        }

        public IList<SitePage> GetIgnoredPages()
        {
            try
            {
                return this.Context.SitePage
                              .Where(x => x.ExcludePageFromSiteMapXml == true && x.IsLive == true)
                              .ToList();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex);

                throw new Exception(StringConstants.DBErrorMessage, ex.InnerException);
            }
        }

        public IList<SitePage> SearchForTerm(string term, int pageNumber, int quantityPerPage, out int total)
        {
            if (pageNumber < 1)
            {
                pageNumber = 1;
            }

            if (quantityPerPage < 1)
            {
                quantityPerPage = 10;
            }

            term = (term ?? string.Empty).Trim();

            // No term: newest live pages
            if (string.IsNullOrWhiteSpace(term))
            {
                total = this.Context.SitePage.Count(x => x.IsLive);
                return this.Context.SitePage
                    .Where(x => x.IsLive)
                    .Include(x => x.SitePageSection)
                    .Include(x => x.Photos)
                    .Include(x => x.Author)
                    .OrderByDescending(x => x.CreateDate)
                    .Skip((pageNumber - 1) * quantityPerPage)
                    .Take(quantityPerPage)
                    .ToList();
            }

            var like = $"%{term}%";
            var termLen = term.Length;

            // Candidate set
            var baseQ = this.Context.SitePage
                .Where(x => x.IsLive && (
                       EF.Functions.Like(x.Title ?? string.Empty, like)
                    || EF.Functions.Like(x.Content ?? string.Empty, like)
                    || EF.Functions.Like(x.PageHeader ?? string.Empty, like)
                    || EF.Functions.Like(x.MetaDescription ?? string.Empty, like)
                    || EF.Functions.Like(x.BreadcrumbName ?? string.Empty, like)
                    || EF.Functions.Like(x.Key ?? string.Empty, like)
                    || EF.Functions.Like(x.ReviewItemName ?? string.Empty, like)));

            total = baseQ.Count();
            if (total == 0)
            {
                return new List<SitePage>();
            }

            // Helper inline for occurrence counts: ((LEN(f)-LEN(REPLACE(f,term,'')))/LEN(term))
            // We multiply before dividing to keep it integer and SQL-translatable.
            var scored = baseQ
                .Select(x => new
                {
                    x.SitePageId,

                    TitleHit = EF.Functions.Like(x.Title ?? string.Empty, like), // hard bump to the top

                    // Occurrence counts per field (integer)
                    TitleCnt = ((x.Title ?? string.Empty).Length - (x.Title ?? string.Empty).Replace(term, string.Empty).Length) / termLen,
                    HeaderCnt = ((x.PageHeader ?? string.Empty).Length - (x.PageHeader ?? string.Empty).Replace(term, string.Empty).Length) / termLen,
                    CrumbCnt = ((x.BreadcrumbName ?? string.Empty).Length - (x.BreadcrumbName ?? string.Empty).Replace(term, string.Empty).Length) / termLen,
                    KeyCnt = ((x.Key ?? string.Empty).Length - (x.Key ?? string.Empty).Replace(term, string.Empty).Length) / termLen,
                    MetaCnt = ((x.MetaDescription ?? string.Empty).Length - (x.MetaDescription ?? string.Empty).Replace(term, string.Empty).Length) / termLen,
                    ReviewCnt = ((x.ReviewItemName ?? string.Empty).Length - (x.ReviewItemName ?? string.Empty).Replace(term, string.Empty).Length) / termLen,
                    ContentCnt = ((x.Content ?? string.Empty).Length - (x.Content ?? string.Empty).Replace(term, string.Empty).Length) / termLen,

                    Recency = x.PublishDateTimeUtc > x.CreateDate ? x.PublishDateTimeUtc : x.CreateDate
                })
                .Select(r => new
                {
                    r.SitePageId,
                    r.TitleHit,

                    // Light weights—title count matters most inside the TitleHit bucket
                    Score =
                        (r.TitleCnt * 10) +
                        (r.HeaderCnt * 7) +
                        (r.CrumbCnt * 6) +
                        (r.KeyCnt * 6) +
                        (r.MetaCnt * 4) +
                        (r.ReviewCnt * 4) +
                        (r.ContentCnt * 1),
                    r.Recency
                })
                .OrderByDescending(r => r.TitleHit)
                .ThenByDescending(r => r.Score)
                .ThenByDescending(r => r.Recency)
                .Skip((pageNumber - 1) * quantityPerPage)
                .Take(quantityPerPage)
                .ToList();

            var ids = scored.Select(s => s.SitePageId).ToList();

            // Rehydrate with includes and preserve order
            var pageMap = this.Context.SitePage
                .Where(x => ids.Contains(x.SitePageId))
                .Include(x => x.SitePageSection)
                .Include(x => x.Photos)
                .Include(x => x.Author)
                .ToList()
                .ToDictionary(x => x.SitePageId);

            return ids.Where(id => pageMap.ContainsKey(id)).Select(id => pageMap[id]).ToList();
        }

        private async Task LogToAuditTable(SitePage model)
        {
            await this.SitePageAuditRepository.CreateAsync(
                new SitePageAudit()
                {
                    SitePageId = model.SitePageId,
                    AllowsComments = model.AllowsComments,
                    AuthorId = model.AuthorId,
                    BreadcrumbName = model.BreadcrumbName,
                    Content = model.Content,
                    CreateDate = model.CreateDate,
                    CreatedByUserId = model.CreatedByUserId,
                    ExcludePageFromSiteMapXml = model.ExcludePageFromSiteMapXml,
                    IsLive = model.IsLive,
                    IsSectionHomePage = model.IsSectionHomePage,
                    Key = model.Key,
                    MetaDescription = model.MetaDescription,
                    MetaKeywords = model.MetaKeywords,
                    PageHeader = model.PageHeader,
                    PageType = model.PageType,
                    PublishDateTimeUtc = model.PublishDateTimeUtc,
                    ReviewBestValue = model.ReviewBestValue,
                    ReviewItemName = model.ReviewItemName,
                    ReviewRatingValue = model.ReviewRatingValue,
                    ReviewWorstValue = model.ReviewWorstValue,
                    SitePageSectionId = model.SitePageSectionId,
                    Title = model.Title,
                    UpdateDate = model.UpdateDate,
                    UpdatedByUserId = model.UpdatedByUserId,
                });
        }

        private IList<SiteMapDisplaySection> GroupItemsIntoSections(IList<SiteMapDisplayItem> items)
        {
            var sections = new List<SiteMapDisplaySection>();

            // Group items by their root path
            var groupedItems = items.GroupBy(
                item => item.RelativePath.Split('/')[1]); // This assumes the format is always '/root/child'

            foreach (var group in groupedItems)
            {
                var rootPath = "/" + group.Key;
                var rootItem = group.FirstOrDefault(i => i.RelativePath == rootPath || i.RelativePath == $"{rootPath}/{StringConstants.DefaultKey}");

                var section = new SiteMapDisplaySection
                {
                    RelativePath = rootPath,
                    Items = new List<SiteMapDisplayItem>(),
                    PageTitle = rootItem?.PageTitle ?? group.Key // Fallback to group.Key if rootItem is null
                };

                foreach (var item in group.OrderBy(i => i.PageTitle)) // Sort children by title
                {
                    // Skip the root/index page of the section
                    if (item != rootItem)
                    {
                        section.Items.Add(item);
                    }
                }

                sections.Add(section);
            }

            // Sort sections by a consistent property, fallback to RelativePath if PageTitle is not available
            return sections.OrderBy(s => s.PageTitle ?? s.RelativePath).ToList();
        }
    }
}