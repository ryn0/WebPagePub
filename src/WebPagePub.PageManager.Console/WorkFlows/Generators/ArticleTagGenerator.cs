using System.Text;
using WebPagePub.PageManager.Console.Interfaces;
using WebPagePub.PageManager.Console.Models.SettingsModels;
using WebPagePub.PageManager.Console.Utilities;
using WebPagePub.Core.Utilities;
using WebPagePub.Managers.Interfaces;

namespace WebPagePub.PageManager.Console.WorkFlows.Generators
{
    public class ArticleTagGenerator : BaseGenerator, IExecute
    {
        public const string TopicPlaceHolder = "[text]";

        public ArticleTagGenerator(
            OpenAiApiSettings chatGptSettings,
            ISitePageManager sitePageManager,
            ArticleTagGeneratorModel model) :
            base(chatGptSettings, sitePageManager)
        {
            base.SectionKey = model.SectionKey;
            base.OpenAiApiSettings = chatGptSettings;
            base.sitePageManager = sitePageManager;

        }
        public async Task Execute()
        {
            if (openAiApiClient == null)
            {
                throw new Exception($"{nameof(openAiApiClient)} is null");
            }

            WriteStartMessage(nameof(ArticleTagGenerator));
            var siteSection = sitePageManager.GetSiteSection(SectionKey);
            var pages = sitePageManager.GetSitePages(1, siteSection.SitePageSectionId, int.MaxValue, out _);
            var fileDir = Directory.GetCurrentDirectory() + @"\WorkFlows\Prompts\ArticleTagGenerator";

            var promptTextRaw00 = File.ReadAllText(Path.Combine(fileDir, "00-Setup.txt"), Encoding.UTF8);
            await openAiApiClient.SubmitMessage(promptTextRaw00);

            foreach (var page in pages)
            {
                var existingPage = sitePageManager.GetSitePage(page.SitePageId);

                if (existingPage == null ||
                    string.IsNullOrWhiteSpace(existingPage.Content) ||
                    (existingPage.SitePageTags != null &&
                    existingPage.SitePageTags.Count() > 0))
                {
                    continue;
                }

                //01
                var promptTextRaw01 = File.ReadAllText(Path.Combine(fileDir, "01-ArticleTags.txt"), Encoding.UTF8);
                var promptTextFormatted01 = FormatPromptText(promptTextRaw01, existingPage.Content);
                var articleTags = await openAiApiClient.SubmitMessage(promptTextFormatted01);

                var sitePageEditModel = new Managers.Models.SitePages.SitePageEditModel()
                {
                    Tags = TextHelpers.CleanText(articleTags).Replace(".", string.Empty),
                    SitePageId = existingPage.SitePageId,
                };

                sitePageManager.UpdateBlogTags(sitePageEditModel, existingPage);

                completed++;
                System.Console.WriteLine($"updated page tags for page id: {existingPage.SitePageId}");
            }

            WriteCompletionMessage();
        }

        private string FormatPromptText(string promptTextRaw, string text)
        {
            var textWithoutHtml = TextUtilities.StripHtml(text);
            var shortedTextWithoutHtml = TextHelpers.TruncateLongString(textWithoutHtml, 4000);

            if (shortedTextWithoutHtml == null)
            {
                return string.Empty;
            }

            var lastPeriod = shortedTextWithoutHtml.LastIndexOf(".");
            var textUntilLastSentence = shortedTextWithoutHtml.Substring(0, lastPeriod + 1);
            var response = promptTextRaw.Replace(TopicPlaceHolder, textUntilLastSentence);

            return response ?? string.Empty;
        }
    }
}
