using System.Net;
using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using WebPagePub.Data.Constants;
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
using WebPagePub.Services.Models;
using WebPagePub.Web.Helpers;
using WebPagePub.WebApp.AppRules;

var builder = WebApplication.CreateBuilder(args);

// -------------------------------------
// Memory cache: use KB UNITS (not bytes)
// -------------------------------------
const long CacheSizeLimitKb = 200L * 1024L; // ~200 MB in KB units
builder.Services.AddMemoryCache(opts =>
{
    opts.SizeLimit = CacheSizeLimitKb;
    opts.CompactionPercentage = 0.20;
    opts.ExpirationScanFrequency = TimeSpan.FromMinutes(5);
});

// -------------------------------------
// MVC / Razor
// -------------------------------------
builder.Services.AddRazorPages();
builder.Services.Configure<RouteOptions>(options => options.LowercaseUrls = true);

var mvc = builder.Services.AddControllersWithViews();
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
// App services
// -------------------------------------
builder.Services.AddTransient<ICacheService, CacheService>();

// DbContext: pooling
builder.Services.AddDbContextPool<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("SqlServerConnection")));

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
builder.Services.AddSingleton<ISnippetFetcher, SnippetFetcher>();

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
    o.MultipartBodyLengthLimit = 64L * 1024 * 1024; // 64 MB
    o.ValueCountLimit = 1024;
    o.ValueLengthLimit = 64 * 1024; // 64 KB per value
    o.MultipartHeadersLengthLimit = 16 * 1024;
});

builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestBodySize = 64L * 1024 * 1024; // 64 MB
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

    // IMPORTANT: Removed link cache pre-warm to avoid large startup memory usage.
    // Cache will populate lazily on first access with proper size units.
}

app.UseStaticFiles();
app.UseRouting();

app.UseSession();           // keep before auth/endpoints
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

// -------------------------------------
// (Optional) Helper you can copy where needed:
// Use this in your own caching code to set IMemoryCache entry sizes in KB units.
// -------------------------------------
static long ToKbUnits(params string?[] values)
{
    long bytes = 64; // overhead
    foreach (var v in values)
    {
        if (!string.IsNullOrEmpty(v))
        {
            bytes += (long)v.Length * 2; // UTF-16 chars ~2 bytes
        }
    }
    long kb = Math.Max(1, (bytes + 1023) / 1024);
    return kb;
}
