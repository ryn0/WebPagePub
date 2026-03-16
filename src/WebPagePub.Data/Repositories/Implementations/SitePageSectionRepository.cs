using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using log4net;
using Microsoft.EntityFrameworkCore;
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

                // FIX: Find returns null when the record does not exist. Passing null
                // to Remove throws ArgumentNullException with no useful context.
                if (entry == null)
                {
                    return false;
                }

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
                    .AsNoTracking()
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
                return this.Context.SitePageSection.AsNoTracking().ToList();
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
                    .AsNoTracking()
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
                    // FIX: The original loaded EVERY section in the table as tracked
                    // entities just to clear IsHomePageSection on all of them, then
                    // called SaveChanges, generating an UPDATE for every row.
                    //
                    // Only sections that are currently marked as the home page section
                    // (and are not the section we're about to set) actually need
                    // updating. In practice this is at most one row, not a full table
                    // scan. Load only those rows, flip them, and let SaveChanges
                    // generate a single targeted UPDATE.
                    foreach (var other in this.Context.SitePageSection
                        .Where(x => x.IsHomePageSection && x.SitePageSectionId != model.SitePageSectionId)
                        .ToList())
                    {
                        other.IsHomePageSection = false;
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

        // The context is registered via AddDbContextPool in Program.cs — the DI
        // container owns its lifetime. Calling Context.Dispose() here would return
        // the context to the pool while other scoped services may still hold a
        // reference to the same instance, causing use-after-dispose errors.
        public void Dispose()
        {
            // Intentionally empty. Context lifetime is managed by the DI container.
        }
    }
}
