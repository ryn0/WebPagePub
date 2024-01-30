using Azure.Storage.Blobs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using WebPagePub.ChatCommander.Enums;
using WebPagePub.ChatCommander.Interfaces;
using WebPagePub.ChatCommander.Models.SettingsModels;
using WebPagePub.ChatCommander.SettingsModels;
using WebPagePub.ChatCommander.WorkFlows.Generators;
using WebPagePub.Data.DbContextInfo.Implementations;
using WebPagePub.Data.DbContextInfo.Interfaces;
using WebPagePub.Data.Enums;
using WebPagePub.Data.Repositories.Implementations;
using WebPagePub.Data.Repositories.Interfaces;
using WebPagePub.FileStorage.Repositories.Implementations;
using WebPagePub.FileStorage.Repositories.Interfaces;
using WebPagePub.Managers.Implementations;
using WebPagePub.Managers.Interfaces;
using WebPagePub.Services.Implementations;
using WebPagePub.Services.Interfaces;

IConfigurationRoot config = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .Build();

var serviceProvider = new ServiceCollection()
    .AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        config.GetConnectionString("SqlServerConnection")))
    .AddTransient<IApplicationDbContext, ApplicationDbContext>()
    .AddTransient<IDbInitializer, DbInitializer>()
    .AddTransient<IContentSnippetRepository, ContentSnippetRepository>()
    .BuildServiceProvider();

var snippetsRepo = serviceProvider.GetService<IContentSnippetRepository>();
var azureStorageConnection = GetAzureConnectionString(snippetsRepo);

serviceProvider = new ServiceCollection()
   .AddDbContext<ApplicationDbContext>(options =>
   options.UseSqlServer(
       config.GetConnectionString("SqlServerConnection")))
   .AddTransient<IApplicationDbContext, ApplicationDbContext>()
   .AddTransient<IDbInitializer, DbInitializer>()
   .AddTransient<ISitePageSectionRepository, SitePageSectionRepository>()
   .AddTransient<ISitePageRepository, SitePageRepository>()
   .AddTransient<ISitePagePhotoRepository, SitePagePhotoRepository>()
   .AddTransient<ITagRepository, TagRepository>()
   .AddTransient<ISitePageTagRepository, SitePageTagRepository>()
   .AddTransient<ILinkRedirectionRepository, LinkRedirectionRepository>()
   .AddTransient<IEmailSubscriptionRepository, EmailSubscriptionRepository>()
   .AddTransient<IContentSnippetRepository, ContentSnippetRepository>()
   .AddTransient<IClickLogRepository, ClickLogRepository>()
   .AddTransient<ISitePageCommentRepository, SitePageCommentRepository>()
   .AddTransient<IBlockedIPRepository, BlockedIPRepository>()
   .AddTransient<IRedirectPathRepository, RedirectPathRepository>()
   .AddTransient<IAuthorRepository, AuthorRepository>()
   .AddTransient<ISitePageManager, SitePageManager>()
   .AddTransient<IImageUploaderService, ImageUploaderService>()
   .AddTransient<ISitePageAuditRepository, SitePageAuditRepository>()
   .AddSingleton<ISiteFilesRepository, SiteFilesRepository>()
   .AddTransient<ICacheService, CacheService>()
   .AddMemoryCache()
   .AddSingleton<IBlobService>(provider =>
    {
        return Task.Run(async () =>
        {
            using var scope = provider.CreateScope();
            var cacheService = scope.ServiceProvider.GetRequiredService<ICacheService>();
            var azureStorageConnection = cacheService.GetSnippet(SiteConfigSetting.AzureStorageConnectionString);
            var blobServiceClient = new BlobServiceClient(azureStorageConnection);

            return await BlobService.CreateAsync(blobServiceClient);
        }).GetAwaiter().GetResult();
    })
   .BuildServiceProvider();

Console.WriteLine("Select which workflow to run from below:");

