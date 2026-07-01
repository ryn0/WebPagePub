using Microsoft.Extensions.DependencyInjection;
using WebPagePub.Data.DbContextInfo;
using WebPagePub.Data.DbContextInfo.Implementations;

namespace WebPagePub.Data
{
    public static class ServiceCollectionExtensions
    {
        public static void AddEntityFramework(this IServiceCollection services, string postgresConnection)
        {
            services.AddDbContext<ApplicationDbContext>(options =>
                    DbProvider.Configure(options, postgresConnection));
        }
    }
}
