using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using log4net;
using WebPagePub.Data.DbContextInfo;
using WebPagePub.Data.Models.Db;
using WebPagePub.Data.Repositories.Interfaces;

namespace WebPagePub.Data.Repositories.Implementations
{
    public class RedirectPathRepository : IRedirectPathRepository
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public RedirectPathRepository(IApplicationDbContext context)
        {
            this.Context = context;
        }

        public IApplicationDbContext Context { get; private set; }

        public RedirectPath Create(RedirectPath model)
        {
            try
            {
                this.Context.RedirectPath.Add(model);
                this.Context.SaveChanges();

                return model;
            }
            catch (Exception ex)
            {
                Log.Fatal(ex);
                throw new Exception("DB error", ex.InnerException);
            }
        }

        public bool Delete(int redirectPathId)
        {
            try
            {
                var entry = this.Context.RedirectPath
                                   .FirstOrDefault(x => x.RedirectPathId == redirectPathId);

                this.Context.RedirectPath.Remove(entry);
                this.Context.SaveChanges();

                return true;
            }
            catch (Exception ex)
            {
                Log.Fatal(ex);

                return false;
            }
        }

        public void Dispose()
        {
            this.Context.Dispose();
        }

        public RedirectPath Get(int redirectPathId)
        {
            try
            {
                return this.Context.RedirectPath.FirstOrDefault(x => x.RedirectPathId == redirectPathId);
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
                return this.Context.RedirectPath.FirstOrDefault(x => x.Path == path);
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
                return this.Context.RedirectPath.ToList();
            }
            catch (Exception ex)
            {
                throw new Exception("DB error", ex.InnerException);
            }
        }
    }
}
