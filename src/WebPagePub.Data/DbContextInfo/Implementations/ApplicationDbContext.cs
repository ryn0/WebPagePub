using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Reflection.Emit;
using System.Threading;
using System.Threading.Tasks;
using WebPagePub.Data.DbContextInfo.Interfaces;
using WebPagePub.Data.DbModels.BaseDbModels;
using WebPagePub.Data.Models;
using WebPagePub.Data.Models.Db;

namespace WebPagePub.Data.DbContextInfo.Implementations
{
    public class ApplicationDbContext :
                    ApplicationBaseContext<ApplicationDbContext>,
                    IApplicationDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<SitePageComment> SitePageComment { get; set; }

        public DbSet<SitePage> SitePage { get; set; }

        public DbSet<SitePagePhoto> SitePagePhoto { get; set; }

        public DbSet<SitePageTag> SitePageTag { get; set; }

        public DbSet<Tag> Tag { get; set; }

        public DbSet<ApplicationUser> ApplicationUser { get; set; }

        public DbSet<ApplicationUserRole> ApplicationUserRole { get; set; }

        public DbSet<LinkRedirection> LinkRedirection { get; set; }

        public DbSet<EmailSubscription> EmailSubscription { get; set; }

        public DbSet<ContentSnippet> ContentSnippet { get; set; }

        public DbSet<SitePageSection> SitePageSection { get; set; }

        public DbSet<ClickLog> ClickLog { get; set; }

        public DbSet<BlockedIP> BlockedIP { get; set; }

        public DbSet<RedirectPath> RedirectPath { get; set; }

        public DbSet<Author> Author { get; set; }

        public DbSet<SitePageAudit> SitePageAudit { get; set; }

        public DbSet<SiteSearchLog> SiteSearchLogs { get; set; }

        public override int SaveChanges()
        {
            this.SetDates();

            return base.SaveChanges();
        }

        public override Task<int> SaveChangesAsync(
            CancellationToken cancellationToken = default)
        {
            this.SetDates();

            return base.SaveChangesAsync(cancellationToken);
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<RedirectPath>()
                    .HasIndex(b => b.Path)
                    .IsUnique();

            builder.Entity<BlockedIP>()
                    .HasIndex(b => b.IpAddress)
                    .IsUnique();

            builder.Entity<ContentSnippet>()
                    .HasIndex(b => b.SnippetType)
                    .IsUnique();

            builder.Entity<SitePage>()
                    .HasIndex(p => new { p.SitePageSectionId, p.Key })
                    .IsUnique();

            builder.Entity<SitePageSection>()
                .HasIndex(p => p.Key)
                .IsUnique();

            builder.Entity<SitePageTag>()
                .HasKey(c => new { c.SitePageId, c.TagId });

            builder.Entity<SitePageTag>()
                .HasOne(bc => bc.SitePage)
                .WithMany(b => b.SitePageTags)
                .HasForeignKey(bc => bc.SitePageId);

            builder.Entity<SitePageTag>()
                .HasOne(bc => bc.Tag)
                .WithMany(c => c.SitePageTags)
                .HasForeignKey(bc => bc.TagId);

            builder.Entity<Tag>()
                .HasIndex(p => p.Key)
                .IsUnique();

            builder.Entity<Author>()
                .HasIndex(p => p.AuthorId)
                .IsUnique();

            builder.Entity<ClickLog>()
                .HasIndex(p => p.CreateDate);

            builder.Entity<SiteSearchLog>(eb =>
            {
                eb.HasKey(x => x.SiteSearchLogId);
                eb.Property(x => x.Term).IsRequired().HasMaxLength(400);
                eb.Property(x => x.UserAgent).HasMaxLength(512);
                eb.Property(x => x.ClientIp).HasMaxLength(64);
                eb.Property(x => x.Referer).HasMaxLength(512);
                eb.Property(x => x.Path).HasMaxLength(256);

                eb.HasIndex(x => x.CreateDate);
                eb.HasIndex(x => x.Term);
            });


            base.OnModelCreating(builder);
        }

        private void SetDates()
        {
            foreach (var entry in this.ChangeTracker.Entries()
                .Where(x => (x.Entity is StateInfo)
                            && x.State == EntityState.Added)
                .Select(x => x.Entity as StateInfo))
            {
                if (entry.CreateDate == DateTime.MinValue)
                {
                    entry.CreateDate = DateTime.UtcNow;
                }
            }

            foreach (var entry in this.ChangeTracker.Entries()
                .Where(x => (x.Entity is CreatedStateInfo)
                            && x.State == EntityState.Added)
                .Select(x => x.Entity as CreatedStateInfo))
            {
                if (entry.CreateDate == DateTime.MinValue)
                {
                    entry.CreateDate = DateTime.UtcNow;
                }
            }

            foreach (var entry in this.ChangeTracker.Entries()
                .Where(x => x.Entity is StateInfo && x.State == EntityState.Modified)
                .Select(x => x.Entity as StateInfo))
            {
                entry.UpdateDate = DateTime.UtcNow;
            }
        }
    }
}