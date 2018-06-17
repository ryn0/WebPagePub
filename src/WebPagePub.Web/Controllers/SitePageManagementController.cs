using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using WebPagePub.Web.Models;
using WebPagePub.Data.Repositories.Interfaces;
using WebPagePub.Data.Models;
using System;
using System.Collections.Generic;
using WebPagePub.Core.Utilities;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using System.Linq;
using System.Drawing;
using WebPagePub.Web.Helpers;
using System.Collections;
using WebPagePub.Services.Interfaces;
using WebPagePub.Data.Models.Db;
using WebPagePub.Data.Constants;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Memory;
using System.Net;

namespace WebPagePub.Web.Controllers
{
    [Authorize(Roles = StringConstants.AdminRole)]
    public class SitePageManagementController : Controller
    {
        const string FolderName = "page_img";
        const int AmountPerPage = 10;
        private readonly ISitePagePhotoRepository _sitePagePhotoRepository;
        private readonly ISitePageTagRepository _sitePageTagRepository;
        private readonly ITagRepository _tagRepository;
        private readonly ISitePageSectionRepository _siteSectionRepository;
        private readonly ISitePageRepository _sitePageRepository;
        private readonly ISiteFilesRepository _siteFilesRepository;
        private readonly IImageUploaderService _imageUploaderService;
        private readonly IMemoryCache _memoryCache;
        private readonly ICacheService _cacheService;
        private readonly UserManager<ApplicationUser> _userManager;

        public SitePageManagementController(
            ISitePagePhotoRepository sitePagePhotoRepository,
            ISitePageTagRepository sitePageTagRepository,
            ISitePageSectionRepository siteSectionRepository,
            ITagRepository tagRepository,
            ISitePageRepository SitePageRepository,
            ISiteFilesRepository siteFilesRepository,
            IImageUploaderService imageUploaderService,
            IMemoryCache memoryCache,
            ICacheService cacheService,
            UserManager<ApplicationUser> userManager)
        {
            _sitePagePhotoRepository = sitePagePhotoRepository;
            _sitePageTagRepository = sitePageTagRepository;
            _siteSectionRepository = siteSectionRepository;
            _tagRepository = tagRepository;
            _sitePageRepository = SitePageRepository;
            _siteFilesRepository = siteFilesRepository;
            _imageUploaderService = imageUploaderService;
            _memoryCache = memoryCache;
            _cacheService = cacheService;
            _userManager = userManager;
        }
 
        [Route("sitepages/CreateSiteSection")]
        [HttpGet]
        public IActionResult CreateSiteSection()
        {
            return View();
        }

        [Route("sitepages/CreateSiteSection")]
        [HttpPost]
        public IActionResult CreateSiteSection(CreateSiteSectionModel model)
        {
            if (!ModelState.IsValid)
            {
                throw new Exception("invalid page model");
            }

            _siteSectionRepository.Create(new SitePageSection()
            {
                Title = model.Title.Trim(),
                Key = model.Title.UrlKey(),
                BreadcrumbName = model.Title.Trim()

            });

            return RedirectToAction(nameof(SitePages));
        }

