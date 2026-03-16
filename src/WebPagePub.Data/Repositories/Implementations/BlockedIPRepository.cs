using System;
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
    public class BlockedIPRepository : IBlockedIPRepository
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public BlockedIPRepository(IApplicationDbContext context)
        {
            this.Context = context;
        }

        public IApplicationDbContext Context { get; private set; }

        public async Task<BlockedIP> CreateAsync(BlockedIP model)
        {
            try
            {
                this.Context.BlockedIP.Add(model);
                await this.Context.SaveChangesAsync();

                return model;
            }
            catch (Exception ex)
            {
                Log.Fatal(ex);
                throw new Exception(StringConstants.DBErrorMessage, ex.InnerException);
            }
        }

        public bool IsBlockedIp(string ipAddress)
        {
            try
            {
                var result = this.Context.BlockedIP.AsNoTracking().FirstOrDefault(x => x.IpAddress == ipAddress);

                return result != null;
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
