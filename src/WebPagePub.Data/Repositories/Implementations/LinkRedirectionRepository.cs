using WebPagePub.Data.Repositories.Interfaces;
using System;
using WebPagePub.Data.DbContextInfo;
using System.Linq;
using WebPagePub.Data.Models.Db;
using System.Collections.Generic;
using log4net;
using System.Reflection;

namespace WebPagePub.Data.Repositories.Implementations
{
    public class LinkRedirectionRepository : ILinkRedirectionRepository
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public LinkRedirectionRepository(IApplicationDbContext context)
        {
            Context = context;
        }

        public IApplicationDbContext Context { get; private set; }

        public LinkRedirection Create(LinkRedirection model)
        {
            try
            {
                Context.LinkRedirection.Add(model);
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

        public LinkRedirection Get(int linkRedirectionId)
        {
            try
            {
                return Context.LinkRedirection.FirstOrDefault(x => x.LinkRedirectionId == linkRedirectionId);
            }
            catch (Exception ex)
            {
                throw new Exception("DB error", ex.InnerException);
            }
        }

        public LinkRedirection Get(string key)
        {
            try
            {
                return Context.LinkRedirection.FirstOrDefault(x => x.LinkKey == key);
            }
            catch (Exception ex)
            {
                log.Fatal(ex);
                throw new Exception("DB error", ex.InnerException);

            }
        }

        public bool Update(LinkRedirection model)
        {
            try
            {
                Context.LinkRedirection.Update(model);
                Context.SaveChanges();

                return true;
            }

            catch (Exception ex)
            {
                log.Fatal(ex);
                throw new Exception("DB error", ex.InnerException);

            }
        }

        public bool Delete(int linkRedirectionId)
        {
            try
            {
                var entry = Context.LinkRedirection.FirstOrDefault(x => x.LinkRedirectionId == linkRedirectionId);

                Context.LinkRedirection.Remove(entry);
                Context.SaveChanges();

                return true;
            }
            catch (Exception ex)
            {
                log.Fatal(ex);

                return false;
            }
        }

        public List<LinkRedirection> GetAll()
        {
            try
            {
                return Context.LinkRedirection.ToList();
            }
            catch (Exception ex)
            {
                log.Fatal(ex);
                throw new Exception("DB error", ex.InnerException);
            }
        }
    }
}