        [Route("sitepages/EditSiteSection/{sitePageSectionId}")]
        [HttpGet]
        public IActionResult EditSiteSection(int sitePageSectionId)
        {
            var siteSection = _siteSectionRepository.Get(sitePageSectionId);

            return View("EditSiteSection", new EditSiteSectionModel()
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
            var siteSection = _siteSectionRepository.Get(model.SiteSectionId);

            siteSection.Title = model.Title.Trim();
            siteSection.Key = model.Title.UrlKey();
            siteSection.BreadcrumbName = model.BreadcrumbName.Trim();
            siteSection.IsHomePageSection = model.IsHomePageSection;

            _siteSectionRepository.Update(siteSection);

            return RedirectToAction(nameof(SitePages));
        }

        [Route("sitepages/SetDefaultPhoto/{sitePagePhotoId}")]
        [HttpGet]
        public IActionResult SetDefaultPhoto(int sitePagePhotoId)
        {
            var entry = _sitePagePhotoRepository.Get(sitePagePhotoId);

            try
            {
                var req = WebRequest.Create(entry.PhotoFullScreenUrl);
                var response = req.GetResponse();
                var stream = response.GetResponseStream();
                var img = new Bitmap(stream);

                entry.PhotoFullScreenUrlHeight = img.Height;
                entry.PhotoFullScreenUrlWidth = img.Width;
                _sitePagePhotoRepository.Update(entry);

                _sitePagePhotoRepository.SetDefaultPhoto(sitePagePhotoId);

                var sitePage = _sitePageRepository.Get(entry.SitePageId);
                var sitePageSection = _siteSectionRepository.Get(sitePage.SitePageSectionId);
                var editModel = ToUiEditModel(sitePage, sitePageSection.IsHomePageSection);
                ClearCache(editModel, sitePage);
            }
            catch { throw new Exception("could not set default"); }

            return RedirectToAction("EditSitePage", new { SitePageId = entry.SitePageId });
        }

        [Route("sitepages")]
        public IActionResult SitePages(int siteSectionId = 0, int pageNumber = 1)
        {
            // todo: if siteSectionId = 0, show list
            var model = new SitePageListModel();

            if (siteSectionId == 0)
            {
                model.IsSiteSectionPage = true;

                var sections = _siteSectionRepository.GetAll();

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

                var pages = _sitePageRepository.GetPage(pageNumber, siteSectionId, AmountPerPage, out int total);

                model = ConvertToListModel(pages);
                model.Total = total;
                model.CurrentPageNumber = pageNumber;
                model.QuantityPerPage = AmountPerPage;
                var pageCount = (double)model.Total / model.QuantityPerPage;
                model.PageCount = (int)Math.Ceiling(pageCount);

                model.SitePageSectionId = siteSectionId;
            }
         
            return View("Index", model);
        }

        [Route("sitepages/CreateSitePage/{sitePageSectionId}")]
        [HttpGet]
        public IActionResult CreateSitePage(int sitePageSectionId)
        {
            var model = new SitePageManagementCreateModel()
            {
                SiteSectionId = sitePageSectionId
            };

            return View(model);
        }

        [Route("sitepages/CreateSitePage/{sitePageSectionId}")]
        [HttpPost]
        public IActionResult CreateSitePage(SitePageManagementCreateModel model)
        {
            if (!ModelState.IsValid)
                throw new Exception();

            var title = model.Title.Trim();
            var entry = _sitePageRepository.Create(new SitePage()
            {
                Title = title,
                Key = title.UrlKey(),
                PageHeader = title,
                BreadcrumbName = title,
                PublishDateTimeUtc = DateTime.UtcNow,
                SitePageSectionId = model.SiteSectionId,
                CreatedByUserId = _userManager.GetUserId(User)
            });

            if (entry.SitePageId > 0)
            {
                return RedirectToAction(nameof(EditSitePage), new { SitePageId = entry.SitePageId });
            }
            else
            {
                return View(entry);
            }
        }

        [Route("sitepages/deletephoto/{sitePagePhotoId}")]
        [HttpGet]
        public async Task<IActionResult> DeleteBlogPhotoAsync(int sitePagePhotoId)
        {
            var entry = _sitePagePhotoRepository.Get(sitePagePhotoId);

            await DeleteBlogPhoto(sitePagePhotoId);

            var allBlogPhotos = _sitePagePhotoRepository.GetBlogPhotos(entry.SitePageId)
                                                        .Where(x => x.SitePageId != sitePagePhotoId)
                                                        .OrderBy(x => x.Rank);
            int newRank = 1;

            foreach(var photo in allBlogPhotos)
            {
                photo.Rank = newRank;
                _sitePagePhotoRepository.Update(photo);

                newRank++;
            }

            return RedirectToAction(nameof(EditSitePage), new { SitePageId = entry.SitePageId });
        }

        [Route("sitepages/uploadphotos/{SitePageId}")]
        [HttpGet]
        public IActionResult UploadPhotos(int sitePageId)
        {
            var model = new SitePagePhotoUploadModel()
            {
                SitePageId = sitePageId
            };

            return View(nameof(UploadPhotos), model);
        }

        [Route("sitepages/uploadphotos")]
        [HttpPost]
        public async Task<ActionResult> UploadPhotosAsync(IEnumerable<IFormFile> files, int sitePageId)
        {
            var allBlogPhotos = _sitePagePhotoRepository.GetBlogPhotos(sitePageId);
            var highestRank = allBlogPhotos.Count();
            int currentRank = highestRank;

            try
            {
                var folderPath = GetBlogPhotoFolder(sitePageId);

                foreach (var file in files)
                {
                    if (file != null && file.Length > 0)
                    {
                        var fullsizePhotoUrl = await _siteFilesRepository.UploadAsync(file, folderPath);
                        var thumbnailPhotoUrl = await _imageUploaderService.UploadReducedQualityImage(folderPath, fullsizePhotoUrl, 300, 200, "_thumb");
                        var fullScreenPhotoUrl = await _imageUploaderService.UploadReducedQualityImage(folderPath, fullsizePhotoUrl, 1600, 1200, "_fullscreen");
                        var previewPhotoUrl = await _imageUploaderService.UploadReducedQualityImage(folderPath, fullsizePhotoUrl, 800, 600, "_preview");

                        var existingPhoto = allBlogPhotos.FirstOrDefault(x => x.PhotoUrl == fullsizePhotoUrl.ToString());

                        if (existingPhoto == null)
                        {
                            _sitePagePhotoRepository.Create(new SitePagePhoto()
                            {
                                SitePageId = sitePageId,
                                PhotoUrl = fullsizePhotoUrl.ToString(),
                                PhotoThumbUrl = thumbnailPhotoUrl.ToString(),
                                PhotoFullScreenUrl = fullScreenPhotoUrl.ToString(),
                                PhotoPreviewUrl = previewPhotoUrl.ToString(),
                                Rank = currentRank + 1
                            });

                            currentRank++;
                        }
                        else
                        {
                            existingPhoto.PhotoUrl = fullsizePhotoUrl.ToString();
                            existingPhoto.PhotoThumbUrl = thumbnailPhotoUrl.ToString();
                            existingPhoto.PhotoFullScreenUrl = fullScreenPhotoUrl.ToString();
                            existingPhoto.PhotoPreviewUrl = previewPhotoUrl.ToString();
                            _sitePagePhotoRepository.Update(existingPhoto);
                        }
                    }
                }

                return RedirectToAction(nameof(EditSitePage), new { SitePageId = sitePageId });
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
            var SitePage = _sitePageRepository.Get(sitePageId);

            foreach (var photo in SitePage.Photos)
            {
                await DeleteBlogPhoto(photo.SitePageId);
            }

            var task = Task.Run(() => _sitePageRepository.Delete(sitePageId));
            var myOutput = await task; 

            return RedirectToAction(nameof(SitePages));
        }

        [Route("sitepages/rotate90degrees")]
        [HttpGet]
        public async Task<IActionResult> Rotate90DegreesAsync(int sitePagePhotoId)
        {
            var entry = _sitePagePhotoRepository.Get(sitePagePhotoId);
            await RotateImage(entry.SitePageId, entry.PhotoUrl);
            await RotateImage(entry.SitePageId, entry.PhotoThumbUrl);

            return RedirectToAction(nameof(EditSitePage), new { SitePageId = entry.SitePageId });
        }

        [Route("sitepages/editsitepage/{SitePageId}")]
        [HttpGet]
        public IActionResult EditSitePage(int sitePageId)
        {
            var dbModel = _sitePageRepository.Get(sitePageId);
            var sitePageSection = _siteSectionRepository.Get(dbModel.SitePageSectionId);

            var model = ToUiEditModel(dbModel, sitePageSection.IsHomePageSection);

            return View(model);
        }

        [Route("sitepages/editsitepage")]
        [HttpPost]
        public IActionResult EditSitePage(SitePageEditModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var dbModel = ConvertToDbModel(model);

            if (_sitePageRepository.Update(dbModel))
            {
                var allPhotos = _sitePagePhotoRepository.GetBlogPhotos(model.SitePageId);

                foreach (var photo in allPhotos)
                {
                    photo.Title = Request.Form["PhotoTitle_" + photo.SitePageId];
                    photo.Description = Request.Form["PhotoDescription_" + photo.SitePageId];

                    _sitePagePhotoRepository.Update(photo);
                }

                SetBlogTags(model, dbModel);

                ClearCache(model, dbModel);

                return RedirectToAction(nameof(EditSitePage), new { SitePageId = dbModel.SitePageId });
            }

            return View(model);
        }

        private void ClearCache(SitePageEditModel model, SitePage dbModel)
        {
            var cacheKey = CacheHelper.GetpPageCacheKey(dbModel.SitePageSection.Key, model.Key);
            _memoryCache.Remove(cacheKey);
        }

        private async Task<SitePagePhoto> DeleteBlogPhoto(int sitePagePhotoId)
        {
            var entry = _sitePagePhotoRepository.Get(sitePagePhotoId);

            await _siteFilesRepository.DeleteFileAsync(entry.PhotoUrl);
            await _siteFilesRepository.DeleteFileAsync(entry.PhotoThumbUrl);
            await _siteFilesRepository.DeleteFileAsync(entry.PhotoFullScreenUrl);
            await _siteFilesRepository.DeleteFileAsync(entry.PhotoPreviewUrl);

            _sitePagePhotoRepository.Delete(sitePagePhotoId);

            return entry;
        }

        private SitePageListModel ConvertToListModel(List<SitePage> pages)
        {
            var model = new SitePageListModel();

            foreach(var page in pages)
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
                    IsIndex = page.IsHomePage
                });
            }

