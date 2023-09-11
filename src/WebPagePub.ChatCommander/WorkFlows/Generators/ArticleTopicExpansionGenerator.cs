using System.Text;
using WebPagePub.ChatCommander.Helpers;
using WebPagePub.ChatCommander.Interfaces;
using WebPagePub.ChatCommander.Models.SettingsModels;
using WebPagePub.ChatCommander.Utilities;
using WebPagePub.Data.Models;
using WebPagePub.Managers.Interfaces;

namespace WebPagePub.ChatCommander.WorkFlows.Generators
{
    public class ArticleTopicExpansionGenerator : BaseGenerator, IPageEditor
    {
        private readonly string Topic;
        private readonly int QuestionQuantity;
        private const string questionPlaceholder = "[question]";
        private const string QuantityPlaceholder = "[quantity]";
        private const string TopicPlaceholder = "[topic]";

        public ArticleTopicExpansionGenerator(
               OpenAiApiSettings openAiApiSettings,
               ISitePageManager sitePageManager,
               ArticleTopicExpansionGeneratorModel model) :
               base(openAiApiSettings, sitePageManager)
        {
            base.SectionKey = model.SectionKey;
            base.MinutesOffsetForArticleMin = model.MinutesOffsetForArticleMin;
            base.MinutesOffsetForArticleMax = model.MinutesOffsetForArticleMax;
            this.QuestionQuantity = model.QuestionQuantity;
            this.Topic = model.Topic;
            base.OpenAiApiSettings = openAiApiSettings;
            base.sitePageManager = sitePageManager;
        }

