﻿using System.Text;
using WebPagePub.ChatCommander.Interfaces;
using WebPagePub.ChatCommander.SettingsModels;
using WebPagePub.ChatCommander.Utilities;
using WebPagePub.Data.Models;
using WebPagePub.Managers.Interfaces;

namespace WebPagePub.ChatCommander.WorkFlows.Generators
{
    public class ArticleFromKeywordsGenerator : BaseGenerator, IPageEditor
    {
        const string QuestionPlaceholder = "[keyword]";

        public ArticleFromKeywordsGenerator(
            ChatGptSettings chatGptSettings,
            ISitePageManager sitePageManager,
            ArticleFromKeywordsGeneratorModel model) : 
            base (chatGptSettings, sitePageManager)
        {
            base.SectionKey = model.SectionKey;
            base.MinutesOffsetForArticleMin = model.MinutesOffsetForArticleMin;
            base.MinutesOffsetForArticleMax = model.MinutesOffsetForArticleMax;
            base.chatGptSettings = chatGptSettings;
            base.sitePageManager = sitePageManager;
        }

        public async Task CreatePagesAsync()
        {
            startDateTime = DateTime.UtcNow;
            WriteStartMessage(GetType().Name);

            int? authorId = GetAuthor();

            var siteSection = sitePageManager.GetSiteSection(SectionKey);

            if (siteSection == null)
            {
                throw new Exception("Site section missing");
            }

            var fileDir = Directory.GetCurrentDirectory() + @"\WorkFlows\Prompts\ArticlesFromKeywords";

            // 00
            var promptTextRaw00 = File.ReadAllText(Path.Combine(fileDir, "00-Setup.txt"), Encoding.UTF8);
            await chatGPT.SubmitMessage(promptTextRaw00);

            // 01
            Console.Write($"Loading keywords...");
            var keywords = File.ReadAllText(Path.Combine(fileDir, "01-LoadKeywords.txt"), Encoding.UTF8);
            var keywordList = TextHelpers.GetUniqueLines(keywords);
            Console.WriteLine($"{keywordList.Count()} found.");

            // 02
            var promptTextRaw02 = File.ReadAllText(Path.Combine(fileDir, "02-ArticleKey.txt"), Encoding.UTF8);

            foreach (var keyword in keywordList)
            {
                if (string.IsNullOrWhiteSpace(keyword))
                {
                    continue;
                }

                var promptTextFormatted02 = FormatPromptTextKeyword(promptTextRaw02, keyword);
                var articleKey = await chatGPT.SubmitMessage(promptTextFormatted02);

                articleKey = TextHelpers.CleanArticleKey(articleKey);

                if (string.IsNullOrWhiteSpace(articleKey))
                {
                    Console.WriteLine("Invalid article key");
                    continue;
                }

                if (sitePageManager.DoesPageExist(siteSection.SitePageSectionId, articleKey))
                {
                    Console.WriteLine($"'{articleKey}' exists");
                    continue;
                }

                if (sitePageManager.DoesPageExist(siteSection.SitePageSectionId, string.Format("{0}s", articleKey)))
                {
                    Console.WriteLine($"'{articleKey}+s' exists");
                    continue;
                }
                if (articleKey.EndsWith("s") &&
                    sitePageManager.DoesPageExist(siteSection.SitePageSectionId, articleKey.Remove(articleKey.Length - 1, 1)))
                {
                    Console.WriteLine($"'{articleKey}-s' exists");
                    continue;
                }

                Console.Write($"Creating article for: {keyword}...");

                // 03
                var promptTextRaw03 = File.ReadAllText(Path.Combine(fileDir, "03-ArticleHtmlBody.txt"), Encoding.UTF8);
                var promptTextFormatted03 = FormatPromptTextKeyword(promptTextRaw03, keyword);

                chatGPT.MaxTokens = 2000;
                var articleHtml = await chatGPT.SubmitMessage(promptTextFormatted03);
                articleHtml = articleHtml.Trim();

                var maxAttemptsAtHtmlBody = 3;
                var attemptsAtHtmlBody = 1;

                while (!articleHtml.EndsWith("</p>") && attemptsAtHtmlBody < maxAttemptsAtHtmlBody)
                {
                    Console.Write(".");
                    chatGPT.MaxTokens = 1000;
                    articleHtml = await chatGPT.SubmitMessage(promptTextFormatted03 + " Write this shorter than before.");
                    articleHtml = articleHtml.Trim();
                    attemptsAtHtmlBody++;
                }

                chatGPT.MaxTokens = 1000;

                if (attemptsAtHtmlBody > maxAttemptsAtHtmlBody)
                {
                    Console.WriteLine(" - HTML not formatted correctly at end");
                    continue;
                }

                // 04
                var promptTextRaw04 = File.ReadAllText(Path.Combine(fileDir, "04-ArticleMetaDescription.txt"), Encoding.UTF8);
                var promptTextFormatted04 = FormatPromptTextKeyword(promptTextRaw04, keyword);
                var articleMetaDescription = await chatGPT.SubmitMessage(promptTextFormatted04);
                articleMetaDescription = TextHelpers.CleanText(articleMetaDescription);

                while (articleMetaDescription.Length > 160)
                {
                    Console.Write(".");
                    articleMetaDescription = await chatGPT.SubmitMessage($" {promptTextFormatted04} - again but shorter");
                    articleMetaDescription = TextHelpers.CleanText(articleMetaDescription);
                }

                // 05
                var promptTextRaw05 = File.ReadAllText(Path.Combine(fileDir, "05-ArticleTitle.txt"), Encoding.UTF8);
                var promptTextFormatted05 = FormatPromptTextKeyword(promptTextRaw05, keyword);
                var articleTitle = await chatGPT.SubmitMessage(promptTextFormatted05);
                articleTitle = TextHelpers.CleanTitle(articleTitle);

                while (articleTitle.Length > 65)
                {
                    Console.Write(".");
                    articleTitle = await chatGPT.SubmitMessage("shorter");
                    articleTitle = TextHelpers.CleanText(articleTitle);
                }

                while (articleTitle.Contains("The lowest number possible is 0."))
                {
                    Console.Write(".");
                    articleTitle = await chatGPT.SubmitMessage("Re-write this.");
                    articleTitle = TextHelpers.CleanText(articleTitle);
                }

                //06
                var promptTextRaw06 = File.ReadAllText(Path.Combine(fileDir, "06-ArticleBreadcrumb.txt"), Encoding.UTF8);
                var promptTextFormatted06 = FormatPromptTextKeyword(promptTextRaw06, keyword);
                var articleBreadcrumb = await chatGPT.SubmitMessage(promptTextFormatted06);
                articleBreadcrumb = TextHelpers.ParseBreadcrumb(articleBreadcrumb);

                //07
                var promptTextRaw07 = File.ReadAllText(Path.Combine(fileDir, "07-ArticleHeader.txt"), Encoding.UTF8);
                var promptTextFormatted07 = FormatPromptTextKeyword(promptTextRaw07, keyword);
                var articleHeader = await chatGPT.SubmitMessage(promptTextFormatted07);
                articleHeader = TextHelpers.CleanH1(articleHeader);

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
                    PublishDateTimeUtc = OffSetTime(startDateTime),
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
                    var articleTags = await chatGPT.SubmitMessage(promptTextFormatted08);

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
 
        private string FormatPromptTextKeyword(string promptTextRaw, string keyword)
        {
            keyword = keyword.Trim();
            promptTextRaw = promptTextRaw.Replace(QuestionPlaceholder, keyword);
            promptTextRaw = promptTextRaw.Replace(QuestionPlaceholder, keyword);

            return promptTextRaw;
        }
    }
}