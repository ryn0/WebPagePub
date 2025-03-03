﻿using System;
using System.Collections.Generic;
using WebPagePub.Data.DbContextInfo.Interfaces;
using WebPagePub.Data.Models.Db;

namespace WebPagePub.Data.Repositories.Interfaces
{
    public interface IEmailSubscriptionRepository : IDisposable
    {
        IApplicationDbContext Context { get; }

        EmailSubscription Create(EmailSubscription model);

        bool Update(EmailSubscription model);

        EmailSubscription Get(int emailSubscriptionId);

        EmailSubscription Get(string email);

        IList<EmailSubscription> GetAll();

        bool Delete(int emailSubscriptionId);
        IEnumerable<EmailSubscription> GetPaged(int pageNumber, int pageSize, out int totalItems);
    }
}
