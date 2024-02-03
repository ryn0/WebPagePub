using System.Text;
using WebPagePub.PageManager.Console.Helpers;
using WebPagePub.PageManager.Console.Interfaces;
using WebPagePub.PageManager.Console.Models.SettingsModels;
using WebPagePub.PageManager.Console.Utilities;
using WebPagePub.Data.Models;
using WebPagePub.Managers.Interfaces;

namespace WebPagePub.PageManager.Console.WorkFlows.Generators
{
    public class ArticleFromKeywordsGenerator : BaseGenerator, IExecute
    {
        const string QuestionPlaceholder = "[keyword]";

        public ArticleFromKeywordsGenerator(
            OpenAiApiSettings chatGptSettings,
            ISitePageManager sitePageManager,
            ArticleFromKeywordsGeneratorModel model) : 
            base (chatGptSettings, sitePageManager)
        {
            base.SectionKey = model.SectionKey;
            base.MinutesOffsetForArticleMin = model.MinutesOffsetForArticleMin;
            base.MinutesOffsetForArticleMax = model.MinutesOffsetForArticleMax;
            base.OpenAiApiSettings = chatGptSettings;
            base.sitePageManager = sitePageManager;
        }

        public async Task Execute()
        {
            if (openAiApiClient == null)
            {
                throw new NullReferenceException($"{openAiApiClient}");
            }

            startDateTime = DateTime.UtcNow;
            WriteStartMessage(GetType().Name);

            int? authorId = GetAuthor();

            var siteSection = sitePageManager.GetSiteSection(SectionKey) ?? throw new Exception("Site section missing");
            var fileDir = Directory.GetCurrentDirectory() + @"\WorkFlows\Prompts\ArticlesFromKeywords";

            // 00
            var promptTextRaw00 = File.ReadAllText(Path.Combine(fileDir, "00-Setup.txt"), Encoding.UTF8);
            await openAiApiClient.SubmitMessage(promptTextRaw00);

            // 01
            System.Console.Write($"Loading keywords...");
            var keywords = File.ReadAllText(Path.Combine(fileDir, "01-LoadKeywords.txt"), Encoding.UTF8);
            var keywordList = TextHelpers.GetUniqueLines(keywords);
            System.Console.WriteLine($"{keywordList.Length} found.");

            // 02
            var promptTextRaw02 = File.ReadAllText(Path.Combine(fileDir, "02-ArticleKey.txt"), Encoding.UTF8);

            foreach (var keyword in keywordList)
            {
                if (string.IsNullOrWhiteSpace(keyword))
                {
                    continue;
                }

                var promptTextFormatted02 = FormatPromptTextKeyword(promptTextRaw02, keyword);
                var articleKey = await openAiApiClient.SubmitMessage(promptTextFormatted02);

                articleKey = TextHelpers.CleanArticleKey(articleKey);

                if (string.IsNullOrWhiteSpace(articleKey))
                {
                    System.Console.WriteLine("Invalid article key");
                    continue;
                }

                if (sitePageManager.DoesPageExistSimilar(siteSection.SitePageSectionId, articleKey))
                {
                    System.Console.WriteLine($"'{articleKey}' (or similar) exists");
                    continue;
                }

                System.Console.Write($"Creating article for: {keyword}...");

                // 03
                var promptTextRaw03 = File.ReadAllText(Path.Combine(fileDir, "03-ArticleHtmlBody.txt"), Encoding.UTF8);
                var promptTextFormatted03 = FormatPromptTextKeyword(promptTextRaw03, keyword);

                openAiApiClient.MaxTokens = 2000;
                var articleHtml = await openAiApiClient.SubmitMessage(promptTextFormatted03);
                articleHtml = articleHtml.Trim();

                var maxAttemptsAtHtmlBody = 3;
                var attemptsAtHtmlBody = 1;

                while (!articleHtml.EndsWith("</p>") && attemptsAtHtmlBody < maxAttemptsAtHtmlBody)
                {
                    System.Console.Write(".");
                    openAiApiClient.MaxTokens = 1000;
                    articleHtml = await openAiApiClient.SubmitMessage(promptTextFormatted03 + " Write this shorter than before.");
                    articleHtml = articleHtml.Trim();
                    attemptsAtHtmlBody++;
                }

                openAiApiClient.MaxTokens = 1000;

                if (attemptsAtHtmlBody > maxAttemptsAtHtmlBody)
                {
                    System.Console.WriteLine(" - HTML not formatted correctly at end");
                    continue;
                }

                // 04
                var promptTextRaw04 = File.ReadAllText(Path.Combine(fileDir, "04-ArticleMetaDescription.txt"), Encoding.UTF8);
                var promptTextFormatted04 = FormatPromptTextKeyword(promptTextRaw04, keyword);
                var articleMetaDescription = await openAiApiClient.SubmitMessage(promptTextFormatted04);
                articleMetaDescription = TextHelpers.CleanText(articleMetaDescription);

                while (articleMetaDescription.Length > 160)
                {
                    System.Console.Write(".");
                    articleMetaDescription = await openAiApiClient.SubmitMessage($" {promptTextFormatted04} - again but shorter");
                    articleMetaDescription = TextHelpers.CleanText(articleMetaDescription);
                }

                // 05
                var promptTextRaw05 = File.ReadAllText(Path.Combine(fileDir, "05-ArticleTitle.txt"), Encoding.UTF8);
                var promptTextFormatted05 = FormatPromptTextKeyword(promptTextRaw05, keyword);
                var articleTitle = await openAiApiClient.SubmitMessage(promptTextFormatted05);
                articleTitle = TextHelpers.CleanTitle(articleTitle);

                while (articleTitle.Length > 65)
                {
                    System.Console.Write(".");
                    articleTitle = await openAiApiClient.SubmitMessage("shorter");
                    articleTitle = TextHelpers.CleanText(articleTitle);
                }

                while (articleTitle.Contains("The lowest number possible is 0."))
                {
                    System.Console.Write(".");
                    articleTitle = await openAiApiClient.SubmitMessage("Re-write this.");
                    articleTitle = TextHelpers.CleanText(articleTitle);
                }

                //06
                var promptTextRaw06 = File.ReadAllText(Path.Combine(fileDir, "06-ArticleBreadcrumb.txt"), Encoding.UTF8);
                var promptTextFormatted06 = FormatPromptTextKeyword(promptTextRaw06, keyword);
                var articleBreadcrumb = await openAiApiClient.SubmitMessage(promptTextFormatted06);
                articleBreadcrumb = TextHelpers.ParseBreadcrumb(articleBreadcrumb);

                //07
                var promptTextRaw07 = File.ReadAllText(Path.Combine(fileDir, "07-ArticleHeader.txt"), Encoding.UTF8);
                var promptTextFormatted07 = FormatPromptTextKeyword(promptTextRaw07, keyword);
                var articleHeader = await openAiApiClient.SubmitMessage(promptTextFormatted07);
                articleHeader = TextHelpers.CleanH1(articleHeader);

                var newPage = await sitePageManager.CreatePageAsync(new SitePage()
                {
                    SitePageSectionId = siteSection.SitePageSectionId,
                    AllowsComments = true,
                    BreadcrumbName = articleBreadcrumb,
                    Content = articleHtml,
                    ExcludePageFromSiteMapXml = false,
                    IsLive = true,
                    AuthorId = authorId,
                    Key = articleKey,
                    MetaDescription = articleMetaDescription,
                    PageHeader = articleHeader,
                    PageType = Data.Enums.PageType.Content,
                    PublishDateTimeUtc = DateTimeHelpers.GetRandomDateInRange(startDateTime, MinutesOffsetForArticleMin, MinutesOffsetForArticleMax),
                    Title = articleTitle,
                });

                if (newPage == null || newPage.SitePageId == 0)
                {
                    throw new Exception("could not create page");
                }
                else
                {
                    //08
                    var promptTextRaw08 = File.ReadAllText(Path.Combine(fileDir, "08-ArticleTags.txt"), Encoding.UTF8);
                    var promptTextFormatted08 = FormatPromptTextKeyword(promptTextRaw08, keyword);
                    var articleTags = await openAiApiClient.SubmitMessage(promptTextFormatted08);

                    var sitePageEditModel = new Managers.Models.SitePages.SitePageEditModel()
                    {
                        Tags = TextHelpers.CleanText(articleTags).Replace(".", string.Empty),
                        SitePageId = newPage.SitePageId,
                    };

                    sitePageManager.UpdateBlogTags(sitePageEditModel, newPage);

                    completed++;
                    System.Console.WriteLine(" - New Page: {0}/{1} - {2}", siteSection.Key, newPage.Key, newPage.PublishDateTimeUtc);
                }
            }

            WriteCompletionMessage();
        }
 
        private static string FormatPromptTextKeyword(string promptTextRaw, string keyword)
        {
            keyword = keyword.Trim();
            promptTextRaw = promptTextRaw.Replace(QuestionPlaceholder, keyword);
            promptTextRaw = promptTextRaw.Replace(QuestionPlaceholder, keyword);

            return promptTextRaw;
        }
    }
}