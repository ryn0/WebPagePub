using WebPagePub.Data.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using WebPagePub.Data.DbContextInfo;
using WebPagePub.Data.Repositories.Implementations;
using WebPagePub.Data.Repositories.Interfaces;
using WebPagePub.Services.Interfaces;
using WebPagePub.Services.Implementations;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Rewrite;
using WebPagePub.Web.AppRules;
using WebPagePub.Data.Constants;
using Microsoft.AspNetCore.DataProtection;
using System.IO;
using System;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.ResponseCompression;
using System.Linq;
using System.IO.Compression;
using WebPagePub.Data.Enums;
using System.Collections.Generic;

namespace WebPagePub.Web
{
    public class Startup
    {
        Dictionary<string, string> _redirectPaths = new Dictionary<string, string>();

        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();

            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {            
            services.AddMemoryCache();

            services.Configure<RouteOptions>(options => options.LowercaseUrls = true);

            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(
                    Configuration.GetConnectionString("SqlServerConnection")));

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
                Configuration.GetSection("AmazonEmailCredentials:AccessKey").Value,
                Configuration.GetSection("AmazonEmailCredentials:SecretKey").Value,
                Configuration.GetSection("AmazonEmailCredentials:EmailFrom").Value));

            services.AddTransient<IImageUploaderService, ImageUploaderService>();

            // Add framework services.
            services.AddMvc();

            var sp = services.BuildServiceProvider();
            var cacheService = sp.GetService<ICacheService>();

            var blockedIPRepository = sp.GetService<IBlockedIPRepository>();
            
            services.AddTransient<ISpamFilterService>(x => new SpamFilterService(
                        blockedIPRepository,
                        Configuration.GetSection("NeutrinoApi:UserId").Value,
                        Configuration.GetSection("NeutrinoApi:ApiKey").Value));

            var azureStorageConnection = cacheService.GetSnippet(SiteConfigSetting.AzureStorageConnectionString);

            services.AddSingleton<ISiteFilesRepository>(provider => new SiteFilesRepository(azureStorageConnection));

            // rediects
            var redirectPathRepository = sp.GetService<IRedirectPathRepository>();
            var redirectPathsFromDb = redirectPathRepository.GetAll();
            redirectPathsFromDb.ForEach(x => _redirectPaths.Add(x.Path, x.PathDestination));

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
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
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

            options.Rules.Add(new RedirectMissingPages(_redirectPaths));

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

          //dbInitializer.Initialize();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
