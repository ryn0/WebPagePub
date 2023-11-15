using System;
using System.Threading.Tasks;
using WebPagePub.Data.DbContextInfo.Interfaces;
using WebPagePub.Data.Models;

namespace WebPagePub.Data.Repositories.Interfaces
{
    public interface ISitePageAuditRepository : IDisposable
    {
        IApplicationDbContext Context { get; }

        Task<SitePageAudit> CreateAsync(SitePageAudit model);
    }
}