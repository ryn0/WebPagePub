using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using log4net;
using Microsoft.EntityFrameworkCore;
using WebPagePub.Data.Constants;
using WebPagePub.Data.DbContextInfo.Interfaces;
using WebPagePub.Data.Models;
using WebPagePub.Data.Repositories.Interfaces;

namespace WebPagePub.Data.Repositories.Implementations
{
    public class SitePagePhotoRepository : ISitePagePhotoRepository
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public SitePagePhotoRepository(IApplicationDbContext context)
        {
            this.Context = context;
        }

        // FIX: was `public set` — external code should never swap the context out
        // from under a repository that is already in use.
        public IApplicationDbContext Context { get; private set; }

        public SitePagePhoto Create(SitePagePhoto model)
        {
            try
            {
                this.Context.SitePagePhoto.Add(model);
                this.Context.SaveChanges();

                return model;
            }
            catch (Exception ex)
            {
                Log.Fatal(ex);
                throw new Exception(StringConstants.DBErrorMessage, ex.InnerException);
            }
        }

        public void SetDefaultPhoto(int sitePagePhotoId)
        {
            try
            {
                var photoEntry = this.Get(sitePagePhotoId);

                // Tracking is intentional here: we need EF to detect the IsDefault
                // changes on each photo and generate UPDATE statements for all of them.
                foreach (var photo in this.Context.SitePagePhoto
                    .Where(x => x.SitePageId == photoEntry.SitePageId)
                    .ToList())
                {
                    photo.IsDefault = photo.SitePagePhotoId == sitePagePhotoId;
                }

                this.Context.SaveChanges();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex);
                throw new Exception(StringConstants.DBErrorMessage, ex.InnerException);
            }
        }

        public bool Delete(int sitePagePhotoId)
        {
            try
            {
                var entry = this.Context.SitePagePhoto.Find(sitePagePhotoId);

                // FIX: Find returns null when the record does not exist. Passing null
                // to Remove throws ArgumentNullException with no useful context.
                if (entry == null)
                {
                    return false;
                }

                this.Context.SitePagePhoto.Remove(entry);
                this.Context.SaveChanges();

                return true;
            }
            catch (Exception ex)
            {
                Log.Fatal(ex);
                return false;
            }
        }

        public SitePagePhoto Get(int sitePagePhotoId)
        {
            try
            {
                return this.Context.SitePagePhoto.Find(sitePagePhotoId);
            }
            catch (Exception ex)
            {
                Log.Fatal(ex);
                throw new Exception(StringConstants.DBErrorMessage, ex.InnerException);
            }
        }

        public IList<SitePagePhoto> GetBlogPhotos(int sitePageId)
        {
            try
            {
                return this.Context.SitePagePhoto
                    .AsNoTracking()
                    .Where(x => x.SitePageId == sitePageId)
                    .ToList();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex);
                throw new Exception(StringConstants.DBErrorMessage, ex.InnerException);
            }
        }

        public bool Update(SitePagePhoto model)
        {
            try
            {
                this.Context.SitePagePhoto.Update(model);
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
