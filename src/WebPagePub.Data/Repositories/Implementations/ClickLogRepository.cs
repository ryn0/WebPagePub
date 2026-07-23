using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using log4net;
using Microsoft.EntityFrameworkCore;
using WebPagePub.Data.Constants;
using WebPagePub.Data.DbContextInfo.Interfaces;
using WebPagePub.Data.Models.Db;
using WebPagePub.Data.Repositories.Interfaces;

namespace WebPagePub.Data.Repositories.Implementations
{
    public class ClickLogRepository : IClickLogRepository
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public ClickLogRepository(IApplicationDbContext context)
        {
            this.Context = context;
        }

        public IApplicationDbContext Context { get; private set; }

        public async Task<ClickLog> CreateAsync(ClickLog model)
        {
            try
            {
                this.Context.ClickLog.Add(model);
                await this.Context.SaveChangesAsync();

                return model;
            }
            catch (Exception ex)
            {
                Log.Fatal(ex);
                throw new Exception(StringConstants.DBErrorMessage, ex.InnerException);
            }
        }

        public IList<ClickLog> GetClicksInRange(DateTime startDate, DateTime endDate)
        {
            // ClickLog.CreateDate is a Postgres 'timestamp with time zone' (timestamptz), and Npgsql
            // only accepts UTC DateTimes for it. The report controllers build these bounds with
            // `new DateTime(...)` (Kind=Unspecified), which throws "Cannot write DateTime with
            // Kind=Unspecified to PostgreSQL type 'timestamp with time zone'". Normalize to UTC here
            // so every caller is safe. The values are already UTC-intended, so just stamp the Kind
            // (converting only if a Local time somehow arrives).
            startDate = startDate.Kind == DateTimeKind.Local ? startDate.ToUniversalTime() : DateTime.SpecifyKind(startDate, DateTimeKind.Utc);
            endDate = endDate.Kind == DateTimeKind.Local ? endDate.ToUniversalTime() : DateTime.SpecifyKind(endDate, DateTimeKind.Utc);

            try
            {
                return this.Context.ClickLog
                    .AsNoTracking()
                    .Where(x => x.CreateDate >= startDate && x.CreateDate <= endDate)
                    .ToList();
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