        public async Task Execute()
        {
            if (openAiApiClient == null)
            {
                throw new Exception($"{nameof(openAiApiClient)} is null");
            }

            startDateTime = DateTime.UtcNow;
            WriteStartMessage(GetType().Name);

            int? authorId = GetAuthor();

            var siteSection = sitePageManager.GetSiteSection(SectionKey);

            if (siteSection == null)
            {
                throw new Exception("Site section missing");
            }

            var fileDir = Directory.GetCurrentDirectory() + @"\WorkFlows\Prompts\ArticleTopicExpansion";

            // 00
            var promptTextRaw00 = File.ReadAllText(Path.Combine(fileDir, "00-Setup.txt"), Encoding.UTF8);
            await openAiApiClient.SubmitMessage(promptTextRaw00);

            // 01
            Console.Write($"Getting {QuestionQuantity} questions for '{Topic}'...");
            var promptTextRaw01 = File.ReadAllText(Path.Combine(fileDir, "01-Questions.txt"), Encoding.UTF8);
            var promptTextFormatted01 = FormatPromptTopicAndQuantity(promptTextRaw01);
            var questionsPreformatted01 = await openAiApiClient.SubmitMessage(promptTextFormatted01);
            var questionList = questionsPreformatted01.Split(",");
            Console.WriteLine($"done.");

            // 02
            var promptTextRaw02 = File.ReadAllText(Path.Combine(fileDir, "02-ArticleKey.txt"), Encoding.UTF8);

            foreach (var question in questionList)
            {
                var attemptsMade = 1;
                var promptTextFormatted02 = FormatPromptTextQuestion(promptTextRaw02, question);
                var articleKey = await openAiApiClient.SubmitMessage(promptTextFormatted02);

                articleKey = TextHelpers.CleanArticleKey(articleKey);

                if (string.IsNullOrWhiteSpace(articleKey))
                {
                    Console.WriteLine("Invalid article key");
                    continue;
                }

                if (sitePageManager.DoesPageExistSimilar(siteSection.SitePageSectionId, articleKey))
                {
                    Console.WriteLine($"'{articleKey}' (or similar) exists");
                    continue;
                }

                Console.Write($"Creating article for {question}...");

                // 03
                var promptTextRaw03 = File.ReadAllText(Path.Combine(fileDir, "03-ArticleTitle.txt"), Encoding.UTF8);
                var promptTextFormatted03 = FormatPromptTextQuestion(promptTextRaw03, question);
                var articleTitle = await openAiApiClient.SubmitMessage(promptTextFormatted03);
                articleTitle = TextHelpers.CleanText(articleTitle);

                while (articleTitle.Length > 65 && (attemptsMade < maxAttempts))
                {
                    articleTitle = await openAiApiClient.SubmitMessage(promptTextFormatted03 + " - write it shorter");
                    articleTitle = TextHelpers.CleanText(articleTitle);
                    attemptsMade++;
                }

                while (articleTitle.Contains("The lowest number possible is 0.") && (attemptsMade < maxAttempts))
                {
                    articleTitle = await openAiApiClient.SubmitMessage(promptTextFormatted03 + " - Re-write this.");
                    articleTitle = TextHelpers.CleanText(articleTitle);
                    attemptsMade++;
                }

                if (attemptsMade > maxAttempts)
                {
                    Console.WriteLine(" - Article title is wrong");
                    continue;
                }

                // 04
                var promptTextRaw04 = File.ReadAllText(Path.Combine(fileDir, "04-ArticleMetaDescription.txt"), Encoding.UTF8);
                var promptTextFormatted04 = FormatPromptTextQuestion(promptTextRaw04, question);
                var articleMetaDescription = await openAiApiClient.SubmitMessage(promptTextFormatted04);
                articleMetaDescription = TextHelpers.CleanText(articleMetaDescription);

                while (articleMetaDescription.Length > 160 && (attemptsMade < maxAttempts))
                {
                    articleMetaDescription = await openAiApiClient.SubmitMessage(articleMetaDescription + " - write this shorter");
                    articleMetaDescription = TextHelpers.CleanText(articleMetaDescription);
                    attemptsMade++;
                }

                if (attemptsMade > maxAttempts)
                {
                    Console.WriteLine(" - Meta description is too short");
                    continue;
                }

                // 05
                var promptTextRaw05 = File.ReadAllText(Path.Combine(fileDir, "05-ArticleHtmlBody.txt"), Encoding.UTF8);
                var promptTextFormatted05 = FormatPromptTextTopicAndQuestion(promptTextRaw05, question);
                var articleHtml = await openAiApiClient.SubmitMessage(promptTextFormatted05);
                articleHtml = articleHtml.Trim();

                while (!articleHtml.EndsWith("</p>") && (attemptsMade < maxAttempts))
                {
                    articleHtml = await openAiApiClient.SubmitMessage(promptTextFormatted05 + " - Write this shorter than before.");
                    articleHtml = articleHtml.Trim();
                    attemptsMade++;
                }

                if (attemptsMade > maxAttempts)
                {
                    Console.WriteLine(" - HTML not formatted correctly at end");
                    continue;
                }

                // 06
                var promptTextRaw06 = File.ReadAllText(Path.Combine(fileDir, "06-ArticleHeader.txt"), Encoding.UTF8);
                var promptTextFormatted06 = FormatPromptTextQuestion(promptTextRaw06, question);
                var articleHeader = await openAiApiClient.SubmitMessage(promptTextFormatted06);
                articleHeader = TextHelpers.CleanH1(articleHeader);

                //07
                var promptTextRaw07 = File.ReadAllText(Path.Combine(fileDir, "07-ArticleBreadcrumb.txt"), Encoding.UTF8);
                var promptTextFormatted07 = FormatPromptTextQuestion(promptTextRaw07, question);
                var articleBreadcrumb = await openAiApiClient.SubmitMessage(promptTextFormatted07);
                articleBreadcrumb = TextHelpers.ParseBreadcrumb(articleBreadcrumb);

                var newPage = sitePageManager.CreatePage(new SitePage()
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
                    var promptTextFormatted08 = FormatPromptTextQuestion(promptTextRaw08, question);
                    var articleTags = await openAiApiClient.SubmitMessage(promptTextFormatted08);

                    var sitePageEditModel = new Managers.Models.SitePages.SitePageEditModel()
                    {
                        Tags = TextHelpers.CleanText(articleTags).Replace(".", string.Empty),
                        SitePageId = newPage.SitePageId,
                    };

                    sitePageManager.UpdateBlogTags(sitePageEditModel, newPage);

                    completed++;
                    Console.WriteLine(" - New Page: {0}/{1} - {2}", siteSection.Key, newPage.Key, newPage.PublishDateTimeUtc);
                }
            }

            WriteCompletionMessage();
        }
 
        private string FormatPromptTextQuestion(string promptTextRaw, string question)
        {
            promptTextRaw = promptTextRaw.Replace(questionPlaceholder, question);
            promptTextRaw = promptTextRaw.Replace(questionPlaceholder, question);

            return promptTextRaw;
        }

        private string FormatPromptTextTopicAndQuestion(string promptTextRaw, string question)
        {
            promptTextRaw = promptTextRaw.Replace(TopicPlaceholder, Topic);
            promptTextRaw = promptTextRaw.Replace(questionPlaceholder, question);

            return promptTextRaw;
        }

        private string FormatPromptTopicAndQuantity(string promptTextRaw)
        {
            promptTextRaw = promptTextRaw.Replace(TopicPlaceholder, Topic);
            promptTextRaw = promptTextRaw.Replace(QuantityPlaceholder, QuestionQuantity.ToString());

            return promptTextRaw;
        }

    }
}
