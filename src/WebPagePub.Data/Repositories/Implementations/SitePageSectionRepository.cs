﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using log4net;
using WebPagePub.Data.Constants;
using WebPagePub.Data.DbContextInfo.Interfaces;
using WebPagePub.Data.Models.Db;
using WebPagePub.Data.Repositories.Interfaces;

namespace WebPagePub.Data.Repositories.Implementations
{
    public class SitePageSectionRepository : ISitePageSectionRepository
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public SitePageSectionRepository(IApplicationDbContext context)
        {
            this.Context = context;
        }

        public IApplicationDbContext Context { get; private set; }

        public SitePageSection Create(SitePageSection model)
        {
            try
            {
                this.Context.SitePageSection.Add(model);
                this.Context.SaveChanges();

                return model;
            }
            catch (Exception ex)
            {
                Log.Fatal(ex);

                throw new Exception(StringConstants.DBErrorMessage, ex.InnerException);
            }
        }

        public bool Delete(int sitePageSectionId)
        {
            try
            {
                var entry = this.Context.SitePageSection.Find(sitePageSectionId);

                this.Context.SitePageSection.Remove(entry);
                this.Context.SaveChanges();

                return true;
            }
            catch (Exception ex)
            {
                Log.Fatal(ex);

                return false;
            }
        }

        public void Dispose()
        {
            this.Context.Dispose();
        }

        public SitePageSection Get(int sitePageSectionId)
        {
            try
            {
                return this.Context.SitePageSection.Find(sitePageSectionId);
            }
            catch (Exception ex)
            {
                throw new Exception(StringConstants.DBErrorMessage, ex.InnerException);
            }
        }

        public SitePageSection Get(string key)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(key))
                {
                    return null;
                }

                return this.Context.SitePageSection
                              .FirstOrDefault(x => x.Key == key);
            }
            catch (Exception ex)
            {
                throw new Exception(StringConstants.DBErrorMessage, ex.InnerException);
            }
        }

        public IList<SitePageSection> GetAll()
        {
            try
            {
                return this.Context.SitePageSection.ToList();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex);

                throw new Exception(StringConstants.DBErrorMessage, ex.InnerException);
            }
        }

        public SitePageSection GetHomeSection()
        {
            try
            {
                return this.Context.SitePageSection
                              .FirstOrDefault(x => x.IsHomePageSection == true);
            }
            catch (Exception ex)
            {
                Log.Fatal(ex);

                throw new Exception(StringConstants.DBErrorMessage, ex.InnerException);
            }
        }

        public bool Update(SitePageSection model)
        {
            try
            {
                if (model.IsHomePageSection)
                {
                    foreach (var page in this.Context.SitePageSection.ToList())
                    {
                        page.IsHomePageSection = false;

                        if (page.SitePageSectionId == model.SitePageSectionId)
                        {
                            page.IsHomePageSection = true;
                        }
                    }
                }

                this.Context.SitePageSection.Update(model);
                this.Context.SaveChanges();

                return true;
            }
            catch (Exception ex)
            {
                Log.Fatal(ex);

                throw new Exception(StringConstants.DBErrorMessage, ex.InnerException);
            }
        }
    }
}
