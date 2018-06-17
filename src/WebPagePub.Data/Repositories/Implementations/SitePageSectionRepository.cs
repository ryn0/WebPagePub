using WebPagePub.Data.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using WebPagePub.Data.DbContextInfo;
using log4net;
using System.Reflection;
using WebPagePub.Data.Models.Db;
using System.Linq;

namespace WebPagePub.Data.Repositories.Implementations
{
    public class SitePageSectionRepository : ISitePageSectionRepository
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public SitePageSectionRepository(IApplicationDbContext context)
        {
            Context = context;
        }

        public IApplicationDbContext Context { get; private set; }

        public SitePageSection Create(SitePageSection model)
        {
            try
            {
                Context.SitePageSection.Add(model);
                Context.SaveChanges();

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

        public SitePageSection Get(int sitePageSectionId)
        {
            try
            {
                return Context.SitePageSection
                              .FirstOrDefault(x => x.SitePageSectionId == sitePageSectionId);
            }
            catch (Exception ex)
            {
                throw new Exception("DB error", ex.InnerException);
            }
        }

        public SitePageSection Get(string key)
        {
            try
            {
                return Context.SitePageSection
                              .FirstOrDefault(x => x.Key == key);
            }
            catch (Exception ex)
            {
                throw new Exception("DB error", ex.InnerException);
            }
        }

        public List<SitePageSection> GetAll()
        {
            try
            {
                return Context.SitePageSection.ToList();
            }
            catch (Exception ex)
            {
                log.Fatal(ex);

                throw new Exception("DB error", ex.InnerException);

            }
        }

        public SitePageSection GetHomeSection()
        {
            try
            {
                return Context.SitePageSection
                              .FirstOrDefault(x => x.IsHomePageSection == true);
            }
            catch (Exception ex)
            {
                log.Fatal(ex);

                throw new Exception("DB error", ex.InnerException);
            }
        }

        public bool Update(SitePageSection model)
        {
            try
            {
                if (model.IsHomePageSection)
                {
                    foreach (var page in Context.SitePageSection.ToList())
                    {
                        page.IsHomePageSection = false;

                        if (page.SitePageSectionId == model.SitePageSectionId)
                        {
                            page.IsHomePageSection = true;
                        }
                    }
                }

                Context.SitePageSection.Update(model);
                Context.SaveChanges();

                return true;
            }
            catch (Exception ex)
            {
                log.Fatal(ex);

                throw new Exception("DB error", ex.InnerException);

            }
        }
    }
}
