// WebPagePub.Data/Repositories/Implementations/SiteSearchLogRepository.cs
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WebPagePub.Data.DbContextInfo.Interfaces;
using WebPagePub.Data.Models;
using WebPagePub.Data.Models.Transfer;
using WebPagePub.Data.Repositories.Interfaces;

namespace WebPagePub.Data.Repositories.Implementations
{
    public class SiteSearchLogRepository : ISiteSearchLogRepository
    {
        private readonly IApplicationDbContext context;

        public SiteSearchLogRepository(IApplicationDbContext context)
        {
            this.context = context;
        }

        public async Task CreateAsync(SiteSearchLog log, CancellationToken token = default)
        {
            log.Term = (log.Term ?? string.Empty).Trim();
            if (log.Term.Length > 400)
            {
                log.Term = log.Term[..400];
            }

            if (log.CreateDate == default)
            {
                log.CreateDate = DateTime.UtcNow;
            }

            this.context.SiteSearchLogs.Add(log);
            await this.context.SaveChangesAsync(token);
        }

        public async Task<PagedResult<SiteSearchLog>> GetPagedAsync(
            DateTime? fromUtc,
            DateTime? toUtc,
            int pageNumber,
            int pageSize,
            string? term = null,
            CancellationToken token = default)
        {
            if (pageNumber < 1)
            {
                pageNumber = 1;
            }

            if (pageSize < 1)
            {
                pageSize = 10;
            }

            var q = this.context.SiteSearchLogs.AsNoTracking().AsQueryable();

            if (fromUtc.HasValue)
            {
                q = q.Where(x => x.CreateDate >= fromUtc.Value);
            }

            if (toUtc.HasValue)
            {
                q = q.Where(x => x.CreateDate <= toUtc.Value);
            }

            if (!string.IsNullOrWhiteSpace(term))
            {
                var t = term.Trim();
                q = q.Where(x => x.Term.Contains(t));
            }

            var total = await q.CountAsync(token);

            var items = await q.OrderByDescending(x => x.CreateDate)
                               .Skip((pageNumber - 1) * pageSize)
                               .Take(pageSize)
                               .ToListAsync(token);

            return new PagedResult<SiteSearchLog>
            {
                TotalCount = total,
                Items = items
            };
        }
    }
}
