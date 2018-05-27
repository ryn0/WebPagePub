using WebPagePub.Data.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using WebPagePub.Data.DbContextInfo;
using log4net;
using System.Reflection;
using WebPagePub.Data.Models.Db;
using System.Linq;
using System.Threading.Tasks;

namespace WebPagePub.Data.Repositories.Implementations
{
    public class ClickLogRepository : IClickLogRepository
    { 
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public ClickLogRepository(IApplicationDbContext context)
        {
            Context = context;
        }

        public IApplicationDbContext Context { get; private set; }

        public async Task<ClickLog> CreateAsync(ClickLog model)
        {
            try
            {
                Context.ClickLog.Add(model);
                await Context.SaveChangesAsync();

                return model;
            }
            catch (Exception ex)
            {
                log.Fatal(ex);
                throw new Exception("DB error", ex.InnerException);
            }
        }

        public List<ClickLog> GetClicksInRange(DateTime startDate, DateTime endDate)
        {
            try
            {
                var clicks = Context.ClickLog.Where(x => x.CreateDate >= startDate && x.CreateDate <= endDate).ToList();

                return clicks;
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
    }
}
