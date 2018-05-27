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
    public class EmailSubscriptionRepository : IEmailSubscriptionRepository
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public EmailSubscriptionRepository(IApplicationDbContext context)
        {
            Context = context;
        }

        public IApplicationDbContext Context { get; private set; }

        public EmailSubscription Create(EmailSubscription model)
        {
            try
            {
                Context.EmailSubscription.Add(model);
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

        public EmailSubscription Get(int emailSubscriptionId)
        {
            try
            {
                return Context.EmailSubscription.FirstOrDefault(x => x.EmailSubscriptionId == emailSubscriptionId);
            }
            catch (Exception ex)
            {
                throw new Exception("DB error", ex.InnerException);
            }
        }

        public EmailSubscription Get(string email)
        {
            try
            {
                return Context.EmailSubscription.FirstOrDefault(x => x.Email == email);
            }
            catch (Exception ex)
            {
                log.Fatal(ex);
                throw new Exception("DB error", ex.InnerException);
            }
        }

        public bool Update(EmailSubscription model)
        {
            try
            {
                Context.EmailSubscription.Update(model);
                Context.SaveChanges();

                return true;
            }

            catch (Exception ex)
            {
                log.Fatal(ex);
                throw new Exception("DB error", ex.InnerException);

            }
        }

        public bool Delete(int emailSubscriptionId)
        {
            try
            {
                var entry = Context.EmailSubscription.FirstOrDefault(x => x.EmailSubscriptionId == emailSubscriptionId);

                Context.EmailSubscription.Remove(entry);
                Context.SaveChanges();

                return true;
            }
            catch (Exception ex)
            {
                log.Fatal(ex);

                return false;
            }
        }

        public List<EmailSubscription> GetAll()
        {
            try
            {
                return Context.EmailSubscription.ToList();
            }
            catch (Exception ex)
            {
                log.Fatal(ex);
                throw new Exception("DB error", ex.InnerException);
            }
        }
    }
}
