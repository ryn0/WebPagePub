using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Caching.Memory;
using WebPagePub.Core;
using WebPagePub.Core.Utilities;
using WebPagePub.Data.Constants;
using WebPagePub.Data.Models;
using WebPagePub.Data.Models.Db;
using WebPagePub.Managers.Interfaces;
using WebPagePub.Services.Interfaces;
using WebPagePub.Web.Helpers;
using WebPagePub.Web.Models;
using WebPagePub.WebApp.Models.Author;
using WebPagePub.WebApp.Models.SitePage;

namespace WebPagePub.Web.Controllers
{
    [Authorize(Roles = StringConstants.AdminRole)]
    public class SitePageManagementController : Controller
    {
        private readonly ISitePageManager sitePageManager;
        private readonly IMemoryCache memoryCache;
        private readonly ICacheService cacheService;
        private readonly UserManager<ApplicationUser> userManager;

        public SitePageManagementController(
            ISitePageManager sitePageManager,
            IMemoryCache memoryCache,
            ICacheService cacheService,
            UserManager<ApplicationUser> userManager)
        {
            this.sitePageManager = sitePageManager;
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

        [Route("sitepages/Search")]
        [HttpGet]
        public IActionResult Search(string term, int pageNumber = 1)
        {
            var quantityPerPage = 10;
            var pages = this.sitePageManager.SearchForTerm(term, pageNumber, quantityPerPage, out int total);

            var model = new SitePageSearchResultsModel()
            {
                SearchTerm = term,
                CurrentPageNumber = pageNumber,
                QuantityPerPage = quantityPerPage,
                Total = total
            };

            var pageCount = (double)model.Total / model.QuantityPerPage;
            model.PageCount = (int)Math.Ceiling(pageCount);

            foreach (var page in pages)
            {
                model.Items.Add(new SitePageItemModel()
                {
                    CreateDate = page.CreateDate,
                    IsIndex = page.IsSectionHomePage,
                    IsLive = page.IsLive,
                    IsSiteSection = page.IsSectionHomePage,
                    Key = page.Key,
                    LiveUrlPath = UrlBuilder.BlogUrlPath(page.SitePageSection.Key, page.Key),
                    PreviewUrlPath = UrlBuilder.BlogPreviewUrlPath(page.SitePageId),
                    SitePageId = page.SitePageId,
                    SitePageSectionId = page.SitePageSectionId,
                    Title = page.Title,
                    PublishDateTimeUtc = page.PublishDateTimeUtc
                });
            }

            return this.View("Search", model);
        }

        [Route("sitepages/CreateSiteSection")]
        [HttpPost]
        public IActionResult CreateSiteSection(SiteSectionCreateModel model)
        {
            if (!this.ModelState.IsValid)
            {
                throw new Exception("invalid page model");
            }

            this.sitePageManager.CreateSiteSection(
                model.Title.Trim(),
                model.Title.Trim(),
                this.userManager.GetUserId(this.User));

            return this.RedirectToAction(nameof(this.SitePages));
        }

        [Route("sitepages/EditSiteSection/{sitePageSectionId}")]
        [HttpGet]
        public IActionResult EditSiteSection(int sitePageSectionId)
        {
            var siteSection = this.sitePageManager.GetSiteSection(sitePageSectionId);

            return this.View(nameof(this.EditSiteSection), new SiteSectionEditModel()
            {
                SiteSectionId = siteSection.SitePageSectionId,
                Title = siteSection.Title,
                BreadcrumbName = siteSection.BreadcrumbName,
                IsHomePageSection = siteSection.IsHomePageSection,
                Key = siteSection.Key
            });
        }

        [Route("sitepagemanagement/DeleteSiteSection")]
        [HttpPost]
        public IActionResult DeleteSiteSection(int siteSectionId)
        {
            this.sitePageManager.GetSitePages(1, siteSectionId, 1, out int total);

            if (total > 0)
            {
                throw new Exception("There are pages for this section, they must be deleted first");
            }

            this.sitePageManager.DeleteSiteSection(siteSectionId);

            return this.RedirectToAction(nameof(this.SitePages));
        }

        [Route("sitepages/editsitesection/{sitePageSectionId}")]
        [HttpPost]
        public IActionResult EditSiteSection(SiteSectionEditModel model)
        {
            if (!this.ModelState.IsValid)
            {
                throw new Exception();
            }

            var siteSection = this.sitePageManager.GetSiteSection(model.SiteSectionId);

            siteSection.Title = model.Title.Trim();
            siteSection.Key = model.Key.UrlKey();
            siteSection.BreadcrumbName = model.BreadcrumbName.Trim();
            siteSection.IsHomePageSection = model.IsHomePageSection;

            this.sitePageManager.UpdateSiteSection(siteSection);

            return this.RedirectToAction(nameof(this.SitePages));
        }

        [Route("sitepages/setdefaultphoto/{sitePagePhotoId}")]
        [HttpGet]
        public async Task<IActionResult> SetDefaultPhotoAsync(int sitePagePhotoId)
        {
            var entry = await this.sitePageManager.SetPhotoAsDefaultAsync(sitePagePhotoId);

            return this.RedirectToAction(nameof(this.EditSitePage), new { entry.SitePageId });
        }

        [Route("sitepages")]
        public IActionResult SitePages(int siteSectionId = 0, int pageNumber = 1)
        {
            // todo: if siteSectionId = 0, show list
            var model = new SitePageListModel();

            if (siteSectionId == 0)
            {
                model.IsSiteSectionPage = true;

                var sections = this.sitePageManager.GetAllSiteSection();

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
                        IsIndex = section.IsHomePageSection,
                        IsLive = true
                    });
                }

