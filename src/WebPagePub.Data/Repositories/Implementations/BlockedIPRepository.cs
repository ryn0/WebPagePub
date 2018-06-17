using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using log4net;
using WebPagePub.Data.DbContextInfo;
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
                throw new Exception("DB error", ex.InnerException);
            }
        }

        public void Dispose()
        {
            this.Context.Dispose();
        }

        public bool IsBlockedIp(string ipAddress)
        {
            try
            {
                var result = this.Context.BlockedIP.FirstOrDefault(x => x.IpAddress == ipAddress);

                return result != null;
            }
            catch (Exception ex)
            {
                Log.Fatal(ex);
                throw new Exception("DB error", ex.InnerException);
            }
        }
    }
}
