using System;
using Microsoft.EntityFrameworkCore;

namespace WebPagePub.Data.DbContextInfo
{
    /// <summary>
    /// Configures <see cref="Implementations.ApplicationDbContext"/> to use PostgreSQL.
    /// The connection string always comes from configuration (appsettings / environment) —
    /// it is never hard-coded — and a clear error is thrown if it is missing.
    /// </summary>
    public static class DbProvider
    {
        public static void Configure(DbContextOptionsBuilder options, string postgresConnection)
        {
            if (string.IsNullOrWhiteSpace(postgresConnection))
            {
                throw new InvalidOperationException(
                    "ConnectionStrings:PostgresConnection is not configured. " +
                    "Set it in configuration (appsettings / environment).");
            }

            options.UseNpgsql(postgresConnection);
        }
    }
}
