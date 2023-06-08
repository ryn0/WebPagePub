using System.Collections;
using System.Drawing;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using WebPagePub.Core.Utilities;
using WebPagePub.Data.Constants;
using WebPagePub.Data.Enums;
using WebPagePub.Data.Models;
using WebPagePub.Data.Models.Db;
using WebPagePub.Data.Repositories.Interfaces;
using WebPagePub.Services.Interfaces;
using WebPagePub.Web.Helpers;
using WebPagePub.Web.Models;

namespace WebPagePub.Web.Controllers
{
    [Authorize(Roles = StringConstants.AdminRole)]
    public class SitePageManagementController : Controller
    {
        private const string FolderName = "page_img";
        private const int AmountPerPage = 10;
        private readonly ISitePagePhotoRepository sitePagePhotoRepository;
        private readonly ISitePageTagRepository sitePageTagRepository;
        private readonly ITagRepository tagRepository;
        private readonly ISitePageSectionRepository siteSectionRepository;
        private readonly ISitePageRepository sitePageRepository;
        private readonly ISiteFilesRepository siteFilesRepository;
        private readonly IImageUploaderService imageUploaderService;
        private readonly IMemoryCache memoryCache;
        private readonly ICacheService cacheService;
        private readonly UserManager<ApplicationUser> userManager;

        public SitePageManagementController(
            ISitePagePhotoRepository sitePagePhotoRepository,
            ISitePageTagRepository sitePageTagRepository,
            ISitePageSectionRepository siteSectionRepository,
            ITagRepository tagRepository,
            ISitePageRepository sitePageRepository,
            ISiteFilesRepository siteFilesRepository,
            IImageUploaderService imageUploaderService,
            IMemoryCache memoryCache,
            ICacheService cacheService,
            UserManager<ApplicationUser> userManager)
        {
            this.sitePagePhotoRepository = sitePagePhotoRepository;
            this.sitePageTagRepository = sitePageTagRepository;
            this.siteSectionRepository = siteSectionRepository;
            this.tagRepository = tagRepository;
            this.sitePageRepository = sitePageRepository;
            this.siteFilesRepository = siteFilesRepository;
            this.imageUploaderService = imageUploaderService;
            this.memoryCache = memoryCache;
            this.cacheService = cacheService;
            this.userManager = userManager;
        }

        [Route("sitepages/CreateSiteSection")]
        [HttpGet]
        public IActionResult CreateSiteSection()
        {
            return this.View();
        }

        [Route("sitepages/CreateSiteSection")]
        [HttpPost]
        public IActionResult CreateSiteSection(CreateSiteSectionModel model)
        {
            if (!this.ModelState.IsValid)
            {
                throw new Exception("invalid page model");
            }

            this.siteSectionRepository.Create(new SitePageSection()
            {
                Title = model.Title.Trim(),
                Key = model.Title.UrlKey(),
                BreadcrumbName = model.Title.Trim()
            });

            return this.RedirectToAction(nameof(this.SitePages));
        }

        [Route("sitepages/EditSiteSection/{sitePageSectionId}")]
        [HttpGet]
        public IActionResult EditSiteSection(int sitePageSectionId)
        {
            var siteSection = this.siteSectionRepository.Get(sitePageSectionId);

            return this.View("EditSiteSection", new EditSiteSectionModel()
            {
                SiteSectionId = siteSection.SitePageSectionId,
                Title = siteSection.Title,
                BreadcrumbName = siteSection.BreadcrumbName,
                IsHomePageSection = siteSection.IsHomePageSection
            });
        }

        [Route("sitepages/EditSiteSection/{sitePageSectionId}")]
        [HttpPost]
        public IActionResult EditSiteSection(EditSiteSectionModel model)
        {
            var siteSection = this.siteSectionRepository.Get(model.SiteSectionId);

            siteSection.Title = model.Title.Trim();
            siteSection.Key = model.Title.UrlKey();
            siteSection.BreadcrumbName = model.BreadcrumbName.Trim();
            siteSection.IsHomePageSection = model.IsHomePageSection;

            this.siteSectionRepository.Update(siteSection);

            return this.RedirectToAction(nameof(this.SitePages));
        }

