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
using WebPagePub.Core.Utilities; // for .UrlKey()
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
                    q = q.Where(x => x.SitePageSectionId == sitePageSectionId.Value);

                // filter by IsLive if requested
                if (isLive.HasValue)
                    q = q.Where(x => x.IsLive == isLive.Value);

                // filter by publish date
                if (publishedFromUtc.HasValue)
                    q = q.Where(x => x.PublishDateTimeUtc >= publishedFromUtc.Value);

                if (publishedToUtc.HasValue)
                    q = q.Where(x => x.PublishDateTimeUtc <= publishedToUtc.Value);

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
                        .Select(t => t.UrlKey())                // << correct usage
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
        if (pageNumber < 1) pageNumber = 1;
        if (pageSize < 1) pageSize = 10;

        term = (term ?? string.Empty).Trim();

        // Base query with includes you want in the UI
        IQueryable<SitePage> q = this.Context.SitePage
            .Include(x => x.SitePageSection)
            .Include(x => x.Author)
            .Include(x => x.SitePageTags).ThenInclude(t => t.Tag);

        if (!string.IsNullOrWhiteSpace(term))
        {
            var like = $"%{term}%";
            q = q.Where(x =>
                   EF.Functions.Like(x.Title, like)
                || EF.Functions.Like(x.Content, like)
                || EF.Functions.Like(x.PageHeader, like)
                || EF.Functions.Like(x.MetaDescription, like)
                || EF.Functions.Like(x.BreadcrumbName, like)
                || EF.Functions.Like(x.Key, like)
                || EF.Functions.Like(x.ReviewItemName, like)
                || x.SitePageTags.Any(t =>
                       EF.Functions.Like(t.Tag.Name, like) ||
                       EF.Functions.Like(t.Tag.Key, like)));
        }

        // Order newest first by publish, then create
        q = q.OrderByDescending(x => x.PublishDateTimeUtc > x.CreateDate
                                       ? x.PublishDateTimeUtc
                                       : x.CreateDate)
             .ThenByDescending(x => x.CreateDate);

        var total = await q.CountAsync();

        var items = await q.Skip((pageNumber - 1) * pageSize)
                           .Take(pageSize)
                           .ToListAsync();

        return new PagedResult<SitePage>
        {    
            TotalCount = total,
            Items = items
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
            var results = new List<SitePage>();
            var now = DateTime.UtcNow;
            try
            {
                results = this.Context.SitePage
                            .Where(x => x.Title.Contains(term) ||
                                x.Content.Contains(term) ||
                                x.PageHeader.Contains(term) ||
                                x.MetaDescription.Contains(term) ||
                                x.BreadcrumbName.Contains(term) ||
                                x.Key.Contains(term) ||
                                x.ReviewItemName.Contains(term))
                            .Include(x => x.SitePageSection)
                                   .Include(x => x.Photos)
                                   .Include(x => x.Author)
                                   .OrderByDescending(page => page.CreateDate)
                                   .Skip(quantityPerPage * (pageNumber - 1))
                                   .Take(quantityPerPage)
                            .ToList();

                total = this.Context.SitePage
                                .Where(x => x.Title.Contains(term) ||
                                    x.Content.Contains(term) ||
                                    x.PageHeader.Contains(term) ||
                                    x.MetaDescription.Contains(term) ||
                                    x.BreadcrumbName.Contains(term) ||
                                    x.Key.Contains(term) ||
                                    x.ReviewItemName.Contains(term)).Count();

                return results;
            }
            catch (Exception ex)
            {
                Log.Fatal(ex);

                throw new Exception(StringConstants.DBErrorMessage, ex.InnerException);
            }
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