            return model;
        }

        private SitePage ConvertToDbModel(SitePageEditModel model)
        {
            var dbModel = _sitePageRepository.Get(model.SitePageId);

            if (string.IsNullOrWhiteSpace(dbModel.CreatedByUserId))
            {
                dbModel.CreatedByUserId = _userManager.GetUserId(User);
            }

            dbModel.UpdatedByUserId =_userManager.GetUserId(User);
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
            dbModel.IsHomePage = model.IsHomePage;

            return dbModel;
        }

        private SitePageEditModel ToUiEditModel(SitePage dbModel, bool  isHomePageSection)
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
                IsHomePage = dbModel.IsHomePage,
                IsHomePageSection = isHomePageSection
            };

            var mc = new ModelConverter(_cacheService);

            foreach (var photo in dbModel.Photos.OrderBy(x => x.Rank))
            {
                model.BlogPhotos.Add(new SitePagePhotoModel
                {
                    SitePagePhotoId = photo.SitePagePhotoId,
                    IsDefault = photo.IsDefault,
                    PhotoUrl = photo.PhotoUrl,
                    PhotoCdnUrl = mc.ConvertBlobToCdnUrl(photo.PhotoUrl),
                    PhotoThumbCdnUrl = mc.ConvertBlobToCdnUrl(photo.PhotoThumbUrl),
                    PhotoFullScreenCdnUrl = mc.ConvertBlobToCdnUrl(photo.PhotoFullScreenUrl),
                    PhotoPreviewCdnUrl = mc.ConvertBlobToCdnUrl(photo.PhotoPreviewUrl),
                    Title = photo.Title,
                    Description = photo.Description
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
                return;

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

            var tagsToRemove = previousTags.ToArray().Except(currentTagsFormatted.ToArray());

            AddNewTags(model, dbModel, currentTags);

            RemoveDeletedTags(model, tagsToRemove);
        }

        private void RemoveDeletedTags(SitePageEditModel model, IEnumerable<object> tagsToRemove)
        {
            foreach (var tag in tagsToRemove)
            {
                var tagKey = tag.ToString().UrlKey();

                var tagDb = _tagRepository.Get(tagKey);

                _sitePageTagRepository.Delete(tagDb.TagId, model.SitePageId);
            }
        }

        private string GetBlogPhotoFolder(int sitePageId)
        {
            return $"/{FolderName}/{sitePageId}/";
        }

        private async Task RotateImage(int SitePageId, string photoUrl)
        {
            var folderPath = GetBlogPhotoFolder(SitePageId);
            var stream = await _imageUploaderService.ToStreamAsync(photoUrl);
            var imageHelper = new ImageUtilities();
            const float angle = 90;
            var rotatedBitmap = imageHelper.RotateImage(Image.FromStream(stream), angle);

            Image fullPhoto = rotatedBitmap;

            var streamRotated = _imageUploaderService.ToAStream(fullPhoto, _imageUploaderService.SetImageFormat(photoUrl));

            await _siteFilesRepository.UploadAsync(
                                        streamRotated,
                                        photoUrl.GetFileNameFromUrl(),
                                        folderPath);

            fullPhoto.Dispose();
            streamRotated.Dispose();
            rotatedBitmap.Dispose();
        }
 

        private void AddNewTags(SitePageEditModel model, SitePage dbModel, string[] currentTags)
        {
            foreach (var tagName in currentTags)
            {
                var tagKey = tagName.UrlKey();

                if (string.IsNullOrWhiteSpace(tagKey))
                    continue;

                if (dbModel.SitePageTags.FirstOrDefault(x => x.Tag.Key == tagKey) == null)
                {
                    var tagDb = _tagRepository.Get(tagKey);

                    if (tagDb == null || tagDb.TagId == 0)
                    {
                        _tagRepository.Create(new Tag
                        {
                            Name = tagName.Trim(),
                            Key = tagKey
                        });

                        tagDb = _tagRepository.Get(tagKey);
                    }

                    _sitePageTagRepository.Create(new SitePageTag()
                    {
                        SitePageId = model.SitePageId,
                        TagId = tagDb.TagId,
                    });
                }
            }
        }

    }
}