        [Route("sitepages/SetDefaultPhoto/{sitePagePhotoId}")]
        [HttpGet]
        public async Task<IActionResult> SetDefaultPhotoAsync(int sitePagePhotoId)
        {
            var entry = this.sitePagePhotoRepository.Get(sitePagePhotoId);

            try
            {
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
                var editModel = this.ToUiEditModel(sitePage, sitePageSection.IsHomePageSection);
                img.Dispose();
                this.ClearCache(editModel, sitePage);
            }
            catch
            {
                throw new Exception("could not set default");
            }
          

            return this.RedirectToAction("EditSitePage", new { SitePageId = entry.SitePageId });
        }

        [Route("sitepages")]
        public IActionResult SitePages(int siteSectionId = 0, int pageNumber = 1)
        {
            // todo: if siteSectionId = 0, show list
            var model = new SitePageListModel();

            if (siteSectionId == 0)
            {
                model.IsSiteSectionPage = true;

                var sections = this.siteSectionRepository.GetAll();

                sections = sections.OrderBy(x => x.Title).ToList();

                foreach (var section in sections)
                {
                    model.Items.Add(new SitePageItemModel()
                    {
                        IsSiteSection = true,
                        Key = section.Key,
                        Title = section.Title,
                        SitePageSectionId = section.SitePageSectionId,
                        CreateDate = section.CreateDate,
                        IsIndex = section.IsHomePageSection
                    });
                }

                model.SitePageSectionId = siteSectionId;
            }
            else
            {
                model.IsSiteSectionPage = false;

                var pages = this.sitePageRepository.GetPage(pageNumber, siteSectionId, AmountPerPage, out int total);

                model = this.ConvertToListModel(pages);
                model.Total = total;
                model.CurrentPageNumber = pageNumber;
                model.QuantityPerPage = AmountPerPage;
                var pageCount = (double)model.Total / model.QuantityPerPage;
                model.PageCount = (int)Math.Ceiling(pageCount);

                model.SitePageSectionId = siteSectionId;
            }

            return this.View("Index", model);
        }

        [Route("sitepages/CreateSitePage/{sitePageSectionId}")]
        [HttpGet]
        public IActionResult CreateSitePage(int sitePageSectionId)
        {
            var model = new SitePageManagementCreateModel()
            {
                SiteSectionId = sitePageSectionId
            };

            return this.View(model);
        }

        [Route("sitepages/CreateSitePage/{sitePageSectionId}")]
        [HttpPost]
        public IActionResult CreateSitePage(SitePageManagementCreateModel model)
        {
            if (!this.ModelState.IsValid)
            {
                throw new Exception();
            }

            var title = model.Title.Trim();
            var entry = this.sitePageRepository.Create(new SitePage()
            {
                Title = title,
                Key = title.UrlKey(),
                PageHeader = title,
                BreadcrumbName = title,
                PublishDateTimeUtc = DateTime.UtcNow,
                SitePageSectionId = model.SiteSectionId,
                CreatedByUserId = this.userManager.GetUserId(this.User)
            });

            if (entry.SitePageId > 0)
            {
                return this.RedirectToAction(nameof(this.EditSitePage), new { SitePageId = entry.SitePageId });
            }
            else
            {
                return this.View(entry);
            }
        }

        [Route("sitepages/deletephoto/{sitePagePhotoId}")]
        [HttpGet]
        public async Task<IActionResult> DeleteBlogPhotoAsync(int sitePagePhotoId)
        {
            var entry = this.sitePagePhotoRepository.Get(sitePagePhotoId);

            await this.DeleteBlogPhoto(sitePagePhotoId);

            var allBlogPhotos = this.sitePagePhotoRepository.GetBlogPhotos(entry.SitePageId)
                                                        .Where(x => x.SitePageId != sitePagePhotoId)
                                                        .OrderBy(x => x.Rank);
            int newRank = 1;

            foreach (var photo in allBlogPhotos)
            {
                photo.Rank = newRank;
                this.sitePagePhotoRepository.Update(photo);

                newRank++;
            }

            return this.RedirectToAction(nameof(this.EditSitePage), new { SitePageId = entry.SitePageId });
        }

