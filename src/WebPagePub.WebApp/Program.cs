using System.Net;
using System.Runtime.InteropServices;
using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using WebPagePub.Data.DbContextInfo.Implementations;
using WebPagePub.Data.DbContextInfo.Interfaces;
using WebPagePub.Data.Enums;
using WebPagePub.Data.Models;
using WebPagePub.Data.Repositories.Implementations;
using WebPagePub.Data.Repositories.Interfaces;
using WebPagePub.FileStorage.Repositories.Implementations;
using WebPagePub.FileStorage.Repositories.Interfaces;
using WebPagePub.Managers.Implementations;
using WebPagePub.Managers.Interfaces;
using WebPagePub.Services.Implementations;
using WebPagePub.Services.Interfaces;
using WebPagePub.Web.Helpers;
using WebPagePub.WebApp.AppRules;

var builder = WebApplication.CreateBuilder(args);

// -------------------------------
// Memory cache: cap ≈ 200 MB, 5 min scan, 20% compaction
// -------------------------------
const long CacheSizeLimitBytes = 200L * 1024L * 1024L; // ~200 MB
builder.Services.AddMemoryCache(opts =>
{
    opts.SizeLimit = CacheSizeLimitBytes;              // units = "bytes" (we'll set SetSize per entry)
    opts.CompactionPercentage = 0.20;                  // trim 20% on pressure
    opts.ExpirationScanFrequency = TimeSpan.FromMinutes(5);
});

// -------------------------------
// MVC / Razor
// -------------------------------
builder.Services.AddRazorPages();
builder.Services.Configure<RouteOptions>(options => options.LowercaseUrls = true);

var mvc = builder.Services.AddControllersWithViews();
// Runtime compilation only while developing (saves memory in prod)
if (builder.Environment.IsDevelopment())
{
    mvc.AddRazorRuntimeCompilation();
}

// -------------------------------
// App services
// -------------------------------
builder.Services.AddTransient<ICacheService, CacheService>();

// DbContext: use pooling to reduce allocations
builder.Services.AddDbContextPool<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("SqlServerConnection")));

// If you need to resolve via the interface, map it to the pooled context (scoped)
builder.Services.AddScoped<IApplicationDbContext>(sp =>
    sp.GetRequiredService<ApplicationDbContext>());

builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

// db repos
builder.Services.AddTransient<ISitePageSectionRepository, SitePageSectionRepository>();
builder.Services.AddTransient<ISitePageRepository, SitePageRepository>();
builder.Services.AddTransient<ISitePageAuditRepository, SitePageAuditRepository>();
builder.Services.AddTransient<ISitePagePhotoRepository, SitePagePhotoRepository>();
builder.Services.AddTransient<ITagRepository, TagRepository>();
builder.Services.AddTransient<ISitePageTagRepository, SitePageTagRepository>();
builder.Services.AddTransient<ILinkRedirectionRepository, LinkRedirectionRepository>();
builder.Services.AddTransient<IEmailSubscriptionRepository, EmailSubscriptionRepository>();
builder.Services.AddTransient<IContentSnippetRepository, ContentSnippetRepository>();
builder.Services.AddTransient<IClickLogRepository, ClickLogRepository>();
builder.Services.AddTransient<ISitePageCommentRepository, SitePageCommentRepository>();
builder.Services.AddTransient<IBlockedIPRepository, BlockedIPRepository>();
builder.Services.AddTransient<IRedirectPathRepository, RedirectPathRepository>();
builder.Services.AddTransient<IAuthorRepository, AuthorRepository>();
builder.Services.AddTransient<ISiteSearchLogRepository, SiteSearchLogRepository>();

// db context helpers
builder.Services.AddTransient<IDbInitializer, DbInitializer>();

// managers
builder.Services.AddTransient<ISitePageManager, SitePageManager>();

builder.Services.AddTransient<IEmailSender>(x => new AmazonMailService(
    builder.Configuration.GetSection("AmazonEmailCredentials:AccessKey").Value,
    builder.Configuration.GetSection("AmazonEmailCredentials:SecretKey").Value,
    builder.Configuration.GetSection("AmazonEmailCredentials:EmailFrom").Value));

