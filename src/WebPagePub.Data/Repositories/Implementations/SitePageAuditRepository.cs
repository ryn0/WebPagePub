using System;
using System.Reflection;
using System.Threading.Tasks;
using log4net;
using WebPagePub.Data.Constants;
using WebPagePub.Data.DbContextInfo.Interfaces;
using WebPagePub.Data.Models;
using WebPagePub.Data.Repositories.Interfaces;

namespace WebPagePub.Data.Repositories.Implementations
{
    public class SitePageAuditRepository : ISitePageAuditRepository
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public SitePageAuditRepository(IApplicationDbContext context)
        {
            this.Context = context;
        }

        public IApplicationDbContext Context { get; private set; }

        public async Task<SitePageAudit> CreateAsync(SitePageAudit model)
        {
            try
            {
                await this.Context.SitePageAudit.AddAsync(model);

                // FIX: The original called the synchronous SaveChanges() inside an
                // async method after already awaiting AddAsync. Mixing sync and async
                // DB calls on the same context is inconsistent and blocks a thread
                // pool thread for the duration of the write. SaveChangesAsync completes
                // the full operation without blocking.
                await this.Context.SaveChangesAsync();

                return model;
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
