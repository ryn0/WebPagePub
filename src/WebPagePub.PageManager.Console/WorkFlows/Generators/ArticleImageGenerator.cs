using WebPagePub.PageManager.Console.Models.ChatModels;
using WebPagePub.PageManager.Console.Models.SettingsModels;
using WebPagePub.Managers.Interfaces;
using WebPagePub.PageManager.Console.Interfaces;
using OpenAI_API.Moderation;
using Microsoft.Identity.Client;
using WebPagePub.Core.Utilities;
using Azure;
using System.Linq;
using WebPagePub.Core;
using WebPagePub.Data.Enums;
using WebPagePub.Data.Repositories.Interfaces;
using System.Diagnostics;

namespace WebPagePub.PageManager.Console.WorkFlows.Generators
{
    public class ArticleImageGenerator : BaseGenerator, IExecute
    {
        private readonly OpenAiApiClient OpenAiApiClient;
        private readonly ArticleImageGeneratorModel ArticleImageGeneratorModel;
        private readonly IContentSnippetRepository contentSnippetRepository;

        public ArticleImageGenerator(
            OpenAiApiSettings chatGptSettings,
            ArticleImageGeneratorModel model,
            ISitePageManager sitePageManager,
            IContentSnippetRepository contentSnippetRepository) :
            base(chatGptSettings, sitePageManager)
        {
            base.OpenAiApiSettings = chatGptSettings;
            this.ArticleImageGeneratorModel = model;
            base.sitePageManager = sitePageManager;
            this.contentSnippetRepository = contentSnippetRepository;
            this.OpenAiApiClient = new OpenAiApiClient(OpenAiApiSettings);

        }

        public async Task Execute()
        {
            // for the given site section, get all the pages
            // for each page, see if it has an image in the content body
            // if it does not have an image, determine what kind of image it should have
            // given this statement, submit a request to get back and image
            // download the image
            // upload the image with the name of the request in the file name
            // get back the thumb version of the image, place the CDN version of the link at the top of the page
            // update the page

            var blobPrefix = this.contentSnippetRepository.Get(SiteConfigSetting.BlobPrefix).Content;
            var cdnPrefix = this.contentSnippetRepository.Get(SiteConfigSetting.CdnPrefixWithProtocol).Content;
            var sitePageSectionName = ArticleImageGeneratorModel.SectionKey;
            var sitePageSection = this.sitePageManager.GetSiteSection(sitePageSectionName);
            var directoryToDownloadTo = Directory.GetCurrentDirectory() + @"\downloaded-images\";
            
            if (sitePageSection == null)
            {
                throw new Exception("section null");
            }

            var allPage = this.sitePageManager
                              .GetSitePages(1, sitePageSection.SitePageSectionId, int.MaxValue, out int total)
                              .Where(x => x.IsLive == true);

            foreach (var page in allPage)
            {
                if (string.IsNullOrEmpty(page.Content) || page.Content.Contains(@"<img"))
                {
                    // skip pages with an image
                    continue;
                }

                var pageTitle = page.Title;
                var imageUrl = GetImageUrlFromPromptAsync(pageTitle).Result;

                if (string.IsNullOrWhiteSpace(imageUrl))
                {
                    System.Console.WriteLine("failed to make image");
                    continue;
                }

                var localImagePath = DownloadImageAsync(imageUrl, directoryToDownloadTo).Result;
                var newFilePath = FileNameUtilities.ChangeFilename(localImagePath, pageTitle.UrlKey());
                FileInfo fi = new(localImagePath);
                fi.MoveTo(newFilePath);
                var imageThumbUrl = UploadFile(page.SitePageId, newFilePath).Result;
                var cdnUrl = UrlBuilder.ConvertBlobToCdnUrl(imageThumbUrl, blobPrefix, cdnPrefix);               
                var imageTag = string.Format(@"<img src=""{0}"" alt=""{1}"" />", cdnUrl, TextUtilities.RemoveNonAlphaNumeric(pageTitle));
                var pageContentWithImage = imageTag + Environment.NewLine + page.Content;
                page.Content = pageContentWithImage;
                await this.sitePageManager.UpdateSitePage(page);
                System.Console.Write(".");
            }
        }

        private async Task<string> UploadFile(int sitePageId, string localImagePath)
        {
            MemoryStream memoryStream = ImageUtilities.ConvertFileToMemoryStream(localImagePath);
            memoryStream.Position = 0;

            var fileName = Path.GetFileName(localImagePath);
            var images = new List<Tuple<string, MemoryStream>>
            {
                new(fileName, memoryStream)
            };
            
            await this.sitePageManager.UploadPhotos(sitePageId, images);

            memoryStream.Dispose();

            var allImages = this.sitePageManager.GetBlogPhotos(sitePageId);
            var lastImage = allImages.ToList().OrderByDescending(x => x.CreateDate).FirstOrDefault();

            return lastImage.PhotoThumbUrl;
        }

        private async Task<string> GetImageUrlFromPromptAsync(string pageTitle)
        {
            try
            {
                var result = await this.OpenAiApiClient.GenerateImage(new ImageInputRequest()
                {
                    Prompt = pageTitle,
                });

                if (result == null || result.Data == null || result.Data[0] == null || result.Data[0].Url == null)
                {
                    throw new Exception($"Failed to generate image: {pageTitle}");
                }

                if (!result.Success)
                {
                    return string.Empty;
                }

                return result.Data[0].Url;
            }
            catch
            {
                return string.Empty;
                //throw new Exception("could not make image", ex.InnerException);
            }
        }
 
        public async Task<string> DownloadImageAsync(string imageUrl, string folderPath)
        {
            // Ensure the target directory exists
            Directory.CreateDirectory(folderPath); // This will do nothing if the directory already exists

            // Extract the filename from the URL
            string fileName = Path.GetFileNameWithoutExtension(new Uri(imageUrl).AbsolutePath);
            var fileExtension = Path.GetExtension(new Uri(imageUrl).AbsolutePath);
            var normalizedFileName = fileName.UrlKey() + fileExtension;

            // Combine the folder path and filename
            string filePath = Path.Combine(folderPath, normalizedFileName);

            // Initialize HttpClient
            using (HttpClient client = new())
            {
                try
                {
                    // Get the image data
                    byte[] imageData = await client.GetByteArrayAsync(imageUrl);

                    // Write the image data to a file
                    await File.WriteAllBytesAsync(filePath, imageData);
                    System.Console.WriteLine($"Image downloaded successfully to {filePath}");
                }
                catch (Exception ex)
                {
                    System.Console.WriteLine($"An error occurred: {ex.Message}");
                }
            }

            return filePath;
        }
    }
}
