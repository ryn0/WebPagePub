using log4net;
using WebPagePub.Data.DbContextInfo;
using WebPagePub.Data.Models.Db;
using WebPagePub.Data.Repositories.Interfaces;
using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace WebPagePub.Data.Repositories.Implementations
{
    public class BlockedIPRepository : IBlockedIPRepository
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public BlockedIPRepository(IApplicationDbContext context)
        {
            Context = context;
        }

        public IApplicationDbContext Context { get; private set; }

        public async Task<BlockedIP> CreateAsync(BlockedIP model)
        {
            try
            {
                Context.BlockedIP.Add(model);
                await Context.SaveChangesAsync();

                return model;
            }
            catch (Exception ex)
            {
                log.Fatal(ex);
                throw new Exception("DB error", ex.InnerException);
            }
        }

        public void Dispose()
        {
            Context.Dispose();
        }

        public bool IsBlockedIp(string ipAddress)
        {
            try
            {
                var result = Context.BlockedIP.FirstOrDefault(x => x.IpAddress == ipAddress);

                return result != null;
            }
            catch (Exception ex)
            {
                log.Fatal(ex);
                throw new Exception("DB error", ex.InnerException);
            }
        }
    }
}
