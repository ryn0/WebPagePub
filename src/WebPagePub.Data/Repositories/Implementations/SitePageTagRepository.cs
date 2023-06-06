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
    public class SitePageTagRepository : ISitePageTagRepository
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public SitePageTagRepository(IApplicationDbContext context)
        {
            this.Context = context;
        }

        public IApplicationDbContext Context { get; private set; }

        public void Dispose()
        {
            this.Context.Dispose();
        }

        public bool Delete(int tagId, int sitePageId)
        {
            try
            {
                var entry = this.Context.SitePageTag.FirstOrDefault(x => x.TagId == tagId && x.SitePageId == sitePageId);

                this.Context.SitePageTag.Remove(entry);
                this.Context.SaveChanges();

                return true;
            }
            catch (Exception ex)
            {
                Log.Fatal(ex);

                return false;
            }
        }

        public SitePageTag Create(SitePageTag model)
        {
            try
            {
                this.Context.SitePageTag.Add(model);
                this.Context.SaveChanges();

                return model;
            }
            catch (Exception ex)
            {
                Log.Fatal(ex);
                throw new Exception("DB error", ex.InnerException);
            }
        }

        public bool Update(SitePageTag model)
        {
            try
            {
                this.Context.SitePageTag.Update(model);
                this.Context.SaveChanges();

                return true;
            }
            catch (Exception ex)
            {
                Log.Fatal(ex);
                throw new Exception("DB error", ex.InnerException);
            }
        }

        public SitePageTag Get(int tagId, int sitePageId)
        {
            try
            {
                return this.Context.SitePageTag.FirstOrDefault(x => x.TagId == tagId && x.SitePageId == sitePageId);
            }
            catch (Exception ex)
            {
                throw new Exception("DB error", ex.InnerException);
            }
        }

        public List<SitePageTag> GetTagsForBlog(int sitePageId)
        {
            try
            {
                return this.Context.SitePageTag.Where(x => x.SitePageId == sitePageId).ToList();
            }
            catch (Exception ex)
            {
                throw new Exception("DB error", ex.InnerException);
            }
        }

        public List<SitePageTag> GetAll()
        {
            try
            {
                return this.Context.SitePageTag.ToList();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex);
                throw new Exception("DB error", ex.InnerException);
            }
        }
    }
}
