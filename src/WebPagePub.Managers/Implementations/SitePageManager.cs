using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using WebPagePub.Core.Utilities;
using WebPagePub.Data.Constants;
using WebPagePub.Data.Enums;
using WebPagePub.Data.Models;
using WebPagePub.Data.Models.Db;
using WebPagePub.Data.Repositories.Interfaces;
using WebPagePub.FileStorage.Repositories.Interfaces;
using WebPagePub.Managers.Interfaces;
using WebPagePub.Managers.Models.SitePages;
using WebPagePub.Services.Interfaces;

namespace WebPagePub.Managers.Implementations
{
    public class SitePageManager : ISitePageManager
    {
        private readonly ISitePagePhotoRepository sitePagePhotoRepository;
        private readonly ISitePageTagRepository sitePageTagRepository;
        private readonly ITagRepository tagRepository;
        private readonly ISitePageSectionRepository siteSectionRepository;
        private readonly ISitePageRepository sitePageRepository;
        private readonly ISiteFilesRepository siteFilesRepository;
        private readonly IImageUploaderService imageUploaderService;
        private readonly IAuthorRepository authorRepository;
        private readonly IContentSnippetRepository contentSnippetRepository;
        private readonly string blobPrefix;

        public SitePageManager(
            ISitePagePhotoRepository sitePagePhotoRepository,
            ISitePageTagRepository sitePageTagRepository,
            ISitePageSectionRepository siteSectionRepository,
            ITagRepository tagRepository,
            ISitePageRepository sitePageRepository,
            ISiteFilesRepository siteFilesRepository,
            IImageUploaderService imageUploaderService,
            IAuthorRepository authorRepository,
            IContentSnippetRepository contentSnippetRepository)
        {
            this.sitePagePhotoRepository = sitePagePhotoRepository;
            this.sitePageTagRepository = sitePageTagRepository;
            this.siteSectionRepository = siteSectionRepository;
            this.tagRepository = tagRepository;
            this.sitePageRepository = sitePageRepository;
            this.siteFilesRepository = siteFilesRepository;
            this.imageUploaderService = imageUploaderService;
            this.authorRepository = authorRepository;
            this.contentSnippetRepository = contentSnippetRepository;

            var blobPrefixConfig = this.contentSnippetRepository.Get(SiteConfigSetting.BlobPrefix);

            if (blobPrefixConfig != null)
            {
                this.blobPrefix = blobPrefixConfig.Content;
            }
        }

        public bool DoesPageExist(int siteSectionId, string pageKey)
        {
            var page = this.sitePageRepository.Get(siteSectionId, pageKey);

            return page != null && page.SitePageId > 0;
        }

        public SitePageSection GetSiteSection(string key)
        {
            return this.siteSectionRepository.Get(key);
        }

        public SitePageSection CreateSiteSection(
            string sitePageName,
            string sitePageSectionKey,
            string createdByUserId)
        {
            var siteSection = this.siteSectionRepository.Create(new SitePageSection()
            {
                Title = sitePageName.Trim(),
                Key = sitePageSectionKey.Trim().UrlKey(),
                BreadcrumbName = sitePageName.Trim()
            });

            this.sitePageRepository.Create(new SitePage()
            {
                Title = siteSection.Title,
                Key = StringConstants.DefaultKey,
                PageHeader = siteSection.Title,
                BreadcrumbName = siteSection.Title,
                PublishDateTimeUtc = DateTime.UtcNow,
                SitePageSectionId = siteSection.SitePageSectionId,
                CreatedByUserId = createdByUserId,
                PageType = PageType.Informational,
                AllowsComments = false,
                IsSectionHomePage = true
            });

            return siteSection;
        }

        public SitePageSection GetSiteSection(int sitePageSectionId)
        {
            return this.siteSectionRepository.Get(sitePageSectionId);
        }

        public IList<SitePage> GetSitePages(int pageNumber, int siteSectionId, int quantityPerPage, out int total)
        {
            return this.sitePageRepository.GetPage(pageNumber, siteSectionId, quantityPerPage, out total);
        }

