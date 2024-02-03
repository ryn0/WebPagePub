using CsvHelper.Configuration;
using CsvHelper;
using System.Globalization;
using WebPagePub.PageManager.Console.Models.InputModels;
using WebPagePub.PageManager.Console.Interfaces;
using WebPagePub.Managers.Interfaces;
using WebPagePub.Data.Models;
using WebPagePub.PageManager.Console.Utilities;
using WebPagePub.PageManager.Console.Helpers;

namespace WebPagePub.PageManager.Console.WorkFlows.Generators
{
    public class ArticleInternalLinkGenerator : BaseGenerator, IExecute
    {
        public ArticleInternalLinkGenerator(ISitePageManager sitePageManager) :
            base(sitePageManager)
        {
            SitePageManager = sitePageManager;
        }

        public ISitePageManager SitePageManager { get; }

        public async Task Execute()
        {
            WriteStartMessage(GetType().Name);

            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                Delimiter = "\t",
                HasHeaderRecord = true
            };

            var fileDir = Directory.GetCurrentDirectory() + @"\WorkFlows\Prompts\ArticleInternalLinks";

            // Get all CSV files in the directory and select the first one
            var csvFiles = Directory.GetFiles(fileDir, "*.csv");
            if (csvFiles.Length == 0)
            {
                throw new FileNotFoundException($"No CSV files found in '{fileDir}'.");
            }

            var pathToCSv = csvFiles[0];

            if (!File.Exists(pathToCSv))
            {
                throw new FileNotFoundException($"File not found: {pathToCSv}");
            }

            using (var reader = new StreamReader(pathToCSv))
            using (var csv = new CsvReader(reader, config))
            {
                var records = csv.GetRecords<AhrefsInternalLinkModel>().ToList();
                
                foreach (var record in records)
                {
                    System.Console.Write(record.Keyword);

                    var sourcePageUrl = record.SourcePage;
 
                    if (sourcePageUrl == null ||
                        !await WebPageChecker.IsWebPageOnlineAsync(sourcePageUrl))
                    {
                        System.Console.WriteLine(" - bad source page.");
                        continue;
                    }

                    var sourcePage = SitePageManager.GetPageForUrl(sourcePageUrl);

                    if (sourcePage == default(SitePage))
                    {
                        System.Console.WriteLine(" - no/invalid source page.");
                        continue;
                    }

                    var targetPageUrl = record.TargetPage;

                    if (targetPageUrl == null ||
                        !await WebPageChecker.IsWebPageOnlineAsync(targetPageUrl))
                    {
                        System.Console.WriteLine(" - bad target page.");
                        continue;
                    }

                    var targetPage = SitePageManager.GetPageForUrl(targetPageUrl);

                    if (targetPage == default(SitePage))
                    {
                        System.Console.WriteLine(" - no/invalid target page.");
                        continue;
                    }

                    var sourcePageContext = record.KeywordContext;
                    var sourcePageKeyword = record.Keyword;
                    var keywordExtactCase = TextHelpers.FindWithExactCasing(sourcePageContext, sourcePageKeyword);
                    var linkToPlace = $"<a href=\"{targetPageUrl}\">{keywordExtactCase}</a>";
                    var sourcePageContent = sourcePage.Content;
                    var linkedPageContent = TextHelpers.InsertLinkInHtml(sourcePageContent, sourcePageContext, sourcePageKeyword, linkToPlace);

                    if (linkedPageContent == sourcePageContent)
                    {
                        System.Console.WriteLine(" - no changes.");
                    }
                    else
                    {
                        sourcePage.Content = linkedPageContent;
                        SitePageManager.UpdateSitePage(sourcePage);
                        System.Console.WriteLine(" - text changed.");
                    }
                    completed++;
                }
            }
            WriteCompletionMessage();
        }
    }
}