        [Route("sitepages/RankPhotoUp/{sitePagePhotoId}")]
        [HttpGet]
        public IActionResult RankPhotoUp(int sitePagePhotoId)
        {
            var entry = sitePagePhotoRepository.Get(sitePagePhotoId);

            if (entry.Rank == 1)
                return RedirectToAction("editsitepage", new { sitePageId = entry.SitePageId });

            var allBlogPhotos = sitePagePhotoRepository.GetBlogPhotos(entry.SitePageId);

            var rankedHigher = allBlogPhotos.First(x => x.Rank == entry.Rank - 1);
            var higherRankValue = rankedHigher.Rank;
            rankedHigher.Rank = higherRankValue + 1;
            sitePagePhotoRepository.Update(rankedHigher);

            entry.Rank = higherRankValue;
            sitePagePhotoRepository.Update(entry);

            return RedirectToAction("editsitepage", new { sitePageId = entry.SitePageId });
        }

        [Route("sitepages/RankPhotoDown/{sitePagePhotoId}")]
        [HttpGet]
        public IActionResult RankPhotoDown(int sitePagePhotoId)
        {
            var entry = sitePagePhotoRepository.Get(sitePagePhotoId);
            var allBlogPhotos = sitePagePhotoRepository.GetBlogPhotos(entry.SitePageId);

            if (entry.Rank == allBlogPhotos.Count())
            {
                return RedirectToAction("editsitepage", new { sitePageId = entry.SitePageId });
            }

            var rankedLower = allBlogPhotos.First(x => x.Rank == entry.Rank + 1);
            var lowerRankValue = rankedLower.Rank;
            rankedLower.Rank = lowerRankValue - 1;
            sitePagePhotoRepository.Update(rankedLower);

            entry.Rank = lowerRankValue;
            sitePagePhotoRepository.Update(entry);

            return RedirectToAction("editsitepage", new { sitePageId = entry.SitePageId });
        }

        [Route("sitepages/uploadphotos/{SitePageId}")]
        [HttpGet]
        public IActionResult UploadPhotos(int sitePageId)
        {
            var model = new SitePagePhotoUploadModel()
            {
                SitePageId = sitePageId
            };

            return this.View(nameof(this.UploadPhotos), model);
        }

        [Route("sitepages/uploadphotos")]
        [HttpPost]
        public async Task<ActionResult> UploadPhotosAsync(IEnumerable<IFormFile> files, int sitePageId)
        {
            var allBlogPhotos = this.sitePagePhotoRepository.GetBlogPhotos(sitePageId);
            var highestRank = allBlogPhotos.Count();
            int currentRank = highestRank;

            try
            {
                var folderPath = this.GetBlogPhotoFolder(sitePageId);

                foreach (var file in files)
                {
                    if (file != null && file.Length > 0)
                    {
                        var fullsizePhotoUrl = await this.siteFilesRepository.UploadAsync(file, folderPath);
                        var thumbnailPhotoUrl = await this.imageUploaderService.UploadReducedQualityImage(folderPath, fullsizePhotoUrl, 300, 200, StringConstants.SuffixThumb);
                        var fullScreenPhotoUrl = await this.imageUploaderService.UploadReducedQualityImage(folderPath, fullsizePhotoUrl, 1600, 1200, StringConstants.SuffixFullscreen);
                        var previewPhotoUrl = await this.imageUploaderService.UploadReducedQualityImage(folderPath, fullsizePhotoUrl, 800, 600, StringConstants.SuffixPrevew);

                        var existingPhoto = allBlogPhotos.FirstOrDefault(x => x.PhotoOriginalUrl == fullsizePhotoUrl.ToString());

                        if (existingPhoto == null)
                        {
                            this.sitePagePhotoRepository.Create(new SitePagePhoto()
                            {
                                SitePageId = sitePageId,
                                PhotoOriginalUrl = fullsizePhotoUrl.ToString(),
                                PhotoThumbUrl = thumbnailPhotoUrl.ToString(),
                                PhotoFullScreenUrl = fullScreenPhotoUrl.ToString(),
                                PhotoPreviewUrl = previewPhotoUrl.ToString(),
                                Rank = currentRank + 1
                            });

                            currentRank++;
                        }
                        else
                        {
                            existingPhoto.PhotoOriginalUrl = fullsizePhotoUrl.ToString();
                            existingPhoto.PhotoThumbUrl = thumbnailPhotoUrl.ToString();
                            existingPhoto.PhotoFullScreenUrl = fullScreenPhotoUrl.ToString();
                            existingPhoto.PhotoPreviewUrl = previewPhotoUrl.ToString();
                            this.sitePagePhotoRepository.Update(existingPhoto);
                        }
                    }
                }

                return this.RedirectToAction(nameof(this.EditSitePage), new { SitePageId = sitePageId });
            }
            catch (Exception ex)
            {
                throw new Exception("Upload failed", ex.InnerException);
            }
        }

