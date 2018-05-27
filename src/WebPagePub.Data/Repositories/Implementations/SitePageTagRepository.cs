using WebPagePub.Data.Repositories.Interfaces;
using System;
using WebPagePub.Data.DbContextInfo;
using WebPagePub.Data.Models;
using System.Linq;
using System.Collections.Generic;
using log4net;
using System.Reflection;

namespace WebPagePub.Data.Repositories.Implementations
{
    public class SitePageTagRepository : ISitePageTagRepository
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public SitePageTagRepository(IApplicationDbContext context)
        {
            Context = context;
        }

        public IApplicationDbContext Context { get; private set; }
 

        public void Dispose()
        {
            Context.Dispose();
        }

       
        public bool Delete(int tagId, int SitePageId)
        {
            try
            {
                var entry = Context.SitePageTag.FirstOrDefault(x => x.TagId == tagId && x.SitePageId == SitePageId);

                Context.SitePageTag.Remove(entry);
                Context.SaveChanges();

                return true;
            }
            catch (Exception ex)
            {
                log.Fatal(ex);

                return false;
            }
        }

        public SitePageTag Create(SitePageTag model)
        {
            try
            {
                Context.SitePageTag.Add(model);
                Context.SaveChanges();

                return model;
            }
            catch (Exception ex)
            {
                log.Fatal(ex);
                throw new Exception("DB error", ex.InnerException);

            }
        }

        public bool Update(SitePageTag model)
        {
            try
            {
                Context.SitePageTag.Update(model);
                Context.SaveChanges();

                return true;
            }

            catch (Exception ex)
            {
                log.Fatal(ex);
                throw new Exception("DB error", ex.InnerException);

            }
        }

        public SitePageTag Get(int tagId, int SitePageId)
        {
            try
            {
                return Context.SitePageTag.FirstOrDefault(x => x.TagId == tagId && x.SitePageId == SitePageId);
            }
            catch (Exception ex)
            {
                throw new Exception("DB error", ex.InnerException);
            }
        }

        public List<SitePageTag> GetTagsForBlog(int SitePageId)
        {
            try
            {
                return Context.SitePageTag.Where(x => x.SitePageId == SitePageId).ToList();
            }
            catch (Exception ex)
            {
                throw new Exception("DB error", ex.InnerException);
            }
        }
        
    }
}
