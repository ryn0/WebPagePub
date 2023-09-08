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
                blobPrefix = blobPrefixConfig.Content;
            }
        }

        public bool DoesPageExist(int siteSectionId, string pageKey)
        {
            var page = sitePageRepository.Get(siteSectionId, pageKey);

            return page != null && page.SitePageId > 0;
        }

        public SitePageSection GetSiteSection(string key)
        {
            return siteSectionRepository.Get(key);
        }

        public SitePageSection CreateSiteSection(
            string sitePageName,
            string sitePageSectionKey,
            string createdByUserId)
        {
            var siteSection = siteSectionRepository.Create(new SitePageSection()
            {
                Title = sitePageName.Trim(),
                Key = sitePageSectionKey.Trim().UrlKey(),
                BreadcrumbName = sitePageName.Trim()
            });

            sitePageRepository.Create(new SitePage()
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
            return siteSectionRepository.Get(sitePageSectionId);
        }

        public IList<SitePage> GetSitePages(int pageNumber, int siteSectionId, int quantityPerPage, out int total)
        {
            return sitePageRepository.GetPage(pageNumber, siteSectionId, quantityPerPage, out total);
        }

        public void DeleteSiteSection(int siteSectionId)
        {
            siteSectionRepository.Delete(siteSectionId);
        }

        public bool UpdateSiteSection(SitePageSection siteSection)
        {
            return siteSectionRepository.Update(siteSection);
        }

        public async Task<SitePagePhoto> SetPhotoAsDefaultAsync(int sitePagePhotoId)
        {
            try
            {
                var entry = sitePagePhotoRepository.Get(sitePagePhotoId);

                Bitmap img;

                using (var client = new HttpClient())
                {
                    var rsp = await client.GetAsync(entry.PhotoFullScreenUrl);
                    img = new Bitmap(await rsp.Content.ReadAsStreamAsync());
                }

                entry.PhotoFullScreenUrlHeight = img.Height;
                entry.PhotoFullScreenUrlWidth = img.Width;
                sitePagePhotoRepository.Update(entry);

                sitePagePhotoRepository.SetDefaultPhoto(sitePagePhotoId);

                var sitePage = sitePageRepository.Get(entry.SitePageId);
                var sitePageSection = siteSectionRepository.Get(sitePage.SitePageSectionId);
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
            return siteSectionRepository.GetAll();
        }

        public SitePage CreatePage(SitePage sitePage)
        {
            return sitePageRepository.Create(sitePage);
        }

        public async Task<int> DeletePhotoAsync(int sitePagePhotoId)
        {
            var sitePageId = sitePagePhotoRepository.Get(sitePagePhotoId).SitePageId;

            await DeleteBlogPhoto(sitePagePhotoId);

            ReOrderPhotos(sitePagePhotoId, sitePageId);

            return sitePageId;
        }

        public SitePagePhoto RankPhotoUp(int sitePagePhotoId)
        {
            var entry = sitePagePhotoRepository.Get(sitePagePhotoId);

            if (entry.Rank == 1)
            {
                return entry;
            }

            var allBlogPhotos = sitePagePhotoRepository.GetBlogPhotos(entry.SitePageId);
            var rankedHigher = allBlogPhotos.First(x => x.Rank == entry.Rank - 1);
            var higherRankValue = rankedHigher.Rank;

            rankedHigher.Rank = higherRankValue + 1;
            sitePagePhotoRepository.Update(rankedHigher);

            entry.Rank = higherRankValue;
            sitePagePhotoRepository.Update(entry);

            return entry;
        }

        public SitePagePhoto RankPhotoDown(int sitePagePhotoId)
        {
            var entry = sitePagePhotoRepository.Get(sitePagePhotoId);
            var allBlogPhotos = sitePagePhotoRepository.GetBlogPhotos(entry.SitePageId);

            if (entry.Rank == allBlogPhotos.Count())
            {
                return entry;
            }

            var rankedLower = allBlogPhotos.First(x => x.Rank == entry.Rank + 1);
            var lowerRankValue = rankedLower.Rank;
            rankedLower.Rank = lowerRankValue - 1;
            sitePagePhotoRepository.Update(rankedLower);

            entry.Rank = lowerRankValue;
            sitePagePhotoRepository.Update(entry);

            return entry;
        }

        public async Task UploadPhotos(
            int sitePageId,
            IList<Tuple<string, MemoryStream>> fileNameAndImageMemoryStream)
        {
            var allBlogPhotos = sitePagePhotoRepository.GetBlogPhotos(sitePageId);
            var highestRank = allBlogPhotos.Count();
            int currentRank = highestRank;
            var isFirstPhotoToSitePage = allBlogPhotos.Count() == 0;

            try
            {
                var folderPath = GetBlogPhotoFolder(sitePageId);

                foreach (var image in fileNameAndImageMemoryStream)
                {
                    currentRank++;

                    await UploadSizesOfPhotos(
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
            var sitePage = sitePageRepository.Get(sitePageId);

            foreach (var photo in sitePage.Photos)
            {
                await DeleteBlogPhoto(photo.SitePageId);
            }

            var task = Task.Run(() => sitePageRepository.Delete(sitePageId));
            var myOutput = await task;
        }

        public async Task<int> Rotate90DegreesAsync(int sitePagePhotoId)
        {
            var entry = sitePagePhotoRepository.Get(sitePagePhotoId);

            // todo: store original and rotate it, resize it, to low quality loss

            var photoOriginalUrl = await RotateImage90Degrees(entry.SitePageId, entry.PhotoOriginalUrl);
            var photoPreviewUrl = await RotateImage90Degrees(entry.SitePageId, entry.PhotoPreviewUrl);
            var photoThumbUrl = await RotateImage90Degrees(entry.SitePageId, entry.PhotoThumbUrl);
            var photoFullScreenUrl = await RotateImage90Degrees(entry.SitePageId, entry.PhotoFullScreenUrl);

            if (entry.PhotoOriginalUrl != photoOriginalUrl.ToString() ||
               entry.PhotoPreviewUrl != photoPreviewUrl.ToString() ||
               entry.PhotoThumbUrl != photoThumbUrl.ToString() ||
               entry.PhotoFullScreenUrl != photoFullScreenUrl.ToString())
            {
                entry.PhotoOriginalUrl = photoOriginalUrl.ToString();
                entry.PhotoPreviewUrl = photoPreviewUrl.ToString();
                entry.PhotoThumbUrl = photoThumbUrl.ToString();
                entry.PhotoFullScreenUrl = photoFullScreenUrl.ToString();

                sitePagePhotoRepository.Update(entry);
            }

            return entry.SitePageId;
        }


        private async Task<Uri> RotateImage90Degrees(int sitePageId, string photoUrl)
        {
            var folderPath = GetBlogPhotoFolder(sitePageId);
            var stream = await imageUploaderService.ToStreamAsync(new Uri(photoUrl));
            var rotatedBitmap = ImageUtilities.Rotate90Degrees(Image.FromStream(stream));
            var streamRotated = imageUploaderService.ToAStream(
                rotatedBitmap,
                imageUploaderService.SetImageFormat(photoUrl));

            var url = await siteFilesRepository.UploadAsync(
                                        streamRotated,
                                        photoUrl.GetFileNameFromUrl(),
                                        folderPath);

            rotatedBitmap.Dispose();
            streamRotated.Dispose();
            rotatedBitmap.Dispose();

            return url;
        }

        public SitePage GetSitePage(int sitePageId)
        {
            return sitePageRepository.Get(sitePageId);
        }

        public bool UpdateSitePage(SitePage dbModel)
        {
            return sitePageRepository.Update(dbModel);
        }

        public IList<SitePagePhoto> GetBlogPhotos(int sitePageId)
        {
            return sitePagePhotoRepository.GetBlogPhotos(sitePageId);
        }

        public SitePage CreatePage(int siteSectionId, string pageTitle, string createdByUserId)
        {
            var key = pageTitle.UrlKey();
            var pageTypeSetting = contentSnippetRepository.Get(SiteConfigSetting.DefaultPageType);
            var pageType = PageType.Content;

            if (pageTypeSetting != null &&
                !string.IsNullOrEmpty(pageTypeSetting.Content))
            {
                Enum.TryParse(pageTypeSetting.Content, true, out pageType);
            }

            return sitePageRepository.Create(
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
            var allImages = GetBlogPhotos(sitePageId);

            foreach (var newSitePagePhoto in newSitePagePhotos)
            {
                var currentPhoto = allImages.First(x => x.SitePagePhotoId == newSitePagePhoto.SitePagePhotoId);
                await UpdateImageProperties(currentPhoto, newSitePagePhoto);
            }
        }

        public IList<SitePage> GetLivePage(int pageNumber, int quantityPerpage, out int total)
        {
            return sitePageRepository.GetLivePage(pageNumber, quantityPerpage, out total);
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
                RemoveDeletedTags(model, previousTagsToRemove.ToArray());
                return;
            }

            var currentTags = model.Tags.Split(',').Select(x => x.Trim()).ToArray();
            var tagsToAdd = currentTags.ToArray().Except(previousTags).ToArray();

            AddNewTags(model, dbModel, tagsToAdd);

            var tagsToRemove = previousTags.Except(currentTags).ToArray();

            RemoveDeletedTags(model, tagsToRemove);
        }

        public IList<Author> GetAllAuthors()
        {
            return authorRepository.GetAll();
        }

        public int? PreviouslyCreatedPage(DateTime createDate, int sitePageId, int sitePageSectionId)
        {
            var entry = sitePageRepository.GetPreviouslyCreatedEntry(createDate, sitePageId, sitePageSectionId);
            if (entry == null || entry.SitePageId == 0)
            {
                return null;
            }
            return entry.SitePageId;
        }

        public int? NextCreatedPage(DateTime createDate, int sitePageId, int sitePageSectionId)
        {
            var entry = sitePageRepository.GetNextCreatedEntry(createDate, sitePageId, sitePageSectionId);
            if (entry == null || entry.SitePageId == 0)
            {
                return null;
            }
            return entry.SitePageId;
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

                await RenameAllPhotoVarients(sitePagePhoto, newFileName, currentFileName, blobPrefix);
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
                sitePagePhotoRepository.Update(sitePagePhoto);
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

                var tagDb = tagRepository.Get(tagKey);

                sitePageTagRepository.Delete(tagDb.TagId, model.SitePageId);
            }
        }

        private void ReOrderPhotos(int sitePagePhotoId, int sitePageId)
        {
            var allBlogPhotos = sitePagePhotoRepository.GetBlogPhotos(sitePageId)
                                                        .Where(x => x.SitePageId != sitePagePhotoId)
                                                        .OrderBy(x => x.Rank);
            int newRank = 1;

            foreach (var photo in allBlogPhotos)
            {
                photo.Rank = newRank;
                sitePagePhotoRepository.Update(photo);

                newRank++;
            }
        }

        private string GetBlogPhotoFolder(int sitePageId)
        {
            return $"/{StringConstants.SitePhotoBlobPhotoName}/{sitePageId}/";
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
            var originalPhotoUrl = await siteFilesRepository.UploadAsync(memoryStream, newFileName, folderPath);
            var thumbnailPhotoUrl = await imageUploaderService.UploadResizedVersionOfPhoto(folderPath, memoryStream, originalPhotoUrl, 300, 200, StringConstants.SuffixThumb);
            var fullScreenPhotoUrl = await imageUploaderService.UploadResizedVersionOfPhoto(folderPath, memoryStream, originalPhotoUrl, 1600, 1200, StringConstants.SuffixFullscreen);
            var previewPhotoUrl = await imageUploaderService.UploadResizedVersionOfPhoto(folderPath, memoryStream, originalPhotoUrl, 800, 600, StringConstants.SuffixPrevew);
            var existingPhoto = allBlogPhotos.FirstOrDefault(x => x.PhotoOriginalUrl == originalPhotoUrl.ToString());
            memoryStream.Dispose();

            if (existingPhoto == null)
            {
                sitePagePhotoRepository.Create(new SitePagePhoto()
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
                sitePagePhotoRepository.Update(existingPhoto);
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
                    var tagDb = tagRepository.Get(tagKey);

                    if (tagDb == null || tagDb.TagId == 0)
                    {
                        tagRepository.Create(new Tag
                        {
                            Name = tagName.Trim(),
                            Key = tagKey
                        });

                        tagDb = tagRepository.Get(tagKey);
                    }

                    sitePageTagRepository.Create(new SitePageTag()
                    {
                        SitePageId = model.SitePageId,
                        TagId = tagDb.TagId,
                    });
                }
                else
                {
                    var tagDb = tagRepository.Get(tagKey);

                    if (tagDb.Name != tagName)
                    {
                        tagDb.Name = tagName;

                        tagRepository.Update(tagDb);
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

            //
            var currentPathPhotoUrl = photo.PhotoOriginalUrl.Replace(string.Format("{0}/{1}/",
               blobPrefixFormatted, StringConstants.ContainerName), string.Empty);
            var newFilePathPhotoUrl = currentPathPhotoUrl.Replace(currentFileName, newPhotoFileName);
            await siteFilesRepository.ChangeFileName(currentPathPhotoUrl, newFilePathPhotoUrl);
            photo.PhotoOriginalUrl = string.Format("{0}/{1}/{2}", blobPrefixFormatted, StringConstants.ContainerName, newFilePathPhotoUrl);

            var newPhotoExtension = Path.GetExtension(newPhotoFileName);

            //
            var currentPathPhotoFullScreenUrl = photo.PhotoFullScreenUrl.Replace(string.Format("{0}/{1}/",
                blobPrefixFormatted, StringConstants.ContainerName), string.Empty);
            var newFilePathPhotoFullScreenUrl = currentPathPhotoFullScreenUrl.Replace(Path.GetFileName(currentPathPhotoFullScreenUrl),
                string.Format("{0}{1}{2}", Path.GetFileNameWithoutExtension(newPhotoFileName), StringConstants.SuffixFullscreen, newPhotoExtension));
            await siteFilesRepository.ChangeFileName(currentPathPhotoFullScreenUrl, newFilePathPhotoFullScreenUrl);
            photo.PhotoFullScreenUrl = string.Format("{0}/{1}/{2}", blobPrefixFormatted, StringConstants.ContainerName, newFilePathPhotoFullScreenUrl);

            //
            var currentPathPhotoPreviewUrl = photo.PhotoPreviewUrl.Replace(string.Format("{0}/{1}/",
                blobPrefixFormatted, StringConstants.ContainerName), string.Empty);
            var newFilePathPhotoPreviewUrl = currentPathPhotoPreviewUrl.Replace(Path.GetFileName(currentPathPhotoPreviewUrl),
                string.Format("{0}{1}{2}", Path.GetFileNameWithoutExtension(newPhotoFileName), StringConstants.SuffixPrevew, newPhotoExtension));
            await siteFilesRepository.ChangeFileName(currentPathPhotoPreviewUrl, newFilePathPhotoPreviewUrl);
            photo.PhotoPreviewUrl = string.Format("{0}/{1}/{2}", blobPrefixFormatted, StringConstants.ContainerName, newFilePathPhotoPreviewUrl);

            //
            var currentPathPhotoThumbUrl = photo.PhotoThumbUrl.Replace(string.Format("{0}/{1}/",
                blobPrefixFormatted, StringConstants.ContainerName), string.Empty);
            var newFilePathPhotoThumbUrl = currentPathPhotoThumbUrl.Replace(Path.GetFileName(currentPathPhotoThumbUrl),
                string.Format("{0}{1}{2}", Path.GetFileNameWithoutExtension(newPhotoFileName), StringConstants.SuffixThumb, newPhotoExtension));
            await siteFilesRepository.ChangeFileName(currentPathPhotoThumbUrl, newFilePathPhotoThumbUrl);
            photo.PhotoThumbUrl = string.Format("{0}/{1}/{2}", blobPrefixFormatted, StringConstants.ContainerName, newFilePathPhotoThumbUrl);
        }

        private async Task<SitePagePhoto> DeleteBlogPhoto(int sitePagePhotoId)
        {
            var entry = sitePagePhotoRepository.Get(sitePagePhotoId);

            await siteFilesRepository.DeleteFileAsync(entry.PhotoOriginalUrl);
            await siteFilesRepository.DeleteFileAsync(entry.PhotoThumbUrl);
            await siteFilesRepository.DeleteFileAsync(entry.PhotoFullScreenUrl);
            await siteFilesRepository.DeleteFileAsync(entry.PhotoPreviewUrl);

            sitePagePhotoRepository.Delete(sitePagePhotoId);

            return entry;
        }

        public IList<SitePage> SearchForTerm(string term, int pageNumber, int quantityPerPage, out int total)
        {
            if (term == null)
            {
                total = 0;
                return new List<SitePage>();
            }

            term = term.Trim();

            return sitePageRepository.SearchForTerm(term, pageNumber, quantityPerPage, out total);
        }

        public bool DoesPageExistSimilar(int siteSectionId, string pageKey)
        {
            if (DoesPageExist(siteSectionId, pageKey))
            {
                return true;
            }

            if (DoesPageExist(siteSectionId, string.Format("{0}s", pageKey)))
            {
                return true;
            }

            if (pageKey.EndsWith("s") &&
                DoesPageExist(siteSectionId, pageKey.Remove(pageKey.Length - 1, 1)))
            {
                return true;
            }

            if (DoesPageExist(siteSectionId, string.Format("a-{0}", pageKey)))
            {
                return true;
            }

            if (DoesPageExist(siteSectionId, string.Format("the-{0}", pageKey)))
            {
                return true;
            }

            return false;
        }
    }
}