        [Route("sitepages/deletepage/{SitePageId}")]
        [HttpPost]
        public async Task<IActionResult> DeleteAsync(int sitePageId)
        {
            var sitePage = this.sitePageRepository.Get(sitePageId);

            foreach (var photo in sitePage.Photos)
            {
                await this.DeleteBlogPhoto(photo.SitePageId);
            }

            var task = Task.Run(() => this.sitePageRepository.Delete(sitePageId));
            var myOutput = await task;

            return this.RedirectToAction(nameof(this.SitePages));
        }

        [Route("sitepages/rotate90degrees/{sitePagePhotoId}")]
        [HttpGet]
        public async Task<IActionResult> Rotate90DegreesAsync(int sitePagePhotoId)
        {
            var entry = this.sitePagePhotoRepository.Get(sitePagePhotoId);

            // todo: store original and rotate it, resize it, to low quality loss

            var photoOriginalUrl = await RotateImage(entry.SitePageId, entry.PhotoOriginalUrl);
            var photoPreviewUrl = await RotateImage(entry.SitePageId, entry.PhotoPreviewUrl);
            var photoThumbUrl = await RotateImage(entry.SitePageId, entry.PhotoThumbUrl);
            var photoFullScreenUrl = await RotateImage(entry.SitePageId, entry.PhotoFullScreenUrl);

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

            return this.RedirectToAction(nameof(this.EditSitePage), new { SitePageId = entry.SitePageId });
        }

        [Route("sitepages/editsitepage/{SitePageId}")]
        [HttpGet]
        public IActionResult EditSitePage(int sitePageId)
        {
            var dbModel = this.sitePageRepository.Get(sitePageId);
            var sitePageSection = this.siteSectionRepository.Get(dbModel.SitePageSectionId);

            var model = this.ToUiEditModel(dbModel, sitePageSection.IsHomePageSection);

            return this.View(model);
        }

        [Route("sitepages/editsitepage")]
        [HttpPost]
        public async Task<IActionResult> EditSitePageAsync(SitePageEditModel model)
        {
            // todo: do not validate all
            //if (!this.ModelState.IsValid)
            //{
            //    return this.View(model);
            //}

            var dbModel = this.ConvertToDbModel(model);

            if (this.sitePageRepository.Update(dbModel))
            {
                var allPhotos = this.sitePagePhotoRepository.GetBlogPhotos(model.SitePageId);

                foreach (var photo in allPhotos)
                {
                    var hasChanged = false;

                    var title = this.Request.Form["PhotoTitle_" + photo.SitePagePhotoId];

                    if (title != photo.Title)
                    {
                        hasChanged = true;
                        photo.Title = title;
                        photo.Description = photo.Title;
                    }

                    // make this the same for now this.Request.Form["PhotoDescription_" + photo.SitePagePhotoId];

                    var photoFileName = this.Request.Form["PhotoFileName_" + photo.SitePagePhotoId].ToString().Trim();

                    var currentFileName = Path.GetFileName(photo.PhotoOriginalUrl);

                    if (Path.HasExtension(photoFileName) &&
                        Path.HasExtension(currentFileName) &&
                        (photoFileName != currentFileName))
                    {
                        hasChanged = true;
                        await RenameAllPhotoVarients(photo, photoFileName, currentFileName);
                    }

                    if (hasChanged)
                    {
                        this.sitePagePhotoRepository.Update(photo);
                    }
                }

                this.SetBlogTags(model, dbModel);

                this.ClearCache(model, dbModel);

                return this.RedirectToAction(nameof(this.EditSitePage), new { SitePageId = dbModel.SitePageId });
            }

            return this.View(model);
        }

