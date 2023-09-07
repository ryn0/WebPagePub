using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using log4net;
using WebPagePub.Data.Constants;
using WebPagePub.Data.DbContextInfo;
using WebPagePub.Data.Models.Db;
using WebPagePub.Data.Repositories.Interfaces;

namespace WebPagePub.Data.Repositories.Implementations
{
    public class LinkRedirectionRepository : ILinkRedirectionRepository
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public LinkRedirectionRepository(IApplicationDbContext context)
        {
            this.Context = context;
        }

        public IApplicationDbContext Context { get; private set; }

        public LinkRedirection Create(LinkRedirection model)
        {
            try
            {
                this.Context.LinkRedirection.Add(model);
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

        public LinkRedirection Get(int linkRedirectionId)
        {
            try
            {
                return this.Context.LinkRedirection.FirstOrDefault(x => x.LinkRedirectionId == linkRedirectionId);
            }
            catch (Exception ex)
            {
                throw new Exception(StringConstants.DBErrorMessage, ex.InnerException);
            }
        }

        public LinkRedirection Get(string key)
        {
            try
            {
                return this.Context.LinkRedirection.FirstOrDefault(x => x.LinkKey == key);
            }
            catch (Exception ex)
            {
                Log.Fatal(ex);
                throw new Exception(StringConstants.DBErrorMessage, ex.InnerException);
            }
        }

        public bool Update(LinkRedirection model)
        {
            try
            {
                this.Context.LinkRedirection.Update(model);
                this.Context.SaveChanges();

                return true;
            }
            catch (Exception ex)
            {
                Log.Fatal(ex);
                throw new Exception(StringConstants.DBErrorMessage, ex.InnerException);
            }
        }

        public bool Delete(int linkRedirectionId)
        {
            try
            {
                var entry = this.Context.LinkRedirection.FirstOrDefault(x => x.LinkRedirectionId == linkRedirectionId);

                this.Context.LinkRedirection.Remove(entry);
                this.Context.SaveChanges();

                return true;
            }
            catch (Exception ex)
            {
                Log.Fatal(ex);

                return false;
            }
        }

        public IList<LinkRedirection> GetAll()
        {
            try
            {
                return this.Context.LinkRedirection.ToList();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex);
                throw new Exception(StringConstants.DBErrorMessage, ex.InnerException);
            }
        }
    }
}