foreach (Workflows workflow in Enum.GetValues(typeof(Workflows)))
{
    if (workflow == Workflows.Unknown)
    {
        continue;
    }
    Console.WriteLine($"{(int)workflow} - {workflow}");
}

Console.Write("Type number and press enter: ");

var chatGptSettings = config.GetRequiredSection("OpenAiApiSettings").Get<OpenAiApiSettings>();
if (chatGptSettings == null)
{
    throw new NullReferenceException(nameof(chatGptSettings));
}
var sitePageManager = serviceProvider.GetService<ISitePageManager>();

if (sitePageManager == null)
{
    throw new NullReferenceException(nameof(sitePageManager));
}

var workflowSelection = Console.ReadLine();

if (string.IsNullOrWhiteSpace(workflowSelection))
{
    throw new Exception("Invalid selection");
}

var workflowSelectionEnum = (Workflows)Convert.ToInt32(workflowSelection.Trim());

IPageEditor pageEditor;

switch (workflowSelectionEnum)
{
    case Workflows.ArticleTopicExpansionGenerator:
        var articleTopicSettings = config.GetRequiredSection("ArticleTopicExpansionGenerator").Get<ArticleTopicExpansionGeneratorModel>()
            ?? throw new InvalidOperationException("ArticleTopicExpansionGenerator settings are missing or not correctly configured.");
        pageEditor = new ArticleTopicExpansionGenerator(
            chatGptSettings,
            sitePageManager,
            articleTopicSettings);

        break;
    case Workflows.ArticleFromKeywordsGenerator:
        var articleKeywordsSettings =
            config.GetRequiredSection("ArticleFromKeywordsGenerator").Get<ArticleFromKeywordsGeneratorModel>()
                        ?? throw new InvalidOperationException("ArticleFromKeywordsGenerator settings are missing or not correctly configured.");

        pageEditor = new ArticleFromKeywordsGenerator(
            chatGptSettings,
            sitePageManager,
            articleKeywordsSettings);

        break;
    case Workflows.ArticleWithCalculatorGenerator:
        var articleCalculatorsSettings =
            config.GetRequiredSection("ArticleWithCalculatorGenerator").Get<ArticleWithCalculatorGeneratorModel>()
                        ?? throw new InvalidOperationException("ArticleWithCalculatorGenerator settings are missing or not correctly configured.");

        pageEditor = new ArticleWithCalculatorGenerator(
            chatGptSettings,
            sitePageManager,
            articleCalculatorsSettings);

        break;
    case Workflows.ArticleFromUrlGenerator:
        var articleUrlSettings =
            config.GetRequiredSection("ArticleFromUrlGenerator").Get<ArticleFromUrlGeneratorModel>()
             ?? throw new InvalidOperationException("ArticleFromUrlGenerator settings are missing or not correctly configured.");

        pageEditor = new ArticleFromUrlGenerator(
            chatGptSettings,
            sitePageManager,
            articleUrlSettings);

        break;
    case Workflows.ArticleTagGenerator:
        var articleTagGeneratorSettings =
            config.GetRequiredSection("ArticleTagGenerator").Get<ArticleTagGeneratorModel>()
             ?? throw new InvalidOperationException("ArticleTagGenerator settings are missing or not correctly configured.");

        pageEditor = new ArticleTagGenerator(
            chatGptSettings,
            sitePageManager,
            articleTagGeneratorSettings);

        break;
    case Workflows.ArticleInternalLinkGenerator:
       
        pageEditor = new ArticleInternalLinkGenerator( 
            sitePageManager);

        break;
    default:
        throw new Exception("Not a valid selection");
}

await pageEditor.Execute();

Console.ReadLine();

static string GetAzureConnectionString(IContentSnippetRepository? snippetsRepo)
{
    if (snippetsRepo == null)
    {
        return string.Empty;
    }

    var connectionString = snippetsRepo.Get(SiteConfigSetting.AzureStorageConnectionString);

    return connectionString?.Content ?? string.Empty;
}