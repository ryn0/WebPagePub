using CsvHelper.Configuration;
using CsvHelper;
using System.Globalization;
using WebPagePub.ChatCommander.Models.InputModels;
using WebPagePub.ChatCommander.Interfaces;
using WebPagePub.Managers.Interfaces;
using WebPagePub.Data.Models;
using WebPagePub.ChatCommander.Utilities;
using WebPagePub.ChatCommander.Helpers;

namespace WebPagePub.ChatCommander.WorkFlows.Generators
{
    public class ArticleInternalLinkGenerator : BaseGenerator, IPageEditor
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
                    Console.Write(record.Keyword);

                    var sourcePageUrl = record.SourcePage;

                    if (sourcePageUrl == null ||
                        !await WebPageChecker.IsWebPageOnlineAsync(sourcePageUrl))
                    {
                        Console.WriteLine(" - bad source page.");
                        continue;
                    }

                    var sourcePage = SitePageManager.GetPageForUrl(sourcePageUrl);

                    if (sourcePage == default(SitePage))
                    {
                        Console.WriteLine(" - no/invalid source page.");
                        continue;
                    }

                    var targetPageUrl = record.TargetPage;

                    if (targetPageUrl == null ||
                        !await WebPageChecker.IsWebPageOnlineAsync(targetPageUrl))
                    {
                        Console.WriteLine(" - bad target page.");
                        continue;
                    }

                    var targetPage = SitePageManager.GetPageForUrl(targetPageUrl);

                    if (targetPage == default(SitePage))
                    {
                        Console.WriteLine(" - no/invalid target page.");
                        continue;
                    }

                    var sourcePageContext = record.KeywordContext;
                    var sourcePageKeyword = record.Keyword;
                    var keywordExtactCase = TextHelpers.FindWithExactCasing(sourcePageContext, sourcePageKeyword);
                    var sourcePageContextWithLink = TextHelpers.CaseInsensitiveReplace(
                        record.KeywordContext,
                        keywordExtactCase,
                        $"<a href=\"{targetPageUrl}\">{keywordExtactCase}</a>");

                    var sourcePageContent = sourcePage.Content;

                    if (!TextHelpers.IsTextSurroundedByPTag(sourcePageContext, sourcePageContent))
                    {
                        Console.Write(" - text not in paragraph");

                        if (!TextHelpers.IsTextSurroundedByLiTag(sourcePageContext, sourcePageContent))
                        {
                            Console.Write(" - text not in list item");
                            Console.WriteLine();
                            continue;
                        }
                    }

                    var linkedPageContent = TextHelpers.FindAndReplace(sourcePageContent, sourcePageContext, sourcePageContextWithLink);

                    if (linkedPageContent == sourcePageContent)
                    {
                        Console.WriteLine(" - no changes.");
                    }
                    else
                    {
                        sourcePage.Content = linkedPageContent;
                        SitePageManager.UpdateSitePage(sourcePage);
                        Console.WriteLine(" - text changed.");
                    }
                    completed++;
                }
            }
            WriteCompletionMessage();
        }
    }
}