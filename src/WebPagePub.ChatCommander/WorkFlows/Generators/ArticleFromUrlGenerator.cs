﻿using HtmlAgilityPack;
using System.Text;
using System.Xml.Serialization;
using System.Xml;
using WebPagePub.ChatCommander.Interfaces;
using WebPagePub.ChatCommander.Models.InputModels;
using WebPagePub.ChatCommander.Models.SettingsModels;
using WebPagePub.ChatCommander.SettingsModels;
using WebPagePub.ChatCommander.Utilities;
using WebPagePub.Data.Models;
using WebPagePub.Managers.Interfaces;
using WebPagePub.ChatCommander.Helpers;
using static System.Net.Mime.MediaTypeNames;
using WebPagePub.Core.Utilities;

namespace WebPagePub.ChatCommander.WorkFlows.Generators
{
    public class ArticleFromUrlGenerator : BaseGenerator, IPageEditor
    {
        const string InputTextPlaceholder = "[text]";
        const string PageTitlePlaceHolder = "[title]";
        const string DefaultHeader = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/109.0.0.0 Safari/537.36";
        const int SiteMapSkipCount = 0;
        private string ContentExtractionXPath { get; set; }
        private string SiteMapUrl { get; set; }
        private string SiteMapUrlContentIncludeWords { get; set; }