                model.SitePageSectionId = siteSectionId;
            }
            else
            {
                model = this.SetSitePageListModel(siteSectionId, pageNumber, model);
            }

            return this.View("index", model);
        }

        [Route("sitepages/createsitepage/{sitePageSectionId}")]
        [HttpGet]
        public IActionResult CreateSitePage(int sitePageSectionId)
        {
            var siteSection = this.sitePageManager.GetSiteSection(sitePageSectionId);

            var model = new SitePageManagementCreateModel()
            {
                SiteSectionId = siteSection.SitePageSectionId,
                SiteSectionKey = siteSection.Key
            };

            return this.View(model);
        }

        [Route("sitepages/createsitepage/{sitePageSectionId}")]
        [HttpPost]
        public async Task<IActionResult> CreateSitePageAsync(SitePageManagementCreateModel model)
        {
            if (!this.ModelState.IsValid)
            {
                throw new Exception();
            }

            var titleFormattted = model.Title.Trim();
            var key = titleFormattted.UrlKey();

            if (this.sitePageManager.DoesPageExist(model.SiteSectionId, key))
            {
                throw new Exception($"Page with key '{titleFormattted}' already exists in this section");
            }

            var entry = await this.sitePageManager.CreatePageAsync(
                model.SiteSectionId,
                titleFormattted,
                this.userManager.GetUserId(this.User));

            if (entry.SitePageId > 0)
            {
                return this.RedirectToAction(nameof(this.EditSitePage), new { entry.SitePageId });
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
            var sitePageId = await this.sitePageManager.DeletePhotoAsync(sitePagePhotoId);

            return this.RedirectToAction(nameof(this.EditSitePage), new { SitePageId = sitePageId });
        }

        [Route("sitepages/rankphotoup/{sitePagePhotoId}")]
        [HttpGet]
        public IActionResult RankPhotoUp(int sitePagePhotoId)
        {
            var entry = this.sitePageManager.RankPhotoUp(sitePagePhotoId);

            return this.RedirectToAction(nameof(this.EditSitePage), new { sitePageId = entry.SitePageId });
        }

        [Route("sitepages/RankPhotoDown/{sitePagePhotoId}")]
        [HttpGet]
        public IActionResult RankPhotoDown(int sitePagePhotoId)
        {
            var entry = this.sitePageManager.RankPhotoDown(sitePagePhotoId);

            return this.RedirectToAction(nameof(this.EditSitePage), new { sitePageId = entry.SitePageId });
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
            var photosAsMemoryStreams = await ConvertFormFilesToMemoryStreamsAsync(files);
            await this.sitePageManager.UploadPhotos(sitePageId, photosAsMemoryStreams);

            return this.RedirectToAction(nameof(this.EditSitePage), new { SitePageId = sitePageId });
        }

        [Route("sitepages/deletepage/{SitePageId}")]
        [HttpGet]
        public async Task<IActionResult> DeleteAsync(int sitePageId)
        {
            await this.sitePageManager.DeletePage(sitePageId);

            return this.RedirectToAction(nameof(this.SitePages));
        }

        [Route("sitepages/rotate90degrees/{sitePagePhotoId}")]
        [HttpGet]
        public async Task<IActionResult> Rotate90DegreesAsync(int sitePagePhotoId)
        {
            var sitePageId = await this.sitePageManager.Rotate90DegreesAsync(sitePagePhotoId);

            return this.RedirectToAction(nameof(this.EditSitePage), new { SitePageId = sitePageId });
        }

        [Route("sitepages/editsitepage/{SitePageId}")]
        [HttpGet]
        public IActionResult EditSitePage(int sitePageId)
        {
            var dbModel = this.sitePageManager.GetSitePage(sitePageId);
            var sitePageSection = this.sitePageManager.GetSiteSection(dbModel.SitePageSectionId);
            var model = this.ToUiEditModel(dbModel, sitePageSection);

            model.PreviousSitePageId = this.sitePageManager.PreviouslyCreatedPage(
                dbModel.CreateDate, dbModel.SitePageId, dbModel.SitePageSectionId);
            model.NextSitePageId = this.sitePageManager.NextCreatedPage(
                dbModel.CreateDate, model.SitePageId, model.SitePageSectionId);

            return this.View(model);
        }

        [Route("sitepages/editsitepage")]
        [HttpPost]
        public async Task<IActionResult> EditSitePageAsync(SitePageEditModel model)
        {
            var dbModel = this.ConvertToDbModel(model);

            if (await this.sitePageManager.UpdateSitePage(dbModel))
            {
                var sitePagePhotoDetails = this.GetSitePagePhotoDetails(this.sitePageManager.GetBlogPhotos(model.SitePageId), this.Request.Form);
                await this.sitePageManager.UpdatePhotoProperties(model.SitePageId, sitePagePhotoDetails);
                var sitePageEditManagerModel = ConvertToSitePageEditManagerModel(model);
                this.sitePageManager.UpdateBlogTags(sitePageEditManagerModel, dbModel);

                this.ClearCache(model, dbModel);

                return this.RedirectToAction(nameof(this.EditSitePage), new { dbModel.SitePageId });
            }

            return this.View(model);
        }

        private static Managers.Models.SitePages.SitePageEditModel ConvertToSitePageEditManagerModel(
            SitePageEditModel model)
        {
            return new Managers.Models.SitePages.SitePageEditModel()
            {
                AllowsComments = model.AllowsComments,
                AuthorId = model.AuthorId,
                BlogTags = model.BlogTags,
                BreadcrumbName = model.BreadcrumbName,
                Content = model.Content,
                ExcludePageFromSiteMapXml = model.ExcludePageFromSiteMapXml,
                IsLive = model.IsLive,
                IsSectionHomePage = model.IsSectionHomePage,
                Key = model.Key,
                LiveUrlPath = model.LiveUrlPath,
                MetaDescription = model.MetaDescription,
                MetaKeywords = model.MetaKeywords,
                PageHeader = model.PageHeader,
                PageType = model.PageType,
                PreviewUrlPath = model.PreviewUrlPath,
                PublishDateTimeUtc = model.PublishDateTimeUtc,
                ReviewBestValue = model.ReviewBestValue,
                ReviewItemName = model.ReviewItemName,
                ReviewRatingValue = model.ReviewRatingValue,
                ReviewWorstValue = model.ReviewWorstValue,
                SitePageId = model.SitePageId,
                SitePageSectionId = model.SitePageSectionId,
                Tags = model.Tags,
                Title = model.Title,

                // TODO: ADD PHOTOS
            };
        }

        private static async Task<List<Tuple<string, MemoryStream>>> ConvertFormFilesToMemoryStreamsAsync(
            IEnumerable<IFormFile> files)
        {
            var list = new List<Tuple<string, MemoryStream>>();

            foreach (var file in files)
            {
                var memoryStream = new MemoryStream();
                await file.CopyToAsync(memoryStream);
                memoryStream.Seek(0, SeekOrigin.Begin);
                list.Add(new Tuple<string, MemoryStream>(file.FileName, memoryStream));
            }

            return list;
        }

        private static SitePageListModel ConvertToListModel(IList<SitePage> pages)
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
                    IsIndex = page.IsSectionHomePage,
                    PublishDateTimeUtc = page.PublishDateTimeUtc
                });
            }

            return model;
        }

        private static void AddBlogPhotoToModel(
            SitePageEditModel model,
            SitePagePhoto? photo,
            string blobPrefix,
            string cdnPrefix)
        {
            if (photo == null)
            {
                return;
            }

            model.BlogPhotos.Add(new SitePagePhotoModel
            {
                SitePagePhotoId = photo.SitePagePhotoId,
                IsDefault = photo.IsDefault,
                PhotoOriginalUrl = photo.PhotoOriginalUrl,
                PhotoFullScreenUrl = photo.PhotoFullScreenUrl,
                PhotoThumbUrl = photo.PhotoThumbUrl,
                PhotoPreviewUrl = photo.PhotoPreviewUrl,
                PhotoOriginalCdnUrl = UrlBuilder.ConvertBlobToCdnUrl(photo.PhotoOriginalUrl, blobPrefix, cdnPrefix),
                PhotoThumbCdnUrl = UrlBuilder.ConvertBlobToCdnUrl(photo.PhotoThumbUrl, blobPrefix, cdnPrefix),
                PhotoFullScreenCdnUrl = UrlBuilder.ConvertBlobToCdnUrl(photo.PhotoFullScreenUrl, blobPrefix, cdnPrefix),
                PhotoPreviewCdnUrl = UrlBuilder.ConvertBlobToCdnUrl(photo.PhotoPreviewUrl, blobPrefix, cdnPrefix),
                Title = photo.Title,
                Description = photo.Description,
                FileName = Path.GetFileName(photo.PhotoOriginalUrl)
            });
        }

        private IList<Managers.Models.SitePages.SitePagePhotoModel> GetSitePagePhotoDetails(
                IList<SitePagePhoto> sitePagePhotos,
                IFormCollection form)
        {
            var mc = new ModelConverter(this.cacheService);
            var sitePagePhotoDetails = new List<Managers.Models.SitePages.SitePagePhotoModel>();

            foreach (var sitePagePhoto in sitePagePhotos)
            {
                var fileName = form[string.Format("PhotoFileName_{0}", sitePagePhoto.SitePagePhotoId)];
                var title = form[string.Format("PhotoTitle_{0}", sitePagePhoto.SitePagePhotoId)];

                sitePagePhotoDetails.Add(new Managers.Models.SitePages.SitePagePhotoModel
                {
                    SitePagePhotoId = sitePagePhoto.SitePagePhotoId,
                    IsDefault = sitePagePhoto.IsDefault,
                    PhotoOriginalUrl = sitePagePhoto.PhotoOriginalUrl,
                    PhotoFullScreenUrl = sitePagePhoto.PhotoFullScreenUrl,
                    PhotoThumbUrl = sitePagePhoto.PhotoThumbUrl,
                    PhotoPreviewUrl = sitePagePhoto.PhotoPreviewUrl,
                    PhotoOriginalCdnUrl = UrlBuilder.ConvertBlobToCdnUrl(sitePagePhoto.PhotoOriginalUrl, mc.BlobPrefix, mc.CdnPrefix),
                    PhotoThumbCdnUrl = UrlBuilder.ConvertBlobToCdnUrl(sitePagePhoto.PhotoThumbUrl, mc.BlobPrefix, mc.CdnPrefix),
                    PhotoFullScreenCdnUrl = UrlBuilder.ConvertBlobToCdnUrl(sitePagePhoto.PhotoFullScreenUrl, mc.BlobPrefix, mc.CdnPrefix),
                    PhotoPreviewCdnUrl = UrlBuilder.ConvertBlobToCdnUrl(sitePagePhoto.PhotoPreviewUrl, mc.BlobPrefix, mc.CdnPrefix),
                    Title = title,
                    Description = title,
                    FileName = fileName
                });
            }

            return sitePagePhotoDetails;
        }

        private void ClearCache(SitePageEditModel model, SitePage dbModel)
        {
            if (dbModel.SitePageSection != null)
            {
                var cacheKey = CacheHelper.GetPageCacheKey(dbModel.SitePageSection.Key, model.Key);
                this.memoryCache.Remove(cacheKey);

                var firstPageOfSectionKey = CacheHelper.GetPageCacheKey(
                    dbModel.SitePageSection.Key,
                    WebApp.Constants.StringConstants.DefaultSectionKey,
                    1,
                    string.Empty);
                this.memoryCache.Remove(firstPageOfSectionKey);
            }
        }

        private SitePage ConvertToDbModel(SitePageEditModel model)
        {
            var dbModel = this.sitePageManager.GetSitePage(model.SitePageId);

            if (string.IsNullOrWhiteSpace(dbModel.CreatedByUserId))
            {
                dbModel.CreatedByUserId = this.userManager.GetUserId(this.User);
            }

            dbModel.SitePageSectionId = model.SitePageSectionId;
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
            dbModel.AuthorId = model.AuthorId;

            return dbModel;
        }

        private SitePageEditModel ToUiEditModel(SitePage sitePage, SitePageSection sitePageSection)
        {
            var model = new SitePageEditModel
            {
                CreateDate = sitePage.CreateDate,
                Key = sitePage.Key,
                BreadcrumbName = sitePage.BreadcrumbName,
                Content = sitePage.Content,
                PageHeader = sitePage.PageHeader,
                Title = sitePage.Title,
                SitePageId = sitePage.SitePageId,
                PublishDateTimeUtc = sitePage.PublishDateTimeUtc,
                IsLive = sitePage.IsLive,
                ExcludePageFromSiteMapXml = sitePage.ExcludePageFromSiteMapXml,
                LiveUrlPath = UrlBuilder.BlogUrlPath(sitePageSection.Key, sitePage.Key),
                PreviewUrlPath = UrlBuilder.BlogPreviewUrlPath(sitePage.SitePageId),
                MetaDescription = sitePage.MetaDescription,
                PageType = sitePage.PageType,
                ReviewBestValue = sitePage.ReviewBestValue,
                ReviewItemName = sitePage.ReviewItemName,
                ReviewRatingValue = sitePage.ReviewRatingValue,
                ReviewWorstValue = sitePage.ReviewWorstValue,
                MetaKeywords = sitePage.MetaKeywords,
                AllowsComments = sitePage.AllowsComments,
                IsSectionHomePage = sitePage.IsSectionHomePage,
                AuthorId = sitePage.AuthorId,
                Authors = this.AddAuthors().ToList(),
                SitePageSectionId = sitePageSection.SitePageSectionId,
                SiteSections = this.AddSiteSections()
            };

            var mc = new ModelConverter(this.cacheService);

            foreach (var photo in sitePage.Photos.OrderBy(x => x.Rank))
            {
                AddBlogPhotoToModel(model, photo, mc.BlobPrefix, mc.CdnPrefix);
            }

            foreach (var tagItem in sitePage.SitePageTags.OrderBy(x => x.Tag.Name))
            {
                model.BlogTags.Add(tagItem.Tag.Name);
            }

            model.BlogTags = model.BlogTags.OrderBy(x => x).ToList();

            model.Tags = string.Join(", ", model.BlogTags);

            return model;
        }

        private List<SelectListItem> AddSiteSections()
        {
            var sectionList = new List<SelectListItem>();
            var allSections = this.sitePageManager.GetAllSiteSection().OrderBy(x => x.Key);

            foreach (var section in allSections)
            {
                sectionList.Add(new SelectListItem()
                {
                    Text = section.Key,
                    Value = section.SitePageSectionId.ToString()
                });
            }

            return sectionList;
        }

        private SitePageListModel SetSitePageListModel(int siteSectionId, int pageNumber, SitePageListModel model)
        {
            var sitePageSection = this.sitePageManager.GetSiteSection(siteSectionId);
            model.IsSiteSectionPage = false;

            var pages = this.sitePageManager.GetSitePages(pageNumber, siteSectionId, WebApp.Constants.IntegerConstants.AmountPerPage, out int total);

            model = ConvertToListModel(pages);
            model.Total = total;
            model.CurrentPageNumber = pageNumber;
            model.QuantityPerPage = WebApp.Constants.IntegerConstants.AmountPerPage;
            var pageCount = (double)model.Total / model.QuantityPerPage;
            model.PageCount = (int)Math.Ceiling(pageCount);

            model.SitePageSectionId = siteSectionId;
            model.SitePageSectionTitle = sitePageSection.Title;
            return model;
        }

        private IList<SelectListItem> AddAuthors()
        {
            var authorList = new List<SelectListItem>();
            var allAuthors = this.sitePageManager.GetAllAuthors().OrderBy(x => x.FirstName);

            authorList.Add(new SelectListItem()
            {
                Text = StringConstants.NoneSelected,
                Value = StringConstants.NoneSelected,
            });

            foreach (var author in allAuthors)
            {
                var authorItem = new AuthorItem(
                    author.AuthorId,
                    author.FirstName,
                    author.LastName);

                authorList.Add(new SelectListItem()
                {
                    Text = authorItem.FullName,
                    Value = authorItem.AuthorId.ToString()
                });
            }

            return authorList;
        }
    }
}