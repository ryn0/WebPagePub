using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.WindowsAzure.Storage;
using WebPagePub.Data.Constants;
using WebPagePub.Data.DbContextInfo;
using WebPagePub.Data.Enums;
using WebPagePub.Data.Models;
using WebPagePub.Data.Models.Db;
using WebPagePub.Data.Repositories.Implementations;
using WebPagePub.Data.Repositories.Interfaces;
using WebPagePub.Managers.Implementations;
using WebPagePub.Managers.Interfaces;
using WebPagePub.Services.Implementations;
using WebPagePub.Services.Interfaces;
using WebPagePub.WebApp.AppRules;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();

builder.Services.AddControllersWithViews().AddRazorRuntimeCompilation();

builder.Services.AddTransient<ICacheService, CacheService>();

builder.Services.Configure<RouteOptions>(options => options.LowercaseUrls = true);

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("SqlServerConnection")));

builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

// db repos
builder.Services.AddTransient<ISitePageSectionRepository, SitePageSectionRepository>();
builder.Services.AddTransient<ISitePageRepository, SitePageRepository>();
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

// db context
builder.Services.AddTransient<IApplicationDbContext, ApplicationDbContext>();
builder.Services.AddTransient<IDbInitializer, DbInitializer>();

// managers
builder.Services.AddTransient<ISitePageManager, SitePageManager>();

// other
builder.Services.AddTransient<ICacheService, CacheService>();

builder.Services.AddTransient<IEmailSender>(x => new AmazonMailService(
    builder.Configuration.GetSection("AmazonEmailCredentials:AccessKey").Value,
    builder.Configuration.GetSection("AmazonEmailCredentials:SecretKey").Value,
    builder.Configuration.GetSection("AmazonEmailCredentials:EmailFrom").Value));

builder.Services.AddTransient<IImageUploaderService, ImageUploaderService>();

builder.Services.AddTransient<ISiteFilesRepository, SiteFilesRepository>();

builder.Services.AddTransient<IBlobService>(provider =>
{
    var cacheService = provider.GetRequiredService<ICacheService>();
    var azureStorageConnection = cacheService.GetSnippet(SiteConfigSetting.AzureStorageConnectionString);

    if (string.IsNullOrEmpty(azureStorageConnection))
    {
        throw new InvalidOperationException($"The snippet for {nameof(SiteConfigSetting.AzureStorageConnectionString)} is not available.");
    }

    var azureConnection = CloudStorageAccount.Parse(azureStorageConnection);
    var cloudBlobClient = azureConnection.CreateCloudBlobClient();

    return new BlobService(cloudBlobClient);
});

builder.Services.AddTransient<ISpamFilterService>(provider =>
{
    var config = provider.GetRequiredService<IConfiguration>();
    var blockedIPRepo = provider.GetRequiredService<IBlockedIPRepository>();

    return new SpamFilterService(
        blockedIPRepo,
        config.GetSection("NeutrinoApi:UserId").Value,
        config.GetSection("NeutrinoApi:ApiKey").Value);
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.Use(async (context, next) =>
{
    var memoryCache = context.RequestServices.GetService<IMemoryCache>();

    if (memoryCache != null)
    {
        if (!memoryCache.TryGetValue(StringConstants.RedirectsCacheKey, out IList<RedirectPath>? redirects))
        {
            var redirectsRepo = context.RequestServices.GetService<IRedirectPathRepository>();
            redirects = redirectsRepo?.GetAll();

            if (redirects != null)
            {
                memoryCache.Set(StringConstants.RedirectsCacheKey, redirects, TimeSpan.FromHours(IntegerConstants.PageCachingMinutes));
            }
        }

        if (redirects != null)
        {
            var path = context.Request.Path.Value;

            if (!string.IsNullOrEmpty(path))
            {
                var redirect = redirects.FirstOrDefault(x => x.Path.EndsWith(path));

                if (redirect != null)
                {
                    context.Response.Redirect(redirect.PathDestination, true);
                    context.Request.Path = redirect.PathDestination;
                }
            }
        }
    }

    await next();
});

var options = new RewriteOptions()
    .AddRedirectToHttpsPermanent()
    .Add(new RedirectWwwToNonWwwRule());
 
app.UseRewriter(options);

app.UseStaticFiles();

app.UseRouting();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.UseAuthorization();

app.Run();