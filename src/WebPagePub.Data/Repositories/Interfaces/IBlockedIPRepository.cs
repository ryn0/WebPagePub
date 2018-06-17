using System;
using System.Threading.Tasks;
using WebPagePub.Data.DbContextInfo;
using WebPagePub.Data.Models.Db;

namespace WebPagePub.Data.Repositories.Interfaces
{
    public interface IBlockedIPRepository : IDisposable
    {
        IApplicationDbContext Context { get; }

        Task<BlockedIP> CreateAsync(BlockedIP model);

        bool IsBlockedIp(string ipAddress);
    }
}
