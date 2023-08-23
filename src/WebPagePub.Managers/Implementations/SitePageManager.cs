using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
using System.Collections;
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
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

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

        public SitePage ConvertToDbModel(
            SitePageEditModel model,
            string userId)
        {
            var dbModel = this.sitePageRepository.Get(model.SitePageId);

            if (string.IsNullOrWhiteSpace(dbModel.CreatedByUserId))
            {
                dbModel.CreatedByUserId = userId;
            }

            dbModel.SitePageSectionId = model.SitePageSectionId;
            dbModel.UpdatedByUserId = userId;
            dbModel.Key = model.Key.UrlKey();
            dbModel.BreadcrumbName = model.BreadcrumbName.Trim();
            dbModel.PublishDateTimeUtc = model.PublishDateTimeUtc;
            dbModel.Content = model.Content;
            dbModel.Title = model.Title.Trim();
            dbModel.PageHeader = model.PageHeader;
            dbModel.IsLive = model.IsLive;
            dbModel.ExcludePageFromSiteMapXml = model.ExcludePageFromSiteMapXml;
            dbModel.MetaDescription = (model.MetaDescription != null) ? model.MetaDescription.Trim() : string.Empty;
            dbModel.PageType = model.PageType;
            dbModel.ReviewBestValue = model.ReviewBestValue;
            dbModel.ReviewItemName = model.ReviewItemName;
            dbModel.ReviewRatingValue = model.ReviewRatingValue;
            dbModel.ReviewWorstValue = model.ReviewWorstValue;
            dbModel.MetaKeywords = (model.MetaKeywords != null) ? model.MetaKeywords.Trim() : string.Empty;
            dbModel.AllowsComments = model.AllowsComments;
            dbModel.IsSectionHomePage = model.IsSectionHomePage;
            dbModel.AuthorId = model.AuthorId;

            return dbModel;
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

        public List<SitePage> GetSitePages(int pageNumber, int siteSectionId, int quantityPerPage, out int total)
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

            ReOrderPhotos(sitePagePhotoId, sitePageId);

            return sitePageId;
        }

        public SitePagePhoto RankPhotoUp(int sitePagePhotoId)
        {
            var entry = this.sitePagePhotoRepository.Get(sitePagePhotoId);

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
            List<Tuple<string, MemoryStream>> fileNameAndImageMemoryStream)
        {
            var allBlogPhotos = this.sitePagePhotoRepository.GetBlogPhotos(sitePageId);
            var highestRank = allBlogPhotos.Count();
            int currentRank = highestRank;
            var isFirstPhotoToSitePage = allBlogPhotos.Count() == 0;

            try
            {
                var folderPath = this.GetBlogPhotoFolder(sitePageId);

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

                this.sitePagePhotoRepository.Update(entry);
            }

            return entry.SitePageId;
        }


        private async Task<Uri> RotateImage90Degrees(int sitePageId, string photoUrl)
        {
            var folderPath = this.GetBlogPhotoFolder(sitePageId);
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

        public SitePage GetSitePage(int sitePageId)
        {
            return this.sitePageRepository.Get(sitePageId);
        }

        public bool UpdateSitePage(SitePage dbModel)
        {
            return this.sitePageRepository.Update(dbModel);
        }

        public List<SitePagePhoto> GetBlogPhotos(int sitePageId)
        {
            return this.sitePagePhotoRepository.GetBlogPhotos(sitePageId);
        }

        public SitePage CreatePage(int siteSectionId, string pageTitle, string createdByUserId)
        {
            var key = pageTitle.UrlKey();

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
                    PageType = PageType.Content
                });
        }

        public async Task UpdatePhotoProperties(
            int sitePageId,
            List<SitePagePhotoModel> newSitePagePhotos)
        {
            var allImages = this.GetBlogPhotos(sitePageId);

            foreach (var newSitePagePhoto in newSitePagePhotos)
            {
                var currentPhoto = allImages.First(x => x.SitePagePhotoId == newSitePagePhoto.SitePagePhotoId);
                await UpdateImageProperties(currentPhoto, newSitePagePhoto);
            }
        }

        public List<SitePage> GetLivePage(int pageNumber, int quantityPerpage, out int total)
        {
            return this.sitePageRepository.GetLivePage(pageNumber, quantityPerpage, out total);
        }

        public void UpdateBlogTags(SitePageEditModel model, SitePage dbModel)
        {
            var previousTags = dbModel.SitePageTags.Select(x => x.Tag.Name).ToArray();

            if (model.Tags == null)
            {
                var previousTagsToRemove = new ArrayList();
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

        public List<Author> GetAllAuthors()
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

        private async Task UpdateImageProperties(SitePagePhoto sitePagePhoto, SitePagePhotoModel photo)
        {
            var hasChanged = false;
            var photoFileName = photo.FileName;
            var fileExtension = photoFileName.GetFileExtension();
            var newFileName = string.Format("{0}.{1}", Path.GetFileNameWithoutExtension(photoFileName).UrlKey(), fileExtension);
            var currentFileName = Path.GetFileName(photo.PhotoOriginalUrl);

            if (Path.HasExtension(newFileName) &&
                Path.HasExtension(currentFileName) &&
                (newFileName != currentFileName))
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
                this.sitePagePhotoRepository.Update(sitePagePhoto);
            }
        }

        private void RemoveDeletedTags(SitePageEditModel model, IEnumerable<object?> tagsToRemove)
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

        private string GetBlogPhotoFolder(int sitePageId)
        {
            return $"/{StringConstants.SitePhotoBlobPhotoName}/{sitePageId}/";
        }

        private async Task UploadSizesOfPhotos(
            int sitePageId,
            List<SitePagePhoto> allBlogPhotos,
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

                if (string.IsNullOrWhiteSpace(tagKey))
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
