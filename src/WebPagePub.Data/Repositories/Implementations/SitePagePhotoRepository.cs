using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using log4net;
using WebPagePub.Data.DbContextInfo;
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

        public IApplicationDbContext Context { get; set; }

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
                throw new Exception("DB error", ex.InnerException);
            }
        }

        public void SetDefaultPhoto(int sitePagePhotoId)
        {
            try
            {
                var photoEntry = this.Get(sitePagePhotoId);

                foreach (var photo in this.Context.SitePagePhoto
                                             .Where(x => x.SitePageId == photoEntry.SitePageId)
                                             .ToList())
                {
                    photo.IsDefault = false;

                    if (photo.SitePagePhotoId == sitePagePhotoId)
                    {
                        photo.IsDefault = true;
                    }
                }

                this.Context.SaveChanges();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex);
                throw new Exception("DB error", ex.InnerException);
            }
        }

        public bool Delete(int sitePagePhotoId)
        {
            try
            {
                var entry = this.Context.SitePagePhoto
                                   .FirstOrDefault(x => x.SitePagePhotoId == sitePagePhotoId);

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
                return this.Context.SitePagePhoto
                              .FirstOrDefault(x => x.SitePagePhotoId == sitePagePhotoId);
            }
            catch (Exception ex)
            {
                Log.Fatal(ex);
                throw new Exception("DB error", ex.InnerException);
            }
        }

        public List<SitePagePhoto> GetBlogPhotos(int sitePageId)
        {
            try
            {
                return this.Context.SitePagePhoto
                              .Where(x => x.SitePageId == sitePageId)
                              .ToList();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex);
                throw new Exception("DB error", ex.InnerException);
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
                throw new Exception("DB error", ex.InnerException);
            }
        }

        public void Dispose()
        {
            this.Context.Dispose();
        }
    }
}
