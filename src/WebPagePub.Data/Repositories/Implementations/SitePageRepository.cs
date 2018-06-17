using WebPagePub.Data.Repositories.Interfaces;
using System;
using WebPagePub.Data.Models;
using WebPagePub.Data.DbContextInfo;
using System.Linq;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using log4net;
using System.Reflection;

namespace WebPagePub.Data.Repositories.Implementations
{
    public class SitePageRepository : ISitePageRepository
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public SitePageRepository(IApplicationDbContext context)
        {
            Context = context;
        }

        public IApplicationDbContext Context { get; private set; }
 
        public SitePage Create(SitePage model)
        {
            try
            {
                Context.SitePage.Add(model);
                Context.SaveChanges();

                return model;
            }
            catch (Exception ex)
            {
                log.Fatal(ex);

                throw new Exception("DB error", ex.InnerException);

            }
        }

        public List<SitePage> GetPage(int pageNumber, int sitePageSectionId, int quantityPerPage, out int total)
        {
            try
            {
                var model = Context.SitePage
                                   .Where(x => x.SitePageSectionId == sitePageSectionId)
                                   .OrderBy(page => page.Title)
                                   .Skip(quantityPerPage * (pageNumber - 1))
                                   .Take(quantityPerPage)
                                   .Include(x => x.SitePageSection)
                                   .ToList();

                total = Context.SitePage.Where(x => x.SitePageSectionId == sitePageSectionId).Count();
             
                return model;
            }
            catch (Exception ex)
            {
                log.Fatal(ex);

                throw new Exception("DB error", ex.InnerException);
            }
        }

        public List<SitePage> GetLivePage(int pageNumber, int quantityPerPage, out int total)
        {
            var now = DateTime.UtcNow;

            try
            {
                var model = Context.SitePage
                                   .Where(x => x.IsLive == true && x.PublishDateTimeUtc < now)
                                   .Include(x => x.SitePageSection)
                                   .Include(x => x.Photos)
                                   .Include(x => x.SitePageTags)
                                   .Include("SitePageTags.Tag")
                                   .OrderByDescending(blog => blog.PublishDateTimeUtc)
                                   .Skip(quantityPerPage * (pageNumber - 1))
                                   .Take(quantityPerPage)
                                   .ToList();

                total = Context.SitePage.Where(x => x.IsLive == true && x.PublishDateTimeUtc < now).Count();

                return model;
            }
            catch (Exception ex)
            {
                log.Fatal(ex);

                throw new Exception("DB error", ex.InnerException);
            }
        }

        public SitePage GetPreviousEntry(DateTime currentSitePagePublishDateTimeUtc)
        {
            try
            {
                var model = Context.SitePage
                                 
                                   .Where(x => x.PublishDateTimeUtc < currentSitePagePublishDateTimeUtc && x.IsLive == true)
                                   .OrderByDescending(x => x.PublishDateTimeUtc)
                                   .Include(x => x.SitePageSection)
                                   .Include(x => x.Photos)
                                   .Include(x => x.SitePageTags)
                                   .Include("SitePageTags.Tag")
                                   .FirstOrDefault();

                return model;
            }
            catch (Exception ex)
            {
                log.Fatal(ex);

                throw new Exception("DB error", ex.InnerException);
            }
        }

        public SitePage GetNextEntry(DateTime currentSitePagePublishDateTimeUtc)
        {
            try
            {
                var model = Context.SitePage
                                
                                   .Where(x => x.PublishDateTimeUtc > currentSitePagePublishDateTimeUtc && x.IsLive == true)
                                   .OrderBy(x => x.PublishDateTimeUtc)
                                      .Include(x => x.Photos)
                                   .Include(x => x.SitePageTags)
                                   .Include("SitePageTags.Tag")
                                   .FirstOrDefault();

                return model;
            }
            catch (Exception ex)
            {
                log.Fatal(ex);

                throw new Exception("DB error", ex.InnerException);
            }
        }


        public List<SitePage> GetLivePageByTag(string tagKey, int pageNumber, int quantityPerPage, out int total)
        {
            var now = DateTime.UtcNow;

            try
            {
                var model = Context.SitePage
                                   .Where(x => x.IsLive == true && 
                                               x.PublishDateTimeUtc < now && 
                                               (x.SitePageTags.FirstOrDefault(y => y.Tag.Key == tagKey) != null))
                                   .Include(x => x.SitePageSection)
                                   .Include(x => x.Photos)
                                   .Include(x => x.SitePageTags)
                                   .Include("SitePageTags.Tag")
                                   .OrderByDescending(blog => blog.PublishDateTimeUtc)
                                   .Skip(quantityPerPage * (pageNumber - 1))
                                   .Take(quantityPerPage)
                                   .ToList();

                total = Context.SitePage.Where(x => x.IsLive == true &&
                                               x.PublishDateTimeUtc < now &&
                                               (x.SitePageTags.FirstOrDefault(y => y.Tag.Key == tagKey) != null)).Count();

                return model;
            }
            catch (Exception ex)
            {
                log.Fatal(ex);

                throw new Exception("DB error", ex.InnerException);
            }

        }

