using WebPagePub.Data.Repositories.Interfaces;
using System;
using WebPagePub.Data.DbContextInfo;
using System.Linq;
using WebPagePub.Data.Enums;
using WebPagePub.Data.Models.Db;
using System.Collections.Generic;
using log4net;
using System.Reflection;

namespace WebPagePub.Data.Repositories.Implementations
{
    public class ContentSnippetRepository : IContentSnippetRepository
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public ContentSnippetRepository(IApplicationDbContext context)
        {
            Context = context;
        }

        public IApplicationDbContext Context { get; private set; }

        public ContentSnippet Create(ContentSnippet model)
        {
            try
            {
                Context.ContentSnippet.Add(model);
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

        public ContentSnippet Get(int contentSnippetId)
        {
            try
            {
                return Context.ContentSnippet.FirstOrDefault(x => x.ContentSnippetId == contentSnippetId);
            }
            catch (Exception ex)
            {
                throw new Exception("DB error", ex.InnerException);
            }
        }

        public ContentSnippet Get(SiteConfigSetting snippetType)
        {
            try
            {
                return Context.ContentSnippet.FirstOrDefault(x => x.SnippetType == snippetType);
            }
            catch (Exception ex)
            {
                log.Fatal(ex);
                throw new Exception("DB error", ex.InnerException);

            }
        }

        public bool Update(ContentSnippet model)
        {
            try
            {
                Context.ContentSnippet.Update(model);
                Context.SaveChanges();

                return true;
            }

            catch (Exception ex)
            {
                log.Fatal(ex);
                throw new Exception("DB error", ex.InnerException);

            }
        }

        public bool Delete(int contentSnippetId)
        {
            try
            {
                var entry = Context.ContentSnippet.FirstOrDefault(x => x.ContentSnippetId == contentSnippetId);

                Context.ContentSnippet.Remove(entry);
                Context.SaveChanges();

                return true;
            }
            catch (Exception ex)
            {
                log.Fatal(ex);

                return false;
            }
        }
         
        public List<ContentSnippet> GetAll()
        {
            try
            {
                return Context.ContentSnippet.ToList();
            }
            catch (Exception ex)
            {
                log.Fatal(ex);
                throw new Exception("DB error", ex.InnerException);
            }
        }
    }
}
