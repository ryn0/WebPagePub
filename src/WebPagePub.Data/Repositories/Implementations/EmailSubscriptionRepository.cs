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
    public class EmailSubscriptionRepository : IEmailSubscriptionRepository
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public EmailSubscriptionRepository(IApplicationDbContext context)
        {
            this.Context = context;
        }

        public IApplicationDbContext Context { get; private set; }

        public EmailSubscription Create(EmailSubscription model)
        {
            try
            {
                this.Context.EmailSubscription.Add(model);
                this.Context.SaveChanges();

                return model;
            }
            catch (Exception ex)
            {
                Log.Fatal(ex);
                throw new Exception(StringConstants.DBErrorMessage, ex.InnerException);
            }
        }

        public EmailSubscription Get(int emailSubscriptionId)
        {
            try
            {
                return this.Context.EmailSubscription.Find(emailSubscriptionId);
            }
            catch (Exception ex)
            {
                throw new Exception(StringConstants.DBErrorMessage, ex.InnerException);
            }
        }

        public EmailSubscription Get(string email)
        {
            try
            {
                return this.Context.EmailSubscription
                    .AsNoTracking()
                    .FirstOrDefault(x => x.Email == email);
            }
            catch (Exception ex)
            {
                Log.Fatal(ex);
                throw new Exception(StringConstants.DBErrorMessage, ex.InnerException);
            }
        }

        public bool Update(EmailSubscription model)
        {
            try
            {
                this.Context.EmailSubscription.Update(model);
                this.Context.SaveChanges();

                return true;
            }
            catch (Exception ex)
            {
                Log.Fatal(ex);
                throw new Exception(StringConstants.DBErrorMessage, ex.InnerException);
            }
        }

        public bool Delete(int emailSubscriptionId)
        {
            try
            {
                var entry = this.Context.EmailSubscription.Find(emailSubscriptionId);

                if (entry == null)
                {
                    return false;
                }

                this.Context.EmailSubscription.Remove(entry);
                this.Context.SaveChanges();

                return true;
            }
            catch (Exception ex)
            {
                Log.Fatal(ex);
                return false;
            }
        }

        public IList<EmailSubscription> GetAll()
        {
            try
            {
                return this.Context.EmailSubscription.AsNoTracking().ToList();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex);
                throw new Exception(StringConstants.DBErrorMessage, ex.InnerException);
            }
        }

        public IEnumerable<EmailSubscription> GetPaged(int pageNumber, int pageSize, out int totalItems)
        {
            try
            {
                var query = this.Context.EmailSubscription
                    .AsNoTracking()
                    .OrderByDescending(x => x.CreateDate);

                totalItems = query.Count();

                return query
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex);
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