        public void DeleteSiteSection(int siteSectionId)
        {
            this.siteSectionRepository.Delete(siteSectionId);
        }

        public bool UpdateSiteSection(SitePageSection siteSection)
        {
            return this.siteSectionRepository.Update(siteSection);
        }

        public async Task<SitePagePhoto> SetPhotoAsDefaultAsync(int sitePagePhotoId)
        {
            try
            {
                var entry = this.sitePagePhotoRepository.Get(sitePagePhotoId);

                Bitmap img;

                using (var client = new HttpClient())
                {
                    var rsp = await client.GetAsync(entry.PhotoFullScreenUrl);
                    img = new Bitmap(await rsp.Content.ReadAsStreamAsync());
                }

                entry.PhotoFullScreenUrlHeight = img.Height;
                entry.PhotoFullScreenUrlWidth = img.Width;
                this.sitePagePhotoRepository.Update(entry);

                this.sitePagePhotoRepository.SetDefaultPhoto(sitePagePhotoId);

                var sitePage = this.sitePageRepository.Get(entry.SitePageId);
                var sitePageSection = this.siteSectionRepository.Get(sitePage.SitePageSectionId);
                img.Dispose();

                return entry;
            }
            catch
            {
                throw new Exception("could not set default");
            }
        }

        public IEnumerable<SitePageSection> GetAllSiteSection()
        {
            return this.siteSectionRepository.GetAll();
        }

        public SitePage CreatePage(SitePage sitePage)
        {
            return this.sitePageRepository.Create(sitePage);
        }

        public async Task<int> DeletePhotoAsync(int sitePagePhotoId)
        {
            var sitePageId = this.sitePagePhotoRepository.Get(sitePagePhotoId).SitePageId;

            await this.DeleteBlogPhoto(sitePagePhotoId);

            this.ReOrderPhotos(sitePagePhotoId, sitePageId);

            return sitePageId;
        }

        public SitePagePhoto RankPhotoUp(int sitePagePhotoId)
        {
            var entry = this.sitePagePhotoRepository.Get(sitePagePhotoId);

            if (entry.Rank == 1)
            {
                return entry;
            }

            var allBlogPhotos = this.sitePagePhotoRepository.GetBlogPhotos(entry.SitePageId);
            var rankedHigher = allBlogPhotos.First(x => x.Rank == entry.Rank - 1);
            var higherRankValue = rankedHigher.Rank;

            rankedHigher.Rank = higherRankValue + 1;
            this.sitePagePhotoRepository.Update(rankedHigher);

            entry.Rank = higherRankValue;
            this.sitePagePhotoRepository.Update(entry);

            return entry;
        }

        public SitePagePhoto RankPhotoDown(int sitePagePhotoId)
        {
            var entry = this.sitePagePhotoRepository.Get(sitePagePhotoId);
            var allBlogPhotos = this.sitePagePhotoRepository.GetBlogPhotos(entry.SitePageId);

            if (entry.Rank == allBlogPhotos.Count)
            {
                return entry;
            }

            var rankedLower = allBlogPhotos.First(x => x.Rank == entry.Rank + 1);
            var lowerRankValue = rankedLower.Rank;
            rankedLower.Rank = lowerRankValue - 1;
            this.sitePagePhotoRepository.Update(rankedLower);

            entry.Rank = lowerRankValue;
            this.sitePagePhotoRepository.Update(entry);

            return entry;
        }

