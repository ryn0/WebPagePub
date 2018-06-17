using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using log4net;
using WebPagePub.Data.DbContextInfo;
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
                throw new Exception("DB error", ex.InnerException);
            }
        }

        public List<ClickLog> GetClicksInRange(DateTime startDate, DateTime endDate)
        {
            try
            {
                var clicks = this.Context.ClickLog.Where(x => x.CreateDate >= startDate && x.CreateDate <= endDate).ToList();

                return clicks;
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
    }
}
