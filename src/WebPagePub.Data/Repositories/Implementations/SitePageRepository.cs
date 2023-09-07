using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using log4net;
using Microsoft.EntityFrameworkCore;
using WebPagePub.Data.Constants;
using WebPagePub.Data.DbContextInfo;
using WebPagePub.Data.Models;
using WebPagePub.Data.Repositories.Interfaces;

namespace WebPagePub.Data.Repositories.Implementations
{
    public class SitePageRepository : ISitePageRepository
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public SitePageRepository(IApplicationDbContext context)
        {
            this.Context = context;
        }

        public IApplicationDbContext Context { get; private set; }

        public SitePage Create(SitePage model)
        {
            try
            {
                this.Context.SitePage.Add(model);
                this.Context.SaveChanges();

                return model;
            }
            catch (Exception ex)
            {
                Log.Fatal(ex);

                throw new Exception(StringConstants.DBErrorMessage, ex.InnerException);
            }
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

        public bool Update(SitePage model)
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

                this.Context.SitePage.Update(model);
                this.Context.SaveChanges();

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
                                   .Skip(quantityPerPage * (pageNumber - 1))
                                   .Take(quantityPerPage)
                                   .ToList();

                total = this.Context.SitePage.Where(x => x.IsLive == true &&
                                               x.PublishDateTimeUtc < now &&
                                               x.SitePageSectionId == sectionId).Count();

                return model;
            }
            catch (Exception ex)
            {
                Log.Fatal(ex);

                throw new Exception(StringConstants.DBErrorMessage, ex.InnerException);
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
    }
}