        private async Task RenameAllPhotoVarients(SitePagePhoto photo, string newPhotoFileName, string currentFileName)
        {
            var blobPrefix = this.cacheService.GetSnippet(SiteConfigSetting.BlobPrefix);

            var currentPathPhotoUrl = photo.PhotoOriginalUrl.Replace(string.Format("{0}/{1}/",
               blobPrefix, StringConstants.ContainerName), string.Empty);
            var newFilePathPhotoUrl = currentPathPhotoUrl.Replace(currentFileName, newPhotoFileName);
            await siteFilesRepository.ChangeFileName(currentPathPhotoUrl, newFilePathPhotoUrl);
            photo.PhotoOriginalUrl = string.Format("{0}/{1}/{2}", blobPrefix, StringConstants.ContainerName, newFilePathPhotoUrl);

            //
            var newPhotoExtension = Path.GetExtension(newPhotoFileName);
            
            //
            var currentPathPhotoFullScreenUrl = photo.PhotoFullScreenUrl.Replace(string.Format("{0}/{1}/",
                blobPrefix, StringConstants.ContainerName), string.Empty);
            var newFilePathPhotoFullScreenUrl = currentPathPhotoFullScreenUrl.Replace(Path.GetFileName(currentPathPhotoFullScreenUrl), 
                string.Format("{0}{1}{2}", Path.GetFileNameWithoutExtension(newPhotoFileName), StringConstants.SuffixFullscreen, newPhotoExtension));
            await siteFilesRepository.ChangeFileName(currentPathPhotoFullScreenUrl, newFilePathPhotoFullScreenUrl);
            photo.PhotoFullScreenUrl = string.Format("{0}/{1}/{2}", blobPrefix, StringConstants.ContainerName,newFilePathPhotoFullScreenUrl);

            //
            var currentPathPhotoPreviewUrl = photo.PhotoPreviewUrl.Replace(string.Format("{0}/{1}/",
                blobPrefix, StringConstants.ContainerName), string.Empty);
            var newFilePathPhotoPreviewUrl = currentPathPhotoPreviewUrl.Replace(Path.GetFileName(currentPathPhotoPreviewUrl),
                string.Format("{0}{1}{2}", Path.GetFileNameWithoutExtension(newPhotoFileName), StringConstants.SuffixPrevew, newPhotoExtension));
            await siteFilesRepository.ChangeFileName(currentPathPhotoPreviewUrl, newFilePathPhotoPreviewUrl);
            photo.PhotoPreviewUrl = string.Format("{0}/{1}/{2}", blobPrefix, StringConstants.ContainerName, newFilePathPhotoPreviewUrl);

            //
            var currentPathPhotoThumbUrl = photo.PhotoThumbUrl.Replace(string.Format("{0}/{1}/",
                blobPrefix, StringConstants.ContainerName), string.Empty);
            var newFilePathPhotoThumbUrl = currentPathPhotoThumbUrl.Replace(Path.GetFileName(currentPathPhotoThumbUrl),
                string.Format("{0}{1}{2}", Path.GetFileNameWithoutExtension(newPhotoFileName), StringConstants.SuffixThumb, newPhotoExtension));
            await siteFilesRepository.ChangeFileName(currentPathPhotoThumbUrl, newFilePathPhotoThumbUrl);
            photo.PhotoThumbUrl = string.Format("{0}/{1}/{2}", blobPrefix, StringConstants.ContainerName, newFilePathPhotoThumbUrl);

        }