        public ArticleFromUrlGenerator(
            OpenAiApiSettings chatGptSettings,
            ISitePageManager sitePageManager,
            ArticleFromUrlGeneratorModel model) : 
            base (chatGptSettings, sitePageManager)
        {
            this.SiteMapUrl = model.SiteMapUrl;
            this.SiteMapUrlContentIncludeWords = model.SiteMapUrlContentIncludeWords;
            this.ContentExtractionXPath = model.ContentExtractionXPath;
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

            var siteMapUrls = await GetSiteMapUrlsAsync();

            var fileDir = Directory.GetCurrentDirectory() + @"\WorkFlows\Prompts\ArticlesFromUrl";

            // 00
            var promptTextRaw00 = File.ReadAllText(Path.Combine(fileDir, "00-Setup.txt"), Encoding.UTF8);
            await openAiApiClient.SubmitMessage(promptTextRaw00);

            // 01
            Console.Write($"Loading URLs...");
            var urls = File.ReadAllText(Path.Combine(fileDir, "01-LoadUrls.txt"), Encoding.UTF8);
            var urlList = TextHelpers.GetUniqueLines(urls);
            var allUrls = siteMapUrls.ToArray().Union(urlList);
            Console.WriteLine($"{allUrls.Count()} found.");

            foreach (var url in allUrls)
            {
                if (string.IsNullOrWhiteSpace(url) || !Uri.TryCreate(url!, UriKind.Absolute, out Uri? uri) || uri == null)
                {
                    continue;
                }

                Console.Write($"Working on article from: {url}...");

                var extractedText = await ExtractTextFromUrlAsync(uri);
                extractedText = TextHelpers.ReduceSpaces(extractedText);
                var extractedTextShortened = TextHelpers.TruncateLongString(extractedText, 4000);

                if (string.IsNullOrWhiteSpace(extractedTextShortened))
                {
                    continue;
                }

                // 02
                var promptTextRaw02 = File.ReadAllText(Path.Combine(fileDir, "02-ArticleHtmlBody.txt"), Encoding.UTF8);
                var promptTextFormatted02 = FormatPromptTextInputText(promptTextRaw02, extractedTextShortened);

                openAiApiClient.MaxTokens = 2000;
                var articleHtml = await openAiApiClient.SubmitMessage(promptTextFormatted02);
                articleHtml = TextHelpers.RemoveNonHtmlTextAtStart(TextHelpers.HtmlDecode(articleHtml.Trim()));

                var maxAttempts = 3;
                var attemptCount = 1;

                while (!articleHtml.EndsWith("</p>") && attemptCount < maxAttempts)
                {
                    Console.Write(".");
                    openAiApiClient.MaxTokens = 1000;
                    articleHtml = await openAiApiClient.SubmitMessage(promptTextFormatted02 + " Write this shorter than before.");
                    articleHtml = articleHtml.Trim();
                    attemptCount++;
                }

                openAiApiClient.MaxTokens = 1000;

                if (attemptCount > maxAttempts)
                {
                    Console.WriteLine(" - HTML not formatted correctly at end");
                    continue;
                }

                // 03
                var promptTextRaw03 = File.ReadAllText(Path.Combine(fileDir, "03-ArticleTitle.txt"), Encoding.UTF8);
                var promptTextFormatted03 = FormatPromptTextInputText(promptTextRaw03, articleHtml);
                var articleTitle = await openAiApiClient.SubmitMessage(promptTextFormatted03);
                articleTitle = TextHelpers.CleanTitle(articleTitle);

                while (articleTitle.Length > 65 && attemptCount < maxAttempts)
                {
                    Console.Write(".");
                    articleTitle = await openAiApiClient.SubmitMessage(
                        $" This title: '{articleTitle}' is {articleTitle.Length} characters long. It needs to be much shorter. Give the title again but in less than 60 characters this time.");
                    articleTitle = TextHelpers.CleanText(articleTitle);
                    attemptCount++;
                }

                if (articleTitle.Length > 65 && attemptCount >= maxAttempts)
                {
                    Console.WriteLine("title too long.");
                    continue;
                }

                // 04
                var promptTextRaw04 = File.ReadAllText(Path.Combine(fileDir, "04-ArticleKey.txt"), Encoding.UTF8);

                var promptTextFormatted04 = FormatPromptTextTitle(promptTextRaw04, articleTitle);
                var articleKey = await openAiApiClient.SubmitMessage(promptTextFormatted04);

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

                // 05
                attemptCount = 0;
                var promptTextRaw05 = File.ReadAllText(Path.Combine(fileDir, "05-ArticleMetaDescription.txt"), Encoding.UTF8);
                var promptTextFormatted05 = FormatPromptTextTitle(promptTextRaw05, articleTitle);
                var articleMetaDescription = await openAiApiClient.SubmitMessage(promptTextFormatted05);
                articleMetaDescription = TextHelpers.CleanMetaDescription(articleMetaDescription);

                while (articleMetaDescription.Length > 160 && attemptCount < maxAttempts)
                {
                    Console.Write(".");
                    articleMetaDescription = await openAiApiClient.SubmitMessage(
                        $" This meta description '{articleMetaDescription}' is {articleMetaDescription.Length} characters, which is too long. Re-write it to be 150 characters maximum.");
                    articleMetaDescription = TextHelpers.CleanMetaDescription(articleMetaDescription);
                    attemptCount++;
                }

                if (articleMetaDescription.Length > 160 && attemptCount >= maxAttempts)
                {
                    Console.WriteLine("Meta description too long.");
                    continue;
                }

                //06
                var promptTextRaw06 = File.ReadAllText(Path.Combine(fileDir, "06-ArticleBreadcrumb.txt"), Encoding.UTF8);
                var promptTextFormatted06 = FormatPromptTextInputText(promptTextRaw06, articleHtml);
                var articleBreadcrumb = await openAiApiClient.SubmitMessage(promptTextFormatted06);
                articleBreadcrumb = TextHelpers.ParseBreadcrumb(articleBreadcrumb);

                if (string.IsNullOrWhiteSpace(articleBreadcrumb))
                {
                    Console.WriteLine("no breadcrumb.");
                    continue;
                }

                //07
                var promptTextRaw07 = File.ReadAllText(Path.Combine(fileDir, "07-ArticleHeader.txt"), Encoding.UTF8);
                var promptTextFormatted07 = FormatPromptTextInputText(promptTextRaw07, articleHtml);
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
                    var promptTextFormatted08 = FormatPromptTextInputText(promptTextRaw08, articleHtml);
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

        private async Task<List<string>> GetSiteMapUrlsAsync()
        {
            var listOfUrls = new List<string>();
            if (string.IsNullOrWhiteSpace(this.SiteMapUrl))
            {
                return listOfUrls;
            }

            Console.Write("Fetching sitemap URLs...");

            string[]? allowWords;

            if (string.IsNullOrWhiteSpace(this.SiteMapUrlContentIncludeWords))
            {
                allowWords = null;
            }
            else
            {
                allowWords = Array.ConvertAll(SiteMapUrlContentIncludeWords.Split(','), p => p.Trim());
            }

            var siteMapUrlsRaw = await GetResponse(new Uri(this.SiteMapUrl));
            var siteMapUrlsParsed = Deserialize<UrlSet>(siteMapUrlsRaw);

            foreach (var siteMapUrl in siteMapUrlsParsed.Url)
            {
                Console.Write(".");

                if (siteMapUrl == null || 
                    string.IsNullOrWhiteSpace(siteMapUrl.Loc))
                {
                    continue;
                }

                var url = siteMapUrl.Loc;

                if (allowWords != null)
                {
                    var response = await ExtractTextFromUrlAsync(new Uri(url));
                    var responseFormatted = TextHelpers.ReduceSpaces(response);

                    foreach (var word in allowWords)
                    {
                        if (!listOfUrls.Contains(url) &&
                            responseFormatted.Contains(word))
                        {
                            listOfUrls.Add(siteMapUrl.Loc);
                            continue;
                        }
                    }
                }
                else
                {
                    listOfUrls.Add(url);
                }
            }

            Console.Write($"{listOfUrls.Count()} URLs found.");
            Console.WriteLine();

            return listOfUrls.Skip(SiteMapSkipCount).ToList();
        }

        private string FormatPromptTextInputText(string promptTextRaw, string keyword)
        {
            keyword = keyword.Trim();
            promptTextRaw = promptTextRaw.Replace(InputTextPlaceholder, keyword);

            return promptTextRaw;
        }

        private string FormatPromptTextTitle(string promptTextRaw, string title)
        {
            title = title.Trim();
            promptTextRaw = promptTextRaw.Replace(PageTitlePlaceHolder, title);

            return promptTextRaw;
        }

        private async Task<string> ExtractTextFromUrlAsync(Uri uri)
        {
            try
            {
                string htmlContent;

                using (HttpClient client = new())
                {
                    client.DefaultRequestHeaders.UserAgent.ParseAdd(DefaultHeader);

                    using HttpResponseMessage response = await client.GetAsync(uri);
                    using HttpContent content = response.Content;
                    htmlContent = await content.ReadAsStringAsync();
                }

                HtmlDocument doc = new();
                doc.LoadHtml(htmlContent);

                string rawText = doc.DocumentNode.SelectSingleNode(ContentExtractionXPath).InnerText;
                var finalText = TextUtilities.StripHtml(rawText);

                return finalText;
            }
            catch
            {
                return string.Empty;
            }
        }

        private async Task<string> GetResponse(Uri uri)
        {
            try
            {
                string rawContent;

                using (HttpClient client = new())
                {
                    client.DefaultRequestHeaders.UserAgent.ParseAdd(DefaultHeader);

                    using HttpResponseMessage response = await client.GetAsync(uri);
                    using HttpContent content = response.Content;
                    rawContent = await content.ReadAsStringAsync();
                }

                return rawContent;
            }
            catch
            {
                return string.Empty;
            }
        }

        public static T Deserialize<T>(string xml) where T : class
        {
            if (string.IsNullOrEmpty(xml))
            {
                throw new NotSupportedException("Empty string!!");
            }

            try
            {
                var xmlserializer = new XmlSerializer(typeof(T));
                using var stringReader = new StringReader(xml);
                using var reader = XmlReader.Create(stringReader);
                var result = xmlserializer.Deserialize(reader) as T;

                if (result == null)
                {
                    throw new InvalidOperationException("Deserialization resulted in a null object.");
                }

                return result;
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

    }
}