using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using WebPagePub.Data.DbContextInfo;

namespace WebPagePub.Data
{
    public static class ServiceCollectionExtensions
    {
        public static void AddEntityFramework(this IServiceCollection services, string connectionString)
        {
            services.AddDbContext<ApplicationDbContext>(options =>
                    options.UseSqlServer(connectionString));
        }
    }
}
