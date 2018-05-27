using WebPagePub.Data.Models;
using WebPagePub.Data.Models.Db;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace WebPagePub.Data.DbContextInfo
{
    public interface IApplicationDbContext : IDisposable
    {
        DbSet<SitePageSection> SitePageSection { get; set; }

        DbSet<SitePage> SitePage { get; set; }

        DbSet<SitePageComment> SitePageComment { get; set; }

        DbSet<SitePagePhoto> SitePagePhoto { get; set; }

        DbSet<SitePageTag> SitePageTag { get; set; }

        DbSet<Tag> Tag { get; set; }

        DbSet<ApplicationUser> ApplicationUser { get; set; }

        DbSet<ApplicationUserRole> ApplicationUserRole { get; set; }

        DbSet<LinkRedirection> LinkRedirection { get; set; }

        DbSet<EmailSubscription> EmailSubscription { get; set; }

        DbSet<ContentSnippet> ContentSnippet { get; set; }

        DbSet<ClickLog> ClickLog { get; set; }

        DbSet<BlockedIP> BlockedIP { get; set; }

        int SaveChanges();

        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default(CancellationToken));
    }
}
