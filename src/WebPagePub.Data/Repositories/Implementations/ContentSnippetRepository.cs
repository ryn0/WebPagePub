using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using log4net;
using WebPagePub.Data.Constants;
using WebPagePub.Data.DbContextInfo;
using WebPagePub.Data.Enums;
using WebPagePub.Data.Models.Db;
using WebPagePub.Data.Repositories.Interfaces;

namespace WebPagePub.Data.Repositories.Implementations
{
    public class ContentSnippetRepository : IContentSnippetRepository
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public ContentSnippetRepository(IApplicationDbContext context)
        {
            this.Context = context;
        }

        public IApplicationDbContext Context { get; private set; }

        public ContentSnippet Create(ContentSnippet model)
        {
            try
            {
                this.Context.ContentSnippet.Add(model);
                this.Context.SaveChanges();

                return model;
            }
            catch (Exception ex)
            {
                Log.Fatal(ex);
                throw new Exception(StringConstants.DBErrorMessage, ex.InnerException);
            }
        }

        public void Dispose()
        {
            this.Context.Dispose();
        }

        public ContentSnippet Get(int contentSnippetId)
        {
            try
            {
                return this.Context.ContentSnippet.FirstOrDefault(x => x.ContentSnippetId == contentSnippetId);
            }
            catch (Exception ex)
            {
                throw new Exception(StringConstants.DBErrorMessage, ex.InnerException);
            }
        }

        public ContentSnippet Get(SiteConfigSetting snippetType)
        {
            try
            {
                return this.Context.ContentSnippet.FirstOrDefault(x => x.SnippetType == snippetType);
            }
            catch (Exception ex)
            {
                Log.Fatal(ex);
                throw new Exception(StringConstants.DBErrorMessage, ex.InnerException);
            }
        }

        public bool Update(ContentSnippet model)
        {
            try
            {
                this.Context.ContentSnippet.Update(model);
                this.Context.SaveChanges();

                return true;
            }
            catch (Exception ex)
            {
                Log.Fatal(ex);
                throw new Exception(StringConstants.DBErrorMessage, ex.InnerException);
            }
        }

        public bool Delete(int contentSnippetId)
        {
            try
            {
                var entry = this.Context.ContentSnippet.FirstOrDefault(x => x.ContentSnippetId == contentSnippetId);

                this.Context.ContentSnippet.Remove(entry);
                this.Context.SaveChanges();

                return true;
            }
            catch (Exception ex)
            {
                Log.Fatal(ex);

                return false;
            }
        }

        public IList<ContentSnippet> GetAll()
        {
            try
            {
                return this.Context.ContentSnippet.ToList();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex);
                throw new Exception(StringConstants.DBErrorMessage, ex.InnerException);
            }
        }
    }
}