        private void ClearCache(SitePageEditModel model, SitePage dbModel)
        {
            var cacheKey = CacheHelper.GetPageCacheKey(dbModel.SitePageSection.Key, model.Key);
            this.memoryCache.Remove(cacheKey);
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

        private SitePageListModel ConvertToListModel(List<SitePage> pages)
        {
            var model = new SitePageListModel();

            foreach (var page in pages)
            {
                model.Items.Add(new SitePageItemModel()
                {
                    SitePageId = page.SitePageId,
                    CreateDate = page.CreateDate,
                    Title = page.Title,
                    IsLive = page.IsLive,
                    Key = page.Key,
                    LiveUrlPath = UrlBuilder.BlogUrlPath(page.SitePageSection.Key, page.Key),
                    PreviewUrlPath = UrlBuilder.BlogPreviewUrlPath(page.SitePageId),
                    IsIndex = page.IsSectionHomePage
                });
            }

            return model;
        }

        private SitePage ConvertToDbModel(SitePageEditModel model)
        {
            var dbModel = this.sitePageRepository.Get(model.SitePageId);

            if (string.IsNullOrWhiteSpace(dbModel.CreatedByUserId))
            {
                dbModel.CreatedByUserId = this.userManager.GetUserId(this.User);
            }

            dbModel.UpdatedByUserId = this.userManager.GetUserId(this.User);
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

            return dbModel;
        }

        private SitePageEditModel ToUiEditModel(SitePage dbModel, bool isHomePageSection)
        {
            var model = new SitePageEditModel()
            {
                Key = dbModel.Key,
                BreadcrumbName = dbModel.BreadcrumbName,
                Content = dbModel.Content,
                PageHeader = dbModel.PageHeader,
                Title = dbModel.Title,
                SitePageId = dbModel.SitePageId,
                PublishDateTimeUtc = dbModel.PublishDateTimeUtc,
                IsLive = dbModel.IsLive,
                ExcludePageFromSiteMapXml = dbModel.ExcludePageFromSiteMapXml,
                LiveUrlPath = UrlBuilder.BlogUrlPath(dbModel.SitePageSection.Key, dbModel.Key),
                PreviewUrlPath = UrlBuilder.BlogPreviewUrlPath(dbModel.SitePageId),
                MetaDescription = dbModel.MetaDescription,
                PageType = dbModel.PageType,
                ReviewBestValue = dbModel.ReviewBestValue,
                ReviewItemName = dbModel.ReviewItemName,
                ReviewRatingValue = dbModel.ReviewRatingValue,
                ReviewWorstValue = dbModel.ReviewWorstValue,
                MetaKeywords = dbModel.MetaKeywords,
                AllowsComments = dbModel.AllowsComments,
                IsSectionHomePage = dbModel.IsSectionHomePage,
            };

            var mc = new ModelConverter(this.cacheService);

            foreach (var photo in dbModel.Photos.OrderBy(x => x.Rank))
            {
                model.BlogPhotos.Add(new SitePagePhotoModel
                {
                    SitePagePhotoId = photo.SitePagePhotoId,
                    IsDefault = photo.IsDefault,
                    PhotoOriginalUrl = photo.PhotoOriginalUrl,
                    PhotoFullScreenUrl = photo.PhotoFullScreenUrl,
                    PhotoThumbUrl = photo.PhotoThumbUrl,
                    PhotoPreviewUrl = photo.PhotoPreviewUrl,
                    PhotoOriginalCdnUrl = mc.ConvertBlobToCdnUrl(photo.PhotoOriginalUrl),
                    PhotoThumbCdnUrl = mc.ConvertBlobToCdnUrl(photo.PhotoThumbUrl),
                    PhotoFullScreenCdnUrl = mc.ConvertBlobToCdnUrl(photo.PhotoFullScreenUrl),
                    PhotoPreviewCdnUrl = mc.ConvertBlobToCdnUrl(photo.PhotoPreviewUrl),
                    Title = photo.Title,
                    Description = photo.Description,
                    FileName = Path.GetFileName(photo.PhotoOriginalUrl)
                });
            }

            foreach (var tagItem in dbModel.SitePageTags.OrderBy(x => x.Tag.Name))
            {
                model.BlogTags.Add(tagItem.Tag.Name);
            }

            model.BlogTags = model.BlogTags.OrderBy(x => x).ToList();

            model.Tags = string.Join(", ", model.BlogTags);

            return model;
        }

        private void SetBlogTags(SitePageEditModel model, SitePage dbModel)
        {
            if (model.Tags == null)
            {
                return;
            }

            var currentTags = model.Tags.Split(',');
            var currentTagsFormatted = new ArrayList();

            foreach (var tag in currentTags)
            {
                currentTagsFormatted.Add(tag.UrlKey());
            }

            var currentTagsFormattedArray = currentTagsFormatted.ToArray();
            var previousTags = new ArrayList();

            foreach (var tag in dbModel.SitePageTags)
            {
                previousTags.Add(tag.Tag.Key);
            }

            var tagsToRemove = previousTags.ToArray().Except(currentTagsFormattedArray);

            this.AddNewTags(model, dbModel, currentTags);

            this.RemoveDeletedTags(model, tagsToRemove);
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

        private string GetBlogPhotoFolder(int sitePageId)
        {
            return $"/{FolderName}/{sitePageId}/";
        }

        private async Task<Uri> RotateImage(int sitePageId, string photoUrl)
        {
            var folderPath = this.GetBlogPhotoFolder(sitePageId);
            var stream = await this.imageUploaderService.ToStreamAsync(photoUrl);
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
            }
        }
    }
}