        public void Dispose()
        {
            Context.Dispose();
        }

        public SitePage Get(int sitePageId)
        {
            try
            {
                return Context.SitePage
                              .Include(x => x.SitePageSection)
                              .Include(x => x.Photos)
                              .Include(x => x.SitePageTags)
                              .Include("SitePageTags.Tag")
                              .FirstOrDefault(x => x.SitePageId == sitePageId);
            }
            catch (Exception ex)
            {
                throw new Exception("DB error", ex.InnerException);
            }
        }

        public SitePage Get(string key)
        {
            try
            {
                return Context.SitePage
                              .Include(x => x.SitePageSection)
                              .Include(x => x.Photos)
                              .Include(x => x.SitePageTags)
                              .Include("SitePageTags.Tag")
                              .FirstOrDefault(x => x.Key == key);
            }
            catch (Exception ex)
            {
                log.Fatal(ex);

                throw new Exception("DB error", ex.InnerException);

            }
        }

        public bool Update(SitePage model)
        {
            try
            {
                if (model.IsSectionHomePage)
                {
                    foreach (var page in Context.SitePage
                                                .Where(x =>x.SitePageSectionId == model.SitePageSectionId)
                                                .ToList())
                    {
                        page.IsSectionHomePage = false;

                        if (page.SitePageId == model.SitePageId)
                        {
                            page.IsSectionHomePage = true;
                        }
                    }
                }

                Context.SitePage.Update(model);
                Context.SaveChanges();

                return true;
            }

            catch (Exception ex)
            {
                log.Fatal(ex);

                throw new Exception("DB error", ex.InnerException);

            }
        }

        public bool Delete(int SitePageId)
        {
            try
            {
                var entry = Context.SitePage
                              .FirstOrDefault(x => x.SitePageId == SitePageId);

                Context.SitePage.Remove(entry);
                Context.SaveChanges();

                return true;
            }
            catch (Exception ex)
            {

                log.Fatal(ex);

                return false;
            }
        }

        public SitePage Get(int sitePageSectionId, string key)
        {
            try
            {
                return Context.SitePage
                              .Include(x => x.SitePageSection)
                              .Include(x => x.Photos)
                              .Include(x => x.SitePageTags)
                              .Include("SitePageTags.Tag")
                              .FirstOrDefault(x => x.SitePageSectionId == sitePageSectionId && x.Key == key);
            }
            catch (Exception ex)
            {
                throw new Exception("DB error", ex.InnerException);
            }
        }

        public List<SitePage> GetLivePagesForSection(int sitePageSectionId)
        {
            try
            {
                return Context.SitePage
                              .Include(x => x.SitePageSection)
                              .Include(x => x.Photos)
                              .Include(x => x.SitePageTags)
                              .Include("SitePageTags.Tag")
                              .Where(x => x.SitePageSectionId == sitePageSectionId && x.IsLive == true)
                              .OrderByDescending(x => x.PublishDateTimeUtc)
                              .ToList();
            }
            catch (Exception ex)
            {
                throw new Exception("DB error", ex.InnerException);
            }
        }

        public List<SitePage> GetLivePageBySection(int sectionId, int pageNumber, int quantityPerPage, out int total)
        {
            var now = DateTime.UtcNow;
            try
            {
                var model = Context.SitePage
                                   .Where(x => x.IsLive == true &&
                                               x.PublishDateTimeUtc < now &&
                                               x.SitePageSectionId == sectionId)
                                   .Include(x => x.SitePageSection)
                                   .Include(x => x.Photos)
                                   .Include(x => x.SitePageTags)
                                   .Include("SitePageTags.Tag")
                                   .OrderByDescending(blog => blog.PublishDateTimeUtc)
                                   .Skip(quantityPerPage * (pageNumber - 1))
                                   .Take(quantityPerPage)
                                   .ToList();

                total = Context.SitePage.Where(x => x.IsLive == true &&
                                               x.PublishDateTimeUtc < now &&
                                               x.SitePageSectionId == sectionId).Count();

                return model;
            }
            catch (Exception ex)
            {
                log.Fatal(ex);

                throw new Exception("DB error", ex.InnerException);
            }

        }

        public SitePage GetSectionHomePage(int sitePageSectionId)
        {
            try
            {
                return Context.SitePage
                              .FirstOrDefault(x => x.IsSectionHomePage == true && 
                                                   x.SitePageSectionId == sitePageSectionId);
            }
            catch (Exception ex)
            {
                log.Fatal(ex);

                throw new Exception("DB error", ex.InnerException);
            }
        }
    }
}

