using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using WebPagePub.Core.Utilities;
using WebPagePub.Data.Constants;
using WebPagePub.Data.Enums;
using WebPagePub.Data.Models;
using WebPagePub.Data.Models.Db;
using WebPagePub.Data.Models.Transfer;
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

        // FIX 1: ICacheService replaces IContentSnippetRepository in this class.
        // The constructor previously called contentSnippetRepository.Get(BlobPrefix)
        // directly, which fired a live DB query on every injection (this class is
        // Transient). ICacheService.GetSnippet() hits an in-memory cache instead,
        // making repeated construction free after the first call.
        private readonly ICacheService cacheService;

        public SitePageManager(
            ISitePagePhotoRepository sitePagePhotoRepository,
            ISitePageTagRepository sitePageTagRepository,
            ISitePageSectionRepository siteSectionRepository,
            ITagRepository tagRepository,
            ISitePageRepository sitePageRepository,
            ISiteFilesRepository siteFilesRepository,
            IImageUploaderService imageUploaderService,
            IAuthorRepository authorRepository,
            ICacheService cacheService)
        {
            this.sitePagePhotoRepository = sitePagePhotoRepository;
            this.sitePageTagRepository = sitePageTagRepository;
            this.siteSectionRepository = siteSectionRepository;
            this.tagRepository = tagRepository;
            this.sitePageRepository = sitePageRepository;
            this.siteFilesRepository = siteFilesRepository;
            this.imageUploaderService = imageUploaderService;
            this.authorRepository = authorRepository;
            this.cacheService = cacheService;

            // No DB call here any more. BlobPrefix is resolved lazily via the
            // cache on each access, so repeated construction is free.
        }

        // Lazy property — reads from cache on first use, not on construction.
        private string BlobPrefix => this.cacheService.GetSnippet(SiteConfigSetting.BlobPrefix) ?? string.Empty;

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

            this.sitePageRepository.CreateAsync(new SitePage()
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

                // FIX 2: The original code created a raw `new HttpClient()` inside a
                // `using` block. Disposing HttpClient after every call causes socket
                // exhaustion under load (TIME_WAIT accumulation). IImageUploaderService
                // already wraps a properly-managed IHttpClientFactory client — reuse it.
                //
                // FIX 3: Bitmap now lives inside a `using` so it is always disposed,
                // even if an exception is thrown after creation. The original code only
                // called img.Dispose() at the bottom, leaking the GDI handle on any
                // exception thrown before that line.
                //
                // FIX 4 (dead code removed): The original fetched `sitePage` and
                // `sitePageSection` from the DB immediately after this block. Neither
                // variable was used anywhere — they were pure dead DB round-trips.
                using var stream = await this.imageUploaderService.ToStreamAsync(new Uri(entry.PhotoFullScreenUrl));
                using var img = new Bitmap(stream);

                entry.PhotoFullScreenUrlHeight = img.Height;
                entry.PhotoFullScreenUrlWidth = img.Width;
                this.sitePagePhotoRepository.Update(entry);
                this.sitePagePhotoRepository.SetDefaultPhoto(sitePagePhotoId);

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

        public async Task<SitePage> CreatePageAsync(SitePage sitePage)
        {
            return await this.sitePageRepository.CreateAsync(sitePage);
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

                    // FIX 5: The caller owns the stream and is responsible for disposing it.
                    // UploadSizesOfPhotos no longer calls memoryStream.Dispose() internally —
                    // doing so from the callee violated ownership and could cause a double-dispose
                    // if the caller also cleaned up (e.g. in a using block).
                    using (image.Item2)
                    {
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

        public Task<bool> UpdateSitePage(SitePage dbModel)
        {
            return this.sitePageRepository.UpdateAsync(dbModel);
        }

        public IList<SitePagePhoto> GetBlogPhotos(int sitePageId)
        {
            return this.sitePagePhotoRepository.GetBlogPhotos(sitePageId);
        }

        public async Task<SitePage> CreatePageAsync(int siteSectionId, string pageTitle, string createdByUserId)
        {
            var key = pageTitle.UrlKey();

            // FIX 6: Like the constructor, the original made a live DB call here via
            // contentSnippetRepository.Get(DefaultPageType) on every page creation.
            // ICacheService.GetSnippet() serves the same value from the in-memory
            // cache after the first lookup.
            var pageTypeRaw = this.cacheService.GetSnippet(SiteConfigSetting.DefaultPageType);
            var pageType = PageType.Content;

            if (!string.IsNullOrEmpty(pageTypeRaw))
            {
                Enum.TryParse(pageTypeRaw, ignoreCase: true, out pageType);
            }

            return await this.sitePageRepository.CreateAsync(
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

        public IList<SiteMapDisplaySection> GetAllLinksAndTitles()
        {
            return this.sitePageRepository.GetAllLinksAndTitles();
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
                var previousTagsToRemove = dbModel.SitePageTags.Select(x => x.Tag.Name).ToList();
                this.RemoveDeletedTags(model, previousTagsToRemove.ToArray());
                return;
            }

            var currentTags = model.Tags.Split(',').Select(x => x.Trim()).ToArray();
            var tagsToAdd = currentTags.Except(previousTags).ToArray();

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
                sitePage = this.sitePageRepository.GetSectionHomePage(
                    this.siteSectionRepository.GetHomeSection().SitePageSectionId);
            }
            else if (segments.Length == 2)
            {
                siteSection = this.siteSectionRepository.Get(segments[1].TrimEnd('/'));
                sitePage = this.sitePageRepository.GetSectionHomePage(siteSection.SitePageSectionId);
            }
            else if (segments.Length == 3)
            {
                var siteSectionKey = segments[1].TrimEnd('/');
                var pathKey = segments[2].TrimEnd('/');
                siteSection = this.siteSectionRepository.Get(siteSectionKey);
                sitePage = this.sitePageRepository.Get(siteSection.SitePageSectionId, pathKey);
            }
            else
            {
                throw new InvalidOperationException("Url has too many segments");
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

            // FIX 7: The original disposed rotatedBitmap twice (two rotatedBitmap.Dispose()
            // calls at the bottom). Double-disposing a GDI Bitmap can throw an
            // ArgumentException on some platforms. Replaced with a single `using` scope
            // for each disposable so the compiler guarantees exactly one dispose each.
            //
            // FIX 8: streamRotated was only disposed via a manual call. Now inside a
            // `using` so it is always released, including on exceptions.
            using var sourceStream = await this.imageUploaderService.ToStreamAsync(new Uri(photoUrl));
            using var sourceImage = Image.FromStream(sourceStream);
            using var rotatedBitmap = ImageUtilities.Rotate90Degrees(sourceImage);
            using var streamRotated = this.imageUploaderService.ToAStream(
                rotatedBitmap,
                this.imageUploaderService.SetImageFormat(photoUrl));

            var url = await this.siteFilesRepository.UploadAsync(
                streamRotated,
                photoUrl.GetFileNameFromUrl(),
                folderPath);

            return url;
        }

        private async Task UpdateImageProperties(SitePagePhoto sitePagePhoto, SitePagePhotoModel photo)
        {
            var hasChanged = false;
            var photoFileName = photo.FileName;
            var fileExtension = photoFileName.GetFileExtension();
            var newFileName = string.Format(
                "{0}.{1}",
                Path.GetFileNameWithoutExtension(photoFileName).UrlKey(),
                fileExtension);
            var currentFileName = Path.GetFileName(photo.PhotoOriginalUrl);

            if (Path.HasExtension(newFileName) &&
                Path.HasExtension(currentFileName) &&
                newFileName != currentFileName)
            {
                hasChanged = true;
                await this.RenameAllPhotoVarients(sitePagePhoto, newFileName, currentFileName, this.BlobPrefix);
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
            var newFileName = string.Format(
                "{0}.{1}",
                Path.GetFileNameWithoutExtension(fileName).UrlKey(),
                fileExtension);

            var expiresDate = DateTime.UtcNow.AddYears(1).ToString("R");

            var originalPhotoUrl = await this.siteFilesRepository.UploadAsync(memoryStream, newFileName, folderPath, expiresDate);
            var thumbnailPhotoUrl = await this.imageUploaderService.UploadResizedVersionOfPhoto(folderPath, memoryStream, originalPhotoUrl, 300, 200, StringConstants.SuffixThumb, expiresDate);
            var fullScreenPhotoUrl = await this.imageUploaderService.UploadResizedVersionOfPhoto(folderPath, memoryStream, originalPhotoUrl, 1600, 1200, StringConstants.SuffixFullscreen, expiresDate);
            var previewPhotoUrl = await this.imageUploaderService.UploadResizedVersionOfPhoto(folderPath, memoryStream, originalPhotoUrl, 800, 600, StringConstants.SuffixPrevew, expiresDate);

            // FIX 5 (continued): memoryStream.Dispose() has been removed from here.
            // The caller (UploadPhotos) now wraps each stream in a `using` block,
            // which is the correct ownership pattern — the creator disposes.

            var existingPhoto = allBlogPhotos.FirstOrDefault(x => x.PhotoOriginalUrl == originalPhotoUrl.ToString());

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

                if (string.IsNullOrWhiteSpace(tagKey) || tagKey.Length >= 75)
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
            var blobPrefixFormatted = blobPrefix.TrimEnd('/');

            var currentPathPhotoUrl = photo.PhotoOriginalUrl.Replace(
                string.Format("{0}/{1}/", blobPrefixFormatted, StringConstants.ContainerName),
                string.Empty);
            var newFilePathPhotoUrl = currentPathPhotoUrl.Replace(currentFileName, newPhotoFileName);
            await this.siteFilesRepository.ChangeFileName(currentPathPhotoUrl, newFilePathPhotoUrl);
            photo.PhotoOriginalUrl = string.Format("{0}/{1}/{2}", blobPrefixFormatted, StringConstants.ContainerName, newFilePathPhotoUrl);

            var newPhotoExtension = Path.GetExtension(newPhotoFileName);

            var currentPathPhotoFullScreenUrl = photo.PhotoFullScreenUrl.Replace(
                string.Format("{0}/{1}/", blobPrefixFormatted, StringConstants.ContainerName),
                string.Empty);
            var newFilePathPhotoFullScreenUrl = currentPathPhotoFullScreenUrl.Replace(
                Path.GetFileName(currentPathPhotoFullScreenUrl),
                string.Format("{0}{1}{2}", Path.GetFileNameWithoutExtension(newPhotoFileName), StringConstants.SuffixFullscreen, newPhotoExtension));
            await this.siteFilesRepository.ChangeFileName(currentPathPhotoFullScreenUrl, newFilePathPhotoFullScreenUrl);
            photo.PhotoFullScreenUrl = string.Format("{0}/{1}/{2}", blobPrefixFormatted, StringConstants.ContainerName, newFilePathPhotoFullScreenUrl);

            var currentPathPhotoPreviewUrl = photo.PhotoPreviewUrl.Replace(
                string.Format("{0}/{1}/", blobPrefixFormatted, StringConstants.ContainerName),
                string.Empty);
            var newFilePathPhotoPreviewUrl = currentPathPhotoPreviewUrl.Replace(
                Path.GetFileName(currentPathPhotoPreviewUrl),
                string.Format("{0}{1}{2}", Path.GetFileNameWithoutExtension(newPhotoFileName), StringConstants.SuffixPrevew, newPhotoExtension));
            await this.siteFilesRepository.ChangeFileName(currentPathPhotoPreviewUrl, newFilePathPhotoPreviewUrl);
            photo.PhotoPreviewUrl = string.Format("{0}/{1}/{2}", blobPrefixFormatted, StringConstants.ContainerName, newFilePathPhotoPreviewUrl);

            var currentPathPhotoThumbUrl = photo.PhotoThumbUrl.Replace(
                string.Format("{0}/{1}/", blobPrefixFormatted, StringConstants.ContainerName),
                string.Empty);
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