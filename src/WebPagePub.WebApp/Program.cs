using System.Net;
using Azure.Storage.Blobs;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.EntityFrameworkCore;
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
using WebPagePub.Services;
using WebPagePub.Services.Implementations;
using WebPagePub.Services.Interfaces;
using WebPagePub.Services.Models;
using WebPagePub.Web.Helpers;
using WebPagePub.WebApp.AppRules;

var builder = WebApplication.CreateBuilder(args);

// -------------------------------------
// Memory cache: use KB UNITS (not bytes)
// -------------------------------------
builder.Services.AddMemoryCache(opts =>
{
    opts.SizeLimit = 200L * 1024 * 1024; // 200 MB, in BYTES
    opts.CompactionPercentage = 0.25;
    opts.ExpirationScanFrequency = TimeSpan.FromMinutes(5);
});


// -------------------------------------
// MVC / Razor
// -------------------------------------
builder.Services.AddRazorPages();
builder.Services.Configure<RouteOptions>(options => options.LowercaseUrls = true);

// Every POST must carry the site's anti-forgery token (all forms already emit one). Before this,
// only four of the thirty-two POST actions checked it.
var mvc = builder.Services.AddControllersWithViews(o =>
    o.Filters.Add(new Microsoft.AspNetCore.Mvc.AutoValidateAntiforgeryTokenAttribute()));
if (builder.Environment.IsDevelopment())
{
    mvc.AddRazorRuntimeCompilation();
}

// -------------------------------------
// Session (in-process; required for CAPTCHA)
// -------------------------------------
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.Cookie.Name = ".WebPagePub.Session";
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.IdleTimeout = TimeSpan.FromMinutes(20);
});

// Useful helpers
builder.Services.AddHttpContextAccessor();
builder.Services.AddHttpClient();

// -------------------------------------
// Data Protection — persist the key ring so auth cookies / antiforgery tokens
// survive restarts and redeploys (otherwise admins get logged out every deploy).
// The service runs with ProtectHome=true + ProtectSystem=full + a rsync --delete'd
// deploy dir, so the only stable writable spot is systemd's StateDirectory
// (/var/lib/<app>, exposed as $STATE_DIRECTORY). SetApplicationName keeps the
// purpose strings identical across deploys so the persisted keys keep working.
// Falls back to the framework default when not run under systemd (e.g. dev).
// -------------------------------------
var dpKeyRoot = Environment.GetEnvironmentVariable("STATE_DIRECTORY");
var dpAppName = Environment.GetEnvironmentVariable("WEBPAGEPUB_APP_NAME") ?? "webpagepub";
if (!string.IsNullOrWhiteSpace(dpKeyRoot))
{
    // $STATE_DIRECTORY may list multiple paths (colon-separated); take the first.
    var firstStateDir = dpKeyRoot.Split(':', StringSplitOptions.RemoveEmptyEntries)[0];
    var keysDir = Path.Combine(firstStateDir, "dataprotection-keys");
    Directory.CreateDirectory(keysDir);
    builder.Services.AddDataProtection()
        .PersistKeysToFileSystem(new DirectoryInfo(keysDir))
        .SetApplicationName(dpAppName);
}

// -------------------------------------
// App services
// -------------------------------------
builder.Services.AddTransient<ICacheService, CacheService>();

// DbContext: **pooling with cap**
builder.Services.AddDbContextPool<ApplicationDbContext>(
    options => WebPagePub.Data.DbContextInfo.DbProvider.Configure(
        options,
        builder.Configuration.GetConnectionString("PostgresConnection")),
    poolSize: 64);

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

// Email sending is OFF unless a site's settings say "AmazonEmailCredentials": { "SendingEnabled": "true" }.
// While it is off the sender gets no credentials, so nothing on the site can send mail (owner, 2026-11-16).
var emailSendingEnabled = string.Equals(
    builder.Configuration.GetSection("AmazonEmailCredentials:SendingEnabled").Value?.Trim(), "true", StringComparison.OrdinalIgnoreCase);
builder.Services.AddTransient<IEmailSender>(x => new AmazonMailService(
    emailSendingEnabled ? builder.Configuration.GetSection("AmazonEmailCredentials:AccessKey").Value : null,
    emailSendingEnabled ? builder.Configuration.GetSection("AmazonEmailCredentials:SecretKey").Value : null,
    emailSendingEnabled ? builder.Configuration.GetSection("AmazonEmailCredentials:EmailFrom").Value : null));