builder.Services.AddSingleton<IImageUploaderService, ImageUploaderService>();
builder.Services.AddSingleton<ISiteFilesRepository, SiteFilesRepository>();

builder.Services.AddSingleton<IBlobService>(provider =>
{
    return Task.Run(async () =>
    {
        using var scope = provider.CreateScope();
        var cacheService = scope.ServiceProvider.GetRequiredService<ICacheService>();
        var azureStorageConnection = cacheService.GetSnippet(SiteConfigSetting.AzureStorageConnectionString);

        if (string.IsNullOrWhiteSpace(azureStorageConnection))
        {
            return await BlobService.CreateAsync(null);
        }

        var blobServiceClient = new BlobServiceClient(azureStorageConnection);
        return await BlobService.CreateAsync(blobServiceClient);
    }).GetAwaiter().GetResult();
});

builder.Services.AddTransient<ISpamFilterService>(provider =>
{
    var config = provider.GetRequiredService<IConfiguration>();
    var blockedIPRepo = provider.GetRequiredService<IBlockedIPRepository>();

    return new SpamFilterService(
        blockedIPRepo,
        config.GetSection("IPinfo:AccessToken").Value);
});

builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestBodySize = 262_144_000; // 250 MB
});

var app = builder.Build();

// -------------------------------
// Pipeline
// -------------------------------
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

// Build rewrite rules (including custom redirects from DB)
using (var scope = app.Services.CreateScope())
{
    var scopedServiceProvider = scope.ServiceProvider;

    var redirectRepo = scopedServiceProvider.GetService<IRedirectPathRepository>();
    var redirects = redirectRepo?.GetAll();

    var options = new RewriteOptions()
        .AddRedirectToHttpsPermanent()
        .Add(new RedirectWwwToNonWwwRule())
        .Add(new RedirectLowerCaseRule());

    if (redirects != null)
    {
        foreach (var redirect in redirects)
        {
            var fromPath = redirect.Path;
            if (fromPath.StartsWith("/"))
            {
                fromPath = redirect.Path.Remove(0, 1);
            }

            options.AddRedirect(
                string.Format("^{0}$", fromPath),
                redirect.PathDestination,
                (int)HttpStatusCode.MovedPermanently);
        }
    }

    app.UseRewriter(options);

    // -------------------------------------------
    // Pre-warm link cache with 20-minute sliding expiration
    // and per-entry size so the 200 MB cap is enforced.
    // -------------------------------------------
    var memoryCache = scopedServiceProvider.GetService<IMemoryCache>();
    var linkRepo = scopedServiceProvider.GetService<ILinkRedirectionRepository>();
    var links = linkRepo?.GetAll();

    if (links != null && links.Any() && memoryCache != null)
    {
        foreach (var link in links)
        {
            var cacheKey = CacheHelper.GetLinkCacheKey(link.LinkKey);

            var optionsEntry = new MemoryCacheEntryOptions()
                .SetSlidingExpiration(TimeSpan.FromMinutes(20)) // never lasts over 20 min unless used
                                                                // (optional absolute cap if you want a hard max lifetime):
                                                                // .SetAbsoluteExpiration(TimeSpan.FromHours(12))
                .SetPriority(CacheItemPriority.Normal)
                .SetSize(EstimateBytes(cacheKey, link.UrlDestination)); // set entry size (bytes)

            memoryCache.Set(cacheKey, link.UrlDestination, optionsEntry);
        }
    }
}

app.UseStaticFiles();
app.UseRouting();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.UseAuthorization();

app.Run();

// -------------------------------
// Local helpers
// -------------------------------

// Estimate bytes for string-based cache entries so SizeLimit (~200 MB) is meaningful.
// .NET strings are UTF-16 (≈2 bytes/char). Add small overhead for object headers.
static long EstimateBytes(string key, string value)
{
    long bytes = 64; // overhead fudge factor for entry structures
    if (!string.IsNullOrEmpty(key)) bytes += (long)key.Length * 2;
    if (!string.IsNullOrEmpty(value)) bytes += (long)value.Length * 2;
    return bytes;
}
