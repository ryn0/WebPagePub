using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using WebPagePub.Data.DbContextInfo.Implementations;

namespace WebPagePub.Data.DbContextInfo
{
    public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
    {
        public IConfigurationRoot Configuration { get; set; }

        public ApplicationDbContext CreateDbContext(string[] args)
        {
            var builder = new DbContextOptionsBuilder<ApplicationDbContext>();

            this.Configuration = new ConfigurationBuilder()
                        .SetBasePath(Directory.GetCurrentDirectory())
                        .AddJsonFile("appsettings.json", optional: true)
                        .AddJsonFile("appsettings.Development.json", optional: true)
                        .AddEnvironmentVariables()
                        .Build();

            // Connection string ALWAYS comes from config (appsettings / environment) — never
            // hard-coded. For design-time commands, supply it via config or an env var such as
            // ConnectionStrings__PostgresConnection. DbProvider.Configure throws if it is missing.
            DbProvider.Configure(
                builder,
                this.Configuration.GetConnectionString("PostgresConnection"));

            return new ApplicationDbContext(builder.Options);
        }
    }
}
