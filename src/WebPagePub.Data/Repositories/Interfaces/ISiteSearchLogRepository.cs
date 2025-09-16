using System;
using System.Threading;
using System.Threading.Tasks;
using WebPagePub.Data.Models;
using WebPagePub.Data.Models.Transfer;

namespace WebPagePub.Data.Repositories.Interfaces
{
    public interface ISiteSearchLogRepository
    {
        Task CreateAsync(SiteSearchLog log, CancellationToken token = default);

        Task<PagedResult<SiteSearchLog>> GetPagedAsync(
            DateTime? fromUtc,
            DateTime? toUtc,
            int pageNumber,
            int pageSize,
            string? term = null,
            CancellationToken token = default);
    }
}