builder.Services.AddSingleton<IImageUploaderService, ImageUploaderService>();
// Image storage: Bunny.net Edge Storage when the StorageProvider config snippet
// is "Bunny" (with a zone + key); otherwise Azure Blob. Chosen per-site so each
// site migrates independently and unconfigured sites stay on Azure.
builder.Services.AddSingleton<ISiteFilesRepository>(provider =>
{
    using var scope = provider.CreateScope();
    var cacheService = scope.ServiceProvider.GetRequiredService<ICacheService>();

    var storageProvider = cacheService.GetSnippet(SiteConfigSetting.StorageProvider);
    var bunnyZone = cacheService.GetSnippet(SiteConfigSetting.BunnyStorageZone);
    var bunnyKey = cacheService.GetSnippet(SiteConfigSetting.BunnyStorageAccessKey);
    var bunnyHost = cacheService.GetSnippet(SiteConfigSetting.BunnyStorageHost);

    if (!string.IsNullOrWhiteSpace(storageProvider)
        && storageProvider.Trim().Equals("Bunny", StringComparison.OrdinalIgnoreCase)
        && !string.IsNullOrWhiteSpace(bunnyZone)
        && !string.IsNullOrWhiteSpace(bunnyKey))
    {
        return new BunnySiteFilesRepository(bunnyZone, bunnyKey, bunnyHost);
    }

    // No Bunny config and no Azure blob service configured (e.g. a site that has no image
    // storage set up — BlobService.CreateAsync(null) returns null). Pass the possibly-null blob
    // service through rather than throwing on every request: read-only content (pages, sitemap.xml,
    // llms.txt) then works; image upload/delete no-ops until storage is configured.
    return new SiteFilesRepository(provider.GetService<IBlobService>());
});
builder.Services.AddSingleton<ISnippetFetcher, SnippetFetcher>();

builder.Services.AddTransient<ISponsorJsonClient, SponsorJsonClient>();

builder.Services.Configure<CaptchaOptions>(builder.Configuration.GetSection("Captcha"));
builder.Services.AddTransient<ICaptchaService, CaptchaService>();

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

// -------------------------------------
// Request/body limits (reduce spikes)
// -------------------------------------
builder.Services.Configure<FormOptions>(o =>
{
    o.MultipartBodyLengthLimit = WebPagePub.WebApp.Helpers.UploadLimits.OrdinaryMaxBytes; // uploads raise this per action
    o.ValueCountLimit = 1024;
    o.ValueLengthLimit = 64 * 1024; // 64 KB per value
    o.MultipartHeadersLengthLimit = 16 * 1024;
});

builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestBodySize = WebPagePub.WebApp.Helpers.UploadLimits.OrdinaryMaxBytes; // uploads raise this per action
});

// -------------------------------------
// Memory watchdog (soft trim + hard cap)
// -------------------------------------
builder.Services.AddHostedService<MemoryPressureGuard>();

// Sign-in attempts are limited per visitor address and for the whole site (see SignInRateLimit).
builder.Services.AddRateLimiter(WebPagePub.WebApp.Helpers.SignInRateLimit.Configure);

// Sign-in cookie: HTTPS only, never sent on requests other sites make in the background.
builder.Services.ConfigureApplicationCookie(o =>
{
    o.Cookie.SecurePolicy = Microsoft.AspNetCore.Http.CookieSecurePolicy.Always;
    o.Cookie.SameSite = Microsoft.AspNetCore.Http.SameSiteMode.Lax;
    o.Cookie.HttpOnly = true;
});

var app = builder.Build();

// -------------------------------------
// Pipeline
// -------------------------------------
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

// Build rewrite rules (including custom redirects from DB)
using (var scope = app.Services.CreateScope())
{
    var sp = scope.ServiceProvider;

    var redirectRepo = sp.GetService<IRedirectPathRepository>();
    var redirects = redirectRepo?.GetAll();

    var options = new RewriteOptions()
        .AddRedirectToHttpsPermanent()
        .Add(new RedirectWwwToNonWwwRule())
        .Add(new RedirectLowerCaseRule());

    if (redirects != null)
    {
        // NOTE: Avoid precomputing massive regex tables.
        // If this grows large, consider limiting or batching.
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

    // IMPORTANT: Avoid big pre-warmers; let cache fill lazily with size limits.
}

app.UseStaticFiles();
app.UseRouting();

// After routing; the forwarded-headers step (switched on by ASPNETCORE_FORWARDEDHEADERS_ENABLED) runs
// first of all, so the limiter sees the visitor's address, not nginx's.
app.UseRateLimiter();

app.UseSession();           // before auth/endpoints
app.UseAuthentication();
app.UseAuthorization();

// Signed-in sessions can't be driven from other websites (see CrossSiteRequestGuard).
app.Use(async (context, next) =>
{
    if (WebPagePub.Web.Helpers.CrossSiteRequestGuard.ShouldRefuse(context))
    {
        context.Response.StatusCode = StatusCodes.Status403Forbidden;
        await context.Response.WriteAsync("This request came from another website and was refused. Open the page from this site's admin area and try again.");
        return;
    }

    await next();
});

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
