using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using log4net;
using Microsoft.EntityFrameworkCore;
using WebPagePub.Data.Constants;
using WebPagePub.Data.DbContextInfo.Interfaces;
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
                throw new Exception(StringConstants.DBErrorMessage, ex.InnerException);
            }
        }

        public bool Delete(int redirectPathId)
        {
            try
            {
                var entry = this.Context.RedirectPath.Find(redirectPathId);

                // Guard: Find returns null when the record does not exist.
                // The original passed null directly to Remove, causing an
                // ArgumentNullException with no useful context.
                if (entry == null)
                {
                    return false;
                }

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

        public RedirectPath Get(int redirectPathId)
        {
            try
            {
                return this.Context.RedirectPath.Find(redirectPathId);
            }
            catch (Exception ex)
            {
                throw new Exception(StringConstants.DBErrorMessage, ex.InnerException);
            }
        }

        public RedirectPath Get(string path)
        {
            try
            {
                return this.Context.RedirectPath
                    .AsNoTracking()
                    .FirstOrDefault(x => x.Path == path);
            }
            catch (Exception ex)
            {
                throw new Exception(StringConstants.DBErrorMessage, ex.InnerException);
            }
        }

        public IList<RedirectPath> GetAll()
        {
            try
            {
                return this.Context.RedirectPath.AsNoTracking().ToList();
            }
            catch (Exception ex)
            {
                throw new Exception(StringConstants.DBErrorMessage, ex.InnerException);
            }
        }

        // The context is registered via AddDbContextPool in Program.cs — the DI
        // container owns its lifetime. Calling Context.Dispose() here would return
        // the context to the pool while other scoped services may still hold a
        // reference to the same instance, causing use-after-dispose errors.
        public void Dispose()
        {
            // Intentionally empty. Context lifetime is managed by the DI container.
        }
    }
}
