using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using WebPagePub.ChatCommander.Enums;
using WebPagePub.ChatCommander.Interfaces;
using WebPagePub.ChatCommander.Models.SettingsModels;
using WebPagePub.ChatCommander.SettingsModels;
using WebPagePub.ChatCommander.WorkFlows.Generators;
using WebPagePub.Data.DbContextInfo;
using WebPagePub.Data.Enums;
using WebPagePub.Data.Repositories.Implementations;
using WebPagePub.Data.Repositories.Interfaces;
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
CloudBlobClient cloudBlobClient = null;

if (!string.IsNullOrEmpty(azureStorageConnection))
{
    var azureConnection = CloudStorageAccount.Parse(azureStorageConnection);
    cloudBlobClient = azureConnection.CreateCloudBlobClient();
}
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
   .AddSingleton<IBlobService>(provider => 
        new BlobService(cloudBlobClient))
   .AddSingleton<ISiteFilesRepository, SiteFilesRepository>()
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
        var articleTopicSettings =
            config.GetRequiredSection("ArticleTopicExpansionGenerator").Get<ArticleTopicExpansionGeneratorModel>();

        pageEditor = new ArticleTopicExpansionGenerator(
            chatGptSettings,
            sitePageManager,
            articleTopicSettings);
        
        break;
    case Workflows.ArticleFromKeywordsGenerator:
        var articleKeywordsSettings =
            config.GetRequiredSection("ArticleFromKeywordsGenerator").Get<ArticleFromKeywordsGeneratorModel>();

        pageEditor = new ArticleFromKeywordsGenerator(
            chatGptSettings,
            sitePageManager,
            articleKeywordsSettings);

        break;
    case Workflows.ArticleWithCalculatorGenerator:
        var articleCalculatorsSettings =
            config.GetRequiredSection("ArticleWithCalculatorGenerator").Get<ArticleWithCalculatorGeneratorModel>();

        pageEditor = new ArticleWithCalculatorGenerator(
            chatGptSettings,
            sitePageManager,
            articleCalculatorsSettings);

        break;
    case Workflows.ArticleFromUrlGenerator:
        var articleUrlSettings =
            config.GetRequiredSection("ArticleFromUrlGenerator").Get<ArticleFromUrlGeneratorModel>();

        pageEditor = new ArticleFromUrlGenerator(
            chatGptSettings,
            sitePageManager,
            articleUrlSettings);

        break;
    default:
        throw new Exception("Not a valid selection");
}

await pageEditor.CreatePagesAsync();

Console.ReadLine();

string GetAzureConnectionString(IContentSnippetRepository? snippetsRepo)
{
    if (snippetsRepo == null)
    {
        return string.Empty;
    }

    var connectionString = snippetsRepo.Get(SiteConfigSetting.AzureStorageConnectionString);

    return connectionString?.Content ?? string.Empty;
}