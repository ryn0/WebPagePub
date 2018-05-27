using WebPagePub.Data.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using WebPagePub.Data.DbContextInfo;
using WebPagePub.Data.Models;
using System.Linq;
using log4net;
using System.Reflection;

namespace WebPagePub.Data.Repositories.Implementations
{
    public class SitePagePhotoRepository : ISitePagePhotoRepository
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public IApplicationDbContext Context { get; set; }

        public SitePagePhotoRepository(IApplicationDbContext context)
        {
            Context = context;
        }

        public SitePagePhoto Create(SitePagePhoto model)
        {
            try
            {
                Context.SitePagePhoto.Add(model);
                Context.SaveChanges();

                return model;
            }
            catch (Exception ex)
            {
                log.Fatal(ex);
                throw new Exception("DB error", ex.InnerException);
            }
        }

        public void SetDefaultPhoto(int sitePagePhotoId)
        {
            try
            {
                var photoEntry = Get(sitePagePhotoId);

                foreach (var photo in Context.SitePagePhoto
                                             .Where(x => x.SitePageId == photoEntry.SitePageId)
                                             .ToList())
                {
                    photo.IsDefault = false;

                    if (photo.SitePagePhotoId == sitePagePhotoId)
                    {
                        photo.IsDefault = true;
                    }
                }

                Context.SaveChanges();
            }
            catch (Exception ex)
            {
                log.Fatal(ex);
                throw new Exception("DB error", ex.InnerException);
            }
        }

        public bool Delete(int sitePagePhotoId)
        {
            try
            {
                var entry = Context.SitePagePhoto
                                   .FirstOrDefault(x => x.SitePagePhotoId == sitePagePhotoId);

                Context.SitePagePhoto.Remove(entry);
                Context.SaveChanges();

                return true;
            }
            catch (Exception ex)
            {
                log.Fatal(ex);

                return false;
            }
        }
 
        public SitePagePhoto Get(int sitePagePhotoId)
        {
            try
            {
                return Context.SitePagePhoto
                              .FirstOrDefault(x => x.SitePagePhotoId == sitePagePhotoId);
            }
            catch (Exception ex)
            {
                log.Fatal(ex);
                throw new Exception("DB error", ex.InnerException);
            }
        }

        public List<SitePagePhoto> GetBlogPhotos(int sitePageId)
        {
            try
            {
                return Context.SitePagePhoto
                              .Where(x => x.SitePageId == sitePageId)
                              .ToList();
            }
            catch (Exception ex)
            {
                log.Fatal(ex);
                throw new Exception("DB error", ex.InnerException);
            }
        }

        public bool Update(SitePagePhoto model)
        {
            try
            {
                Context.SitePagePhoto.Update(model);
                Context.SaveChanges();

                return true;
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
    }
}
