using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using WebPagePub.ChatCommander.Enums;
using WebPagePub.ChatCommander.Interfaces;
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
var azureStorageConnection = GetAZureConnectionSTring(snippetsRepo);

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
   .AddSingleton<ISiteFilesRepository>(provider => new SiteFilesRepository(azureStorageConnection))
   .BuildServiceProvider();

Console.WriteLine("Select which workflow to run from below:");
Console.WriteLine((int)Workflows.ArticleTopicExpansionGenerator + " - " + Workflows.ArticleTopicExpansionGenerator);
Console.WriteLine((int)Workflows.ArticleFromKeywordsGenerator + " - " + Workflows.ArticleFromKeywordsGenerator);
Console.Write("Type number and press enter: ");

var chatGptSettings = config.GetRequiredSection("ChatGpt").Get<ChatGptSettings>();
var sitePageManager = serviceProvider.GetService<ISitePageManager>();
var workflowSelection = Console.ReadLine();
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
    default:
        throw new Exception("Not a valid selection");
}

await pageEditor.CreatePagesAsync();

Console.ReadLine();

string GetAZureConnectionSTring(IContentSnippetRepository? snippetsRepo)
{
    if (snippetsRepo == null)
    {
        return null;
    }

    var connectionString = snippetsRepo.Get(SiteConfigSetting.AzureStorageConnectionString);

    if (connectionString == null)
    {
        return null;
    }

    return connectionString.Content;
}