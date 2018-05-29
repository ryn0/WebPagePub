using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using WebPagePub.Data.DbContextInfo;
using WebPagePub.Data.Models.Db;
using WebPagePub.Data.Repositories.Interfaces;

namespace WebPagePub.Data.Repositories.Implementations
{
    public class RedirectPathRepository : IRedirectPathRepository
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public RedirectPathRepository(IApplicationDbContext context)
        {
            Context = context;
        }

        public IApplicationDbContext Context { get; private set; }

        public RedirectPath Create(RedirectPath model)
        {
            try
            {
                Context.RedirectPath.Add(model);
                Context.SaveChanges();

                return model;
            }
            catch (Exception ex)
            {
                log.Fatal(ex);
                throw new Exception("DB error", ex.InnerException);
            }
        }

        public bool Delete(int redirectPathId)
        {
            try
            {
                var entry = Context.RedirectPath
                                   .FirstOrDefault(x => x.RedirectPathId == redirectPathId);

                Context.RedirectPath.Remove(entry);
                Context.SaveChanges();

                return true;
            }
            catch (Exception ex)
            {
                log.Fatal(ex);

                return false;
            }
        }

        public void Dispose()
        {
            Context.Dispose();
        }

        public RedirectPath Get(int redirectPathId)
        {
            try
            {
                return Context.RedirectPath.FirstOrDefault(x => x.RedirectPathId == redirectPathId);
            }
            catch (Exception ex)
            {
                throw new Exception("DB error", ex.InnerException);
            }
        }

        public RedirectPath Get(string path)
        {
            try
            {
                return Context.RedirectPath.FirstOrDefault(x => x.Path == path);
            }
            catch (Exception ex)
            {
                throw new Exception("DB error", ex.InnerException);
            }
        }

        public List<RedirectPath> GetAll()
        {
            try
            {
                return Context.RedirectPath.ToList();
            }
            catch (Exception ex)
            {
                throw new Exception("DB error", ex.InnerException);
            }
        }
    }
}
