using System.Net;
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

// Add services to the container.
builder.Services.AddRazorPages();

builder.Services.Configure<RouteOptions>(options => options.LowercaseUrls = true);

builder.Services.AddControllersWithViews().AddRazorRuntimeCompilation();

builder.Services.AddTransient<ICacheService, CacheService>();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("SqlServerConnection")));

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

// db context
builder.Services.AddTransient<IApplicationDbContext, ApplicationDbContext>();
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
    options.Limits.MaxRequestBodySize = 262_144_000;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");

    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

using var scope = app.Services.CreateScope();
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

var memoryCache = scopedServiceProvider.GetService<IMemoryCache>();
var linkRepo = scopedServiceProvider.GetService<ILinkRedirectionRepository>();
var links = linkRepo?.GetAll();

if (links != null && links.Any() && memoryCache != null)
{
    foreach (var link in links)
    {
        var cacheKey = CacheHelper.GetLinkCacheKey(link.LinkKey);

        memoryCache.Set(cacheKey, link.UrlDestination);
    }
}

app.UseRewriter(options);

app.UseStaticFiles();

app.UseRouting();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.UseAuthorization();

app.Run();