        public async Task UploadPhotos(
            int sitePageId,
            IList<Tuple<string, MemoryStream>> fileNameAndImageMemoryStream)
        {
            var allBlogPhotos = this.sitePagePhotoRepository.GetBlogPhotos(sitePageId);
            var highestRank = allBlogPhotos.Count;
            int currentRank = highestRank;
            var isFirstPhotoToSitePage = allBlogPhotos.Count == 0;

            try
            {
                var folderPath = GetBlogPhotoFolder(sitePageId);

                foreach (var image in fileNameAndImageMemoryStream)
                {
                    currentRank++;

                    if (currentRank > 1)
                    {
                        isFirstPhotoToSitePage = false;
                    }

                    await this.UploadSizesOfPhotos(
                        sitePageId,
                        allBlogPhotos,
                        currentRank,
                        folderPath,
                        image.Item2,
                        image.Item1,
                        isFirstPhotoToSitePage);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Upload failed", ex.InnerException);
            }
        }

        public async Task DeletePage(int sitePageId)
        {
            var sitePage = this.sitePageRepository.Get(sitePageId);

            foreach (var photo in sitePage.Photos)
            {
                await this.DeleteBlogPhoto(photo.SitePageId);
            }

            var task = Task.Run(() => this.sitePageRepository.Delete(sitePageId));
            var myOutput = await task;
        }

        public async Task<int> Rotate90DegreesAsync(int sitePagePhotoId)
        {
            var entry = this.sitePagePhotoRepository.Get(sitePagePhotoId);

            // todo: store original and rotate it, resize it, to low quality loss
            var photoOriginalUrl = await this.RotateImage90Degrees(entry.SitePageId, entry.PhotoOriginalUrl);
            var photoPreviewUrl = await this.RotateImage90Degrees(entry.SitePageId, entry.PhotoPreviewUrl);
            var photoThumbUrl = await this.RotateImage90Degrees(entry.SitePageId, entry.PhotoThumbUrl);
            var photoFullScreenUrl = await this.RotateImage90Degrees(entry.SitePageId, entry.PhotoFullScreenUrl);

            if (entry.PhotoOriginalUrl != photoOriginalUrl.ToString() ||
               entry.PhotoPreviewUrl != photoPreviewUrl.ToString() ||
               entry.PhotoThumbUrl != photoThumbUrl.ToString() ||
               entry.PhotoFullScreenUrl != photoFullScreenUrl.ToString())
            {
                entry.PhotoOriginalUrl = photoOriginalUrl.ToString();
                entry.PhotoPreviewUrl = photoPreviewUrl.ToString();
                entry.PhotoThumbUrl = photoThumbUrl.ToString();
                entry.PhotoFullScreenUrl = photoFullScreenUrl.ToString();

                this.sitePagePhotoRepository.Update(entry);
            }

            return entry.SitePageId;
        }

        public SitePage GetSitePage(int sitePageId)
        {
            return this.sitePageRepository.Get(sitePageId);
        }

        public bool UpdateSitePage(SitePage dbModel)
        {
            return this.sitePageRepository.Update(dbModel);
        }

        public IList<SitePagePhoto> GetBlogPhotos(int sitePageId)
        {
            return this.sitePagePhotoRepository.GetBlogPhotos(sitePageId);
        }

        public SitePage CreatePage(int siteSectionId, string pageTitle, string createdByUserId)
        {
            var key = pageTitle.UrlKey();
            var pageTypeSetting = this.contentSnippetRepository.Get(SiteConfigSetting.DefaultPageType);
            var pageType = PageType.Content;

            if (pageTypeSetting != null &&
                !string.IsNullOrEmpty(pageTypeSetting.Content))
            {
                Enum.TryParse(pageTypeSetting.Content, true, out pageType);
            }

            return this.sitePageRepository.Create(
                new SitePage()
                {
                    Title = pageTitle,
                    Key = key,
                    PageHeader = pageTitle,
                    BreadcrumbName = pageTitle,
                    PublishDateTimeUtc = DateTime.UtcNow,
                    SitePageSectionId = siteSectionId,
                    CreatedByUserId = createdByUserId,
                    AllowsComments = true,
                    PageType = pageType
                });
        }

        public async Task UpdatePhotoProperties(
            int sitePageId,
            IList<SitePagePhotoModel> newSitePagePhotos)
        {
            var allImages = this.GetBlogPhotos(sitePageId);

            foreach (var newSitePagePhoto in newSitePagePhotos)
            {
                var currentPhoto = allImages.First(x => x.SitePagePhotoId == newSitePagePhoto.SitePagePhotoId);
                await this.UpdateImageProperties(currentPhoto, newSitePagePhoto);
            }
        }

        public IList<SitePage> GetLivePage(int pageNumber, int quantityPerpage, out int total)
        {
            return this.sitePageRepository.GetLivePage(pageNumber, quantityPerpage, out total);
        }

        public void UpdateBlogTags(SitePageEditModel model, SitePage dbModel)
        {
            var previousTags = dbModel.SitePageTags.Select(x => x.Tag.Name).ToArray();

            if (model.Tags == null)
            {
                var previousTagsToRemove = new List<string>();
                foreach (var tag in dbModel.SitePageTags)
                {
                    previousTagsToRemove.Add(tag.Tag.Name);
                }

                this.RemoveDeletedTags(model, previousTagsToRemove.ToArray());
                return;
            }

            var currentTags = model.Tags.Split(',').Select(x => x.Trim()).ToArray();
            var tagsToAdd = currentTags.ToArray().Except(previousTags).ToArray();

            this.AddNewTags(model, dbModel, tagsToAdd);

            var tagsToRemove = previousTags.Except(currentTags).ToArray();

            this.RemoveDeletedTags(model, tagsToRemove);
        }

        public IList<Author> GetAllAuthors()
        {
            return this.authorRepository.GetAll();
        }

        public int? PreviouslyCreatedPage(DateTime createDate, int sitePageId, int sitePageSectionId)
        {
            var entry = this.sitePageRepository.GetPreviouslyCreatedEntry(createDate, sitePageId, sitePageSectionId);
            if (entry == null || entry.SitePageId == 0)
            {
                return null;
            }

            return entry.SitePageId;
        }

        public int? NextCreatedPage(DateTime createDate, int sitePageId, int sitePageSectionId)
        {
            var entry = this.sitePageRepository.GetNextCreatedEntry(createDate, sitePageId, sitePageSectionId);
            if (entry == null || entry.SitePageId == 0)
            {
                return null;
            }

            return entry.SitePageId;
        }

        public IList<SitePage> SearchForTerm(string term, int pageNumber, int quantityPerPage, out int total)
        {
            if (term == null)
            {
                total = 0;
                return new List<SitePage>();
            }

            term = term.Trim();

            return this.sitePageRepository.SearchForTerm(term, pageNumber, quantityPerPage, out total);
        }

        public bool DoesPageExistSimilar(int siteSectionId, string pageKey)
        {
            if (this.DoesPageExist(siteSectionId, pageKey))
            {
                return true;
            }

            if (this.DoesPageExist(siteSectionId, string.Format("{0}s", pageKey)))
            {
                return true;
            }

            if (pageKey.EndsWith("s") &&
                this.DoesPageExist(siteSectionId, pageKey.Remove(pageKey.Length - 1, 1)))
            {
                return true;
            }

            if (this.DoesPageExist(siteSectionId, string.Format("a-{0}", pageKey)))
            {
                return true;
            }

            if (this.DoesPageExist(siteSectionId, string.Format("the-{0}", pageKey)))
            {
                return true;
            }

            return false;
        }

        public SitePage GetPageForUrl(Uri sourcePage)
        {
            if (ShouldSkipUrl(sourcePage))
            {
                return default;
            }

            SitePageSection siteSection;
            SitePage sitePage;
            string[] segments = sourcePage.Segments;

            if (segments.Length == 1)
            {
                sitePage = this.sitePageRepository.GetSectionHomePage(this.siteSectionRepository.GetHomeSection().SitePageSectionId);
            }
            else if (segments.Length == 2)
            {
                siteSection = this.siteSectionRepository.Get(segments[1].TrimEnd('/'));
                sitePage = this.sitePageRepository.GetSectionHomePage(siteSection.SitePageSectionId);
            }
            else if (segments.Length == 3)
            {
                // Removing any trailing slashes from the segments
                var siteSectionKey = segments[1].TrimEnd('/');
                var pathKey = segments[2].TrimEnd('/');
                siteSection = this.siteSectionRepository.Get(siteSectionKey);
                sitePage = this.sitePageRepository.Get(siteSection.SitePageSectionId, pathKey);
            }
            else
            {
                throw new InvalidOperationException("Url has incorrect many segments");
            }

            return sitePage;
        }

        private static bool ShouldSkipUrl(Uri url)
        {
            return url.ToString().Contains("/tag/") || url.ToString().Contains("/page/");
        }

        private static string GetBlogPhotoFolder(int sitePageId)
        {
            return $"/{StringConstants.SitePhotoBlobPhotoName}/{sitePageId}/";
        }

        private async Task<Uri> RotateImage90Degrees(int sitePageId, string photoUrl)
        {
            var folderPath = GetBlogPhotoFolder(sitePageId);
            var stream = await this.imageUploaderService.ToStreamAsync(new Uri(photoUrl));
            var rotatedBitmap = ImageUtilities.Rotate90Degrees(Image.FromStream(stream));
            var streamRotated = this.imageUploaderService.ToAStream(
                rotatedBitmap,
                this.imageUploaderService.SetImageFormat(photoUrl));

            var url = await this.siteFilesRepository.UploadAsync(
                                        streamRotated,
                                        photoUrl.GetFileNameFromUrl(),
                                        folderPath);

            rotatedBitmap.Dispose();
            streamRotated.Dispose();
            rotatedBitmap.Dispose();

            return url;
        }

        private async Task UpdateImageProperties(SitePagePhoto sitePagePhoto, SitePagePhotoModel photo)
        {
            var hasChanged = false;
            var photoFileName = photo.FileName;
            var fileExtension = photoFileName.GetFileExtension();
            var newFileName = string.Format("{0}.{1}", Path.GetFileNameWithoutExtension(photoFileName).UrlKey(), fileExtension);
            var currentFileName = Path.GetFileName(photo.PhotoOriginalUrl);

            if (Path.HasExtension(newFileName) &&
                Path.HasExtension(currentFileName) &&
                newFileName != currentFileName)
            {
                hasChanged = true;

                await this.RenameAllPhotoVarients(sitePagePhoto, newFileName, currentFileName, this.blobPrefix);
            }

            if (sitePagePhoto.Title != photo.Title)
            {
                hasChanged = true;
                sitePagePhoto.Title = photo.Title;
            }

            if (sitePagePhoto.Description != photo.Description)
            {
                hasChanged = true;
                sitePagePhoto.Description = photo.Description;
            }

            if (hasChanged)
            {
                this.sitePagePhotoRepository.Update(sitePagePhoto);
            }
        }

        private void RemoveDeletedTags(SitePageEditModel model, IEnumerable<string> tagsToRemove)
        {
            if (tagsToRemove == null)
            {
                return;
            }

            foreach (var tag in tagsToRemove)
            {
                if (tag == null)
                {
                    continue;
                }

                var tagKey = tag.ToString().UrlKey();

                var tagDb = this.tagRepository.Get(tagKey);

                this.sitePageTagRepository.Delete(tagDb.TagId, model.SitePageId);
            }
        }

        private void ReOrderPhotos(int sitePagePhotoId, int sitePageId)
        {
            var allBlogPhotos = this.sitePagePhotoRepository.GetBlogPhotos(sitePageId)
                                                        .Where(x => x.SitePageId != sitePagePhotoId)
                                                        .OrderBy(x => x.Rank);
            int newRank = 1;

            foreach (var photo in allBlogPhotos)
            {
                photo.Rank = newRank;
                this.sitePagePhotoRepository.Update(photo);

                newRank++;
            }
        }

        private async Task UploadSizesOfPhotos(
            int sitePageId,
            IList<SitePagePhoto> allBlogPhotos,
            int currentRank,
            string folderPath,
            MemoryStream memoryStream,
            string fileName,
            bool isFirstPhotoToSitePage)
        {
            var fileExtension = fileName.GetFileExtension();
            var newFileName = string.Format("{0}.{1}", Path.GetFileNameWithoutExtension(fileName).UrlKey(), fileExtension);
            var originalPhotoUrl = await this.siteFilesRepository.UploadAsync(memoryStream, newFileName, folderPath);
            var thumbnailPhotoUrl = await this.imageUploaderService.UploadResizedVersionOfPhoto(folderPath, memoryStream, originalPhotoUrl, 300, 200, StringConstants.SuffixThumb);
            var fullScreenPhotoUrl = await this.imageUploaderService.UploadResizedVersionOfPhoto(folderPath, memoryStream, originalPhotoUrl, 1600, 1200, StringConstants.SuffixFullscreen);
            var previewPhotoUrl = await this.imageUploaderService.UploadResizedVersionOfPhoto(folderPath, memoryStream, originalPhotoUrl, 800, 600, StringConstants.SuffixPrevew);
            var existingPhoto = allBlogPhotos.FirstOrDefault(x => x.PhotoOriginalUrl == originalPhotoUrl.ToString());
            memoryStream.Dispose();

            if (existingPhoto == null)
            {
                this.sitePagePhotoRepository.Create(new SitePagePhoto()
                {
                    SitePageId = sitePageId,
                    PhotoOriginalUrl = originalPhotoUrl.ToString(),
                    PhotoThumbUrl = thumbnailPhotoUrl.ToString(),
                    PhotoFullScreenUrl = fullScreenPhotoUrl.ToString(),
                    PhotoPreviewUrl = previewPhotoUrl.ToString(),
                    Rank = currentRank,
                    IsDefault = isFirstPhotoToSitePage
                });
            }
            else
            {
                existingPhoto.PhotoOriginalUrl = originalPhotoUrl.ToString();
                existingPhoto.PhotoThumbUrl = thumbnailPhotoUrl.ToString();
                existingPhoto.PhotoFullScreenUrl = fullScreenPhotoUrl.ToString();
                existingPhoto.PhotoPreviewUrl = previewPhotoUrl.ToString();
                this.sitePagePhotoRepository.Update(existingPhoto);
            }
        }

        private void AddNewTags(SitePageEditModel model, SitePage dbModel, string[] currentTags)
        {
            foreach (var tagName in currentTags)
            {
                var tagKey = tagName.UrlKey();

                if (string.IsNullOrWhiteSpace(tagKey) ||
                    tagKey.Length >= 75)
                {
                    continue;
                }

                if (dbModel.SitePageTags.FirstOrDefault(x => x.Tag.Key == tagKey) == null)
                {
                    var tagDb = this.tagRepository.Get(tagKey);

                    if (tagDb == null || tagDb.TagId == 0)
                    {
                        this.tagRepository.Create(new Tag
                        {
                            Name = tagName.Trim(),
                            Key = tagKey
                        });

                        tagDb = this.tagRepository.Get(tagKey);
                    }

                    this.sitePageTagRepository.Create(new SitePageTag()
                    {
                        SitePageId = model.SitePageId,
                        TagId = tagDb.TagId,
                    });
                }
                else
                {
                    var tagDb = this.tagRepository.Get(tagKey);

                    if (tagDb.Name != tagName)
                    {
                        tagDb.Name = tagName;

                        this.tagRepository.Update(tagDb);
                    }
                }
            }
        }

        private async Task RenameAllPhotoVarients(
            SitePagePhoto photo,
            string newPhotoFileName,
            string currentFileName,
            string blobPrefix)
        {
            var blobPrefixFormatted = blobPrefix;

            if (blobPrefix.EndsWith("/"))
            {
                blobPrefixFormatted = blobPrefix.Remove(blobPrefix.Length - 1, 1);
            }

            var currentPathPhotoUrl = photo.PhotoOriginalUrl.Replace(
                string.Format(
                    "{0}/{1}/",
                    blobPrefixFormatted,
                    StringConstants.ContainerName), string.Empty);
            var newFilePathPhotoUrl = currentPathPhotoUrl.Replace(currentFileName, newPhotoFileName);
            await this.siteFilesRepository.ChangeFileName(currentPathPhotoUrl, newFilePathPhotoUrl);
            photo.PhotoOriginalUrl = string.Format("{0}/{1}/{2}", blobPrefixFormatted, StringConstants.ContainerName, newFilePathPhotoUrl);

            var newPhotoExtension = Path.GetExtension(newPhotoFileName);

            var currentPathPhotoFullScreenUrl = photo.PhotoFullScreenUrl.Replace(
                string.Format(
                    "{0}/{1}/",
                    blobPrefixFormatted,
                    StringConstants.ContainerName), string.Empty);
            var newFilePathPhotoFullScreenUrl = currentPathPhotoFullScreenUrl.Replace(
                Path.GetFileName(currentPathPhotoFullScreenUrl),
                string.Format("{0}{1}{2}", Path.GetFileNameWithoutExtension(newPhotoFileName), StringConstants.SuffixFullscreen, newPhotoExtension));
            await this.siteFilesRepository.ChangeFileName(currentPathPhotoFullScreenUrl, newFilePathPhotoFullScreenUrl);
            photo.PhotoFullScreenUrl = string.Format("{0}/{1}/{2}", blobPrefixFormatted, StringConstants.ContainerName, newFilePathPhotoFullScreenUrl);

            var currentPathPhotoPreviewUrl = photo.PhotoPreviewUrl.Replace(
                string.Format(
                    "{0}/{1}/",
                    blobPrefixFormatted,
                    StringConstants.ContainerName), string.Empty);
            var newFilePathPhotoPreviewUrl = currentPathPhotoPreviewUrl.Replace(
                Path.GetFileName(currentPathPhotoPreviewUrl),
                string.Format("{0}{1}{2}", Path.GetFileNameWithoutExtension(newPhotoFileName), StringConstants.SuffixPrevew, newPhotoExtension));
            await this.siteFilesRepository.ChangeFileName(currentPathPhotoPreviewUrl, newFilePathPhotoPreviewUrl);
            photo.PhotoPreviewUrl = string.Format("{0}/{1}/{2}", blobPrefixFormatted, StringConstants.ContainerName, newFilePathPhotoPreviewUrl);

            var currentPathPhotoThumbUrl = photo.PhotoThumbUrl.Replace(
                string.Format(
                    "{0}/{1}/",
                    blobPrefixFormatted,
                    StringConstants.ContainerName), string.Empty);
            var newFilePathPhotoThumbUrl = currentPathPhotoThumbUrl.Replace(
                Path.GetFileName(currentPathPhotoThumbUrl),
                string.Format("{0}{1}{2}", Path.GetFileNameWithoutExtension(newPhotoFileName), StringConstants.SuffixThumb, newPhotoExtension));
            await this.siteFilesRepository.ChangeFileName(currentPathPhotoThumbUrl, newFilePathPhotoThumbUrl);
            photo.PhotoThumbUrl = string.Format("{0}/{1}/{2}", blobPrefixFormatted, StringConstants.ContainerName, newFilePathPhotoThumbUrl);
        }

        private async Task<SitePagePhoto> DeleteBlogPhoto(int sitePagePhotoId)
        {
            var entry = this.sitePagePhotoRepository.Get(sitePagePhotoId);

            await this.siteFilesRepository.DeleteFileAsync(entry.PhotoOriginalUrl);
            await this.siteFilesRepository.DeleteFileAsync(entry.PhotoThumbUrl);
            await this.siteFilesRepository.DeleteFileAsync(entry.PhotoFullScreenUrl);
            await this.siteFilesRepository.DeleteFileAsync(entry.PhotoPreviewUrl);

            this.sitePagePhotoRepository.Delete(sitePagePhotoId);

            return entry;
        }
    }
}