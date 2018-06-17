using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using WebPagePub.Data.Constants;
using WebPagePub.Data.DbContextInfo;
using WebPagePub.Data.Enums;
using WebPagePub.Data.Models;
using WebPagePub.Data.Repositories.Implementations;
using WebPagePub.Data.Repositories.Interfaces;
using WebPagePub.Services.Implementations;
using WebPagePub.Services.Interfaces;
using WebPagePub.Web.AppRules;

namespace WebPagePub.Web
{
    public class Startup
    {
        private readonly Dictionary<string, string> redirectPaths = new Dictionary<string, string>();

        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();

            this.Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMemoryCache();

            services.Configure<RouteOptions>(options => options.LowercaseUrls = true);

            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(
                    this.Configuration.GetConnectionString("SqlServerConnection")));

            services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            // db repos
            services.AddTransient<ISitePageSectionRepository, SitePageSectionRepository>();
            services.AddTransient<ISitePageRepository, SitePageRepository>();
            services.AddTransient<ISitePagePhotoRepository, SitePagePhotoRepository>();
            services.AddTransient<ITagRepository, TagRepository>();
            services.AddTransient<ISitePageTagRepository, SitePageTagRepository>();
            services.AddTransient<ILinkRedirectionRepository, LinkRedirectionRepository>();
            services.AddTransient<IEmailSubscriptionRepository, EmailSubscriptionRepository>();
            services.AddTransient<IContentSnippetRepository, ContentSnippetRepository>();
            services.AddTransient<IClickLogRepository, ClickLogRepository>();
            services.AddTransient<ISitePageCommentRepository, SitePageCommentRepository>();
            services.AddTransient<IBlockedIPRepository, BlockedIPRepository>();
            services.AddTransient<IRedirectPathRepository, RedirectPathRepository>();

            // db context
            services.AddTransient<IApplicationDbContext, ApplicationDbContext>();
            services.AddTransient<IDbInitializer, DbInitializer>();

            // other
            services.AddTransient<ICacheService, CacheService>();

            services.AddTransient<IEmailSender>(x => new AmazonMailService(
                this.Configuration.GetSection("AmazonEmailCredentials:AccessKey").Value,
                this.Configuration.GetSection("AmazonEmailCredentials:SecretKey").Value,
                this.Configuration.GetSection("AmazonEmailCredentials:EmailFrom").Value));

            services.AddTransient<IImageUploaderService, ImageUploaderService>();

            // Add framework services.
            services.AddMvc();

            var sp = services.BuildServiceProvider();
            var cacheService = sp.GetService<ICacheService>();

            var blockedIPRepository = sp.GetService<IBlockedIPRepository>();

            services.AddTransient<ISpamFilterService>(x => new SpamFilterService(
                        blockedIPRepository,
                        this.Configuration.GetSection("NeutrinoApi:UserId").Value,
                        this.Configuration.GetSection("NeutrinoApi:ApiKey").Value));

            var azureStorageConnection = cacheService.GetSnippet(SiteConfigSetting.AzureStorageConnectionString);

            services.AddSingleton<ISiteFilesRepository>(provider => new SiteFilesRepository(azureStorageConnection));

            // rediects
            var redirectPathRepository = sp.GetService<IRedirectPathRepository>();
            var redirectPathsFromDb = redirectPathRepository.GetAll();
            redirectPathsFromDb.ForEach(x => this.redirectPaths.Add(x.Path, x.PathDestination));

            var iisName = cacheService.GetSnippet(SiteConfigSetting.IisWebsiteName);

            services.AddDataProtection()

              // This helps surviving a restart: a same app will find back its keys
              .PersistKeysToFileSystem(new DirectoryInfo(@"\Sites\_SiteSettings\keys\"))

              // This helps surviving a site update: each app has its own store, building the site creates a new app
              .SetApplicationName(iisName)
              .SetDefaultKeyLifetime(TimeSpan.FromDays(90));

            services.AddResponseCompression(options =>
            {
                options.Providers.Add<GzipCompressionProvider>();
                options.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(new[] { "image/svg+xml" });
            });

            services.Configure<GzipCompressionProviderOptions>(options =>
            {
                options.Level = CompressionLevel.Fastest;
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(
            IApplicationBuilder app,
            IHostingEnvironment env,
            ILoggerFactory loggerFactory,
            IDbInitializer dbInitializer)
        {
            loggerFactory.AddConsole(this.Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            var options = new RewriteOptions();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseBrowserLink();
            }
            else
            {
                if (BoolConstants.EnableSsl)
                {
                    options.AddRedirectToHttps(301);
                }

                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles();
            app.UseAuthentication();

            options.Rules.Add(new NonWwwRule());

            options.Rules.Add(new RedirectMissingPages(this.redirectPaths));

            app.UseRewriter(options);

            app.Use(async (context, next) =>
            {
                context.Response.Headers.Add("X-Frame-Options", "SAMEORIGIN");
                await next();
            });

            app.Use(async (context, next) =>
            {
                context.Response.Headers.Add("X-Content-Type-Options", "nosniff");
                await next();
            });

            dbInitializer.Initialize();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}