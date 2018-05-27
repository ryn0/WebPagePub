using WebPagePub.Data.DbContextInfo;
using WebPagePub.Data.Models.Db;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace WebPagePub.Data.Repositories.Interfaces
{
    public interface IClickLogRepository : IDisposable
    {
        IApplicationDbContext Context { get; }

        Task<ClickLog> CreateAsync(ClickLog model);

        List<ClickLog> GetClicksInRange(DateTime startDate, DateTime endDate);
    }
}
