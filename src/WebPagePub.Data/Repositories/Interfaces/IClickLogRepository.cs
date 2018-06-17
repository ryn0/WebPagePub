using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WebPagePub.Data.DbContextInfo;
using WebPagePub.Data.Models.Db;

namespace WebPagePub.Data.Repositories.Interfaces
{
    public interface IClickLogRepository : IDisposable
    {
        IApplicationDbContext Context { get; }

        Task<ClickLog> CreateAsync(ClickLog model);

        List<ClickLog> GetClicksInRange(DateTime startDate, DateTime endDate);
    }
}
