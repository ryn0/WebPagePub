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

        public void Dispose()
        {
            this.Context.Dispose();
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
                return this.Context.EmailSubscription.FirstOrDefault(x => x.Email == email);
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
                return this.Context.EmailSubscription.ToList();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex);
                throw new Exception(StringConstants.DBErrorMessage, ex.InnerException);
            }
        }
    }
}
