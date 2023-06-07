using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using System.Text;
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
    // todo: cache keys which are retrieved for lookups
    public class HomeController : Controller
    {
        private readonly ISpamFilterService spamFilterService;
        private readonly ISitePageCommentRepository sitePageCommentRepository;
        private readonly ISitePageRepository sitePageRepository;
        private readonly ISitePageSectionRepository sitePageSectionRepository;
        private readonly ISitePageTagRepository sitePageTagRepository;
        private readonly ITagRepository tagRepository;
        private readonly IMemoryCache memoryCache;
        private readonly ICacheService cacheService;
        private IHttpContextAccessor accessor;

        public HomeController(
            IHttpContextAccessor accessor,
            ISpamFilterService spamFilterService,
            ISitePageCommentRepository stePageCommentRepository,
            ISitePageRepository sitePageRepository,
            ISitePageSectionRepository sitePageSectionRepository,
            ISitePageTagRepository sitePageTagRepository,
            ITagRepository tagRepository,
            IMemoryCache memoryCache,
            ICacheService cacheService)
        {
            this.accessor = accessor;
            this.spamFilterService = spamFilterService;
            this.sitePageCommentRepository = stePageCommentRepository;
            this.sitePageRepository = sitePageRepository;
            this.sitePageSectionRepository = sitePageSectionRepository;
            this.sitePageTagRepository = sitePageTagRepository;
            this.tagRepository = tagRepository;
            this.memoryCache = memoryCache;
            this.cacheService = cacheService;
        }

        [HttpGet]
        public IActionResult Index()
        {
            var homeSection = this.sitePageSectionRepository.GetHomeSection();

            if (homeSection == null)
            {
                return Show404Page();
            }

            var homePage = this.sitePageRepository.GetSectionHomePage(homeSection.SitePageSectionId);

            if (homePage == null)
            {
                return Show404Page();
            }

            return this.CatchAllRequests(homeSection.Key, homePage.Key);
        }

        [Route("{sectionKey}/page/{pageNumber}")]
        [HttpGet]
        public IActionResult SectionPage(string sectionKey, int pageNumber = 1)
        {
            var section = this.sitePageSectionRepository.Get(sectionKey);
            var homePage = this.sitePageRepository.GetSectionHomePage(section.SitePageSectionId);

            return this.CatchAllRequests(sectionKey, homePage.Key, false, pageNumber);
        }

        [Route("tag/{tagName}/page/{pageNumber}")]
        [HttpGet]
        public IActionResult TagPage(string tagName, int pageNumber = 1)
        {
            var homeSection = this.sitePageSectionRepository.GetHomeSection();
            var homePage = this.sitePageRepository.GetSectionHomePage(homeSection.SitePageSectionId);

            return this.CatchAllRequests(
                        tagKey: tagName,
                        pageKey: homePage.Key,
                        isPreview: false,
                        pageNumber: pageNumber);
        }

        [Route("tag/{tagName}")]
        [HttpGet]
        public IActionResult TagPage(string tagName)
        {
            var homeSection = this.sitePageSectionRepository.GetHomeSection();
            var homePage = this.sitePageRepository.GetSectionHomePage(homeSection.SitePageSectionId);

            return this.CatchAllRequests(
                        tagKey: tagName,
                        pageKey: homePage.Key,
                        isPreview: false);
        }

        [Route("{sectionKey}/{pageKey}")]
        [HttpGet]
        public IActionResult Index(string sectionKey, string pageKey)
        {
            return this.CatchAllRequests(sectionKey, pageKey);
        }

        [Route("{sectionKey}")]
        [HttpGet]
        public IActionResult Index(string sectionKey)
        {
            var section = this.sitePageSectionRepository.Get(sectionKey);
            var homePage = this.sitePageRepository.GetSectionHomePage(section.SitePageSectionId);

            return this.CatchAllRequests(sectionKey, homePage.Key);
        }

        [Route(StringConstants.PreviewKey + "/{sitePageId}")]
        [HttpGet]
        public IActionResult Preview(int sitePageId)
        {
            var dbModel = this.sitePageRepository.Get(sitePageId);
            var siteSection = this.sitePageSectionRepository.Get(dbModel.SitePageSectionId);

            return this.CatchAllRequests(siteSection.Key, dbModel.Key, true);
        }

        [Route(StringConstants.Tags)]
        [HttpGet]
        //[ResponseCache(Duration = 120, Location = ResponseCacheLocation.Any, NoStore = false)]
        public IActionResult Tags()
        {
            var allSitePageTags = sitePageTagRepository.GetAll();
            var allTags = tagRepository.GetAll();

            allTags = allTags.OrderBy(x => x.Name).ToList();

            var sb = new StringBuilder();

            sb.AppendLine("<ul>");
            foreach (var tag in allTags)
            {
                if (allSitePageTags.FirstOrDefault(x => x.TagId == tag.TagId) != null)
                {
                    var totalTagged = allSitePageTags.Count(x => x.TagId == tag.TagId);
                    sb.Append("<li>");
                    sb.AppendFormat(@"<a href=""/tag/{0}"">{1}</a> ({2})", tag.Key, tag.Name, totalTagged);
                    sb.Append("</li>");
                    sb.AppendLine();
                }
            }
            sb.AppendLine("</ul>");

            var title = "Tags";
            var sitePage = new SitePageDisplayModel(cacheService);
            sitePage.PageContent.Title = title;
            sitePage.PageContent.PageHeader = title;
            sitePage.PageContent.Content = sb.ToString();

            return View(sitePage);
        }

        [HttpPost]
        public IActionResult Comment(SitePageCommentModel model)
        {
            var existingComment = this.sitePageCommentRepository.Get(model.RequestId);

            if (existingComment != null)
            {
                return this.View("Commented");
            }

            if (!this.ModelState.IsValid)
            {
                return this.View("CommentError");
            }

            if (!SiteUtilityHelper.IsCaptchaValid(this.Request.Form))
            {
                return this.View("CommentError");
            }

            var ipAddress = this.accessor.HttpContext.Connection.RemoteIpAddress.ToString();

            if (this.spamFilterService.IsBlocked(ipAddress))
            {
                return this.BadRequest();
            }

            this.sitePageCommentRepository.Create(new SitePageComment()
            {
                Comment = model.Comment.Trim(),
                Email = model.Email.Trim(),
                CommentStatus = CommentStatus.AwaitingModeration,
                Name = model.Name.Trim(),
                RequestId = model.RequestId,
                IpAddress = ipAddress,
                SitePageId = model.SitePageId,
                WebSite = model.Website?.Trim()
            });

            return this.View("Commented");
        }

        private IActionResult CatchAllRequests(
                                string sectionKey = null,
                                string pageKey = null,
                                bool isPreview = false,
                                int pageNumber = 1,
                                string tagKey = null)
        {
            var cacheKey = CacheHelper.GetPageCacheKey(sectionKey, pageKey, isPreview, pageNumber, tagKey);
            var cachedPage = this.memoryCache.Get(cacheKey);

            SitePageDisplayModel model;

            if (cachedPage == null)
            {
                if (string.IsNullOrWhiteSpace(tagKey))
                {
                    var siteSection = this.sitePageSectionRepository.Get(sectionKey);
                    if (siteSection == null)
                    {
                        return this.Show404Page();
                    }

                    var dbModel = this.sitePageRepository.Get(siteSection.SitePageSectionId, pageKey);

                    if (dbModel == null || (!isPreview && !dbModel.IsLive))
                    {
                        return this.Show404Page();
                    }

                    switch (dbModel.PageType)
                    {
                        case PageType.PageList:
                            model = this.CreateDisplayListModel(siteSection, dbModel, tagKey, pageNumber);
                            break;
                        case PageType.Content:
                        case PageType.Review:
                        case PageType.Photo:
                        default:
                            model = this.CreateDisplayModel(siteSection, dbModel);
                            break;
                    }

                    model.SitePageId = dbModel.SitePageId;
                    model.SectionKey = siteSection.Key;
                    model.IsHomePageSection = siteSection.IsHomePageSection;
                }
                else
                {
                    model = this.CreateDisplayListModel(tagKey: tagKey, pageNumber: pageNumber);
                }

                this.memoryCache.Set(cacheKey, model, DateTime.UtcNow.AddMinutes(10));
            }
            else
            {
                model = (SitePageDisplayModel)cachedPage;
            }

            switch (model.PageType)
            {
                case PageType.PageList:
                    return this.View("SectionList", model);
                case PageType.Review:
                    return this.View("Review", model);
                case PageType.Content:
                default:
                    return this.View("Content", model);
            }
        }

        private SitePageDisplayModel CreateDisplayListModel(
            SitePageSection? sitePageSection = null,
            SitePage sitePage = null,
            string tagKey = null,
            int pageNumber = 1)
        {
            var displayModel = new SitePageDisplayModel(this.cacheService);
            List<SitePage> pages;
            int total = 0;

            if (string.IsNullOrWhiteSpace(tagKey))
            {
                var contentModel = this.CreatePageContentModel(sitePageSection, sitePage);

                displayModel = new SitePageDisplayModel(this.cacheService)
                {
                    BreadcrumbList = this.BuildBreadcrumbList(sitePageSection, sitePage),
                    PageType = sitePage.PageType,
                    Review = this.BuildReviewModel(sitePageSection, sitePage),
                    PageContent = contentModel
                };

                pages = this.sitePageRepository.GetLivePageBySection(
                                                            sitePageSection.SitePageSectionId,
                                                            pageNumber,
                                                            IntegerConstants.PageSize,
                                                            out total);

                pages = pages.Where(x => x.IsSectionHomePage == false).ToList();
            }
            else
            {
                displayModel.TagKey = tagKey;
                displayModel.TagKeyword = this.tagRepository.Get(tagKey).Name;
                displayModel.PageContent.Title = $"{displayModel.TagKeyword} - Pages Tagged";
                displayModel.PageContent.PageHeader = displayModel.PageContent.Title;
                displayModel.PageContent.MetaDescription = $"Pages which are tagged {displayModel.TagKeyword}";
                displayModel.PageType = PageType.PageList;

                pages = this.sitePageRepository.GetLivePageByTag(
                                                            tagKey,
                                                            pageNumber,
                                                            IntegerConstants.PageSize,
                                                            out total);
            }

            var pageCount = (double)total / IntegerConstants.PageSize;

            displayModel.Paging = new SitePagePagingModel()
            {
                CurrentPageNumber = pageNumber,
                PageCount = (int)Math.Ceiling(pageCount),
                QuantityPerPage = IntegerConstants.PageSize,
                Total = total
            };

            foreach (var page in pages)
            {
                var itemPageModel = this.CreatePageContentModel(page.SitePageSection, page);

                if (itemPageModel.IsIndex)
                {
                    continue;
                }

                displayModel.Items.Add(itemPageModel);
            }

            return displayModel;
        }

        private SitePageDisplayModel CreateDisplayModel(SitePageSection sitePageSection, SitePage sitePage)
        {
            var contentModel = this.CreatePageContentModel(sitePageSection, sitePage);
            var comments = this.BuildComments(sitePage);

            var displayModel = new SitePageDisplayModel(this.cacheService)
            {
                BreadcrumbList = this.BuildBreadcrumbList(sitePageSection, sitePage),
                PageType = sitePage.PageType,
                Review = this.BuildReviewModel(sitePageSection, sitePage),
                PageContent = contentModel,
                Comments = comments,
                AllowCommenting = sitePage.AllowsComments,
            };

            displayModel.PostComment.SitePageId = sitePage.SitePageId;

            return displayModel;
        }

        private List<SitePageCommentModel> BuildComments(SitePage sitePage)
        {
            var commentModel = new List<SitePageCommentModel>();

            if (!sitePage.AllowsComments)
            {
                return commentModel;
            }

            var pageComments = this.sitePageCommentRepository.GetCommentsForPage(sitePage.SitePageId, CommentStatus.Approved);

            foreach (var commentItem in pageComments)
            {
                commentModel.Add(new SitePageCommentModel()
                {
                    Comment = commentItem.Comment,
                    CommentStatus = commentItem.CommentStatus,
                    Email = commentItem.Email,
                    Name = commentItem.Name,
                    SitePageCommentId = commentItem.SitePageCommentId,
                    SitePageId = sitePage.SitePageId,
                    Website = commentItem.WebSite,
                    CreateDate = commentItem.CreateDate
                });
            }

            return commentModel;
        }

        private SitePageContentModel CreatePageContentModel(SitePageSection sitePageSection, SitePage sitePage)
        {
            var blobPrefix = this.cacheService.GetSnippet(SiteConfigSetting.BlobPrefix);
            var cdnPrefix = this.cacheService.GetSnippet(SiteConfigSetting.CdnPrefixWithProtocol);

            var canonicalUrl = new Uri(UrlBuilder.GetCurrentDomain(this.HttpContext) +
                    UrlBuilder.BlogUrlPath(sitePageSection.Key, sitePage.Key));

            var defaultPhotoUrl = sitePage?.Photos.FirstOrDefault(x => x.IsDefault == true);

            var displayModel = CreateDisplayModel(sitePageSection, sitePage, blobPrefix, cdnPrefix, canonicalUrl, defaultPhotoUrl);

            if (sitePage.Photos.Any())
            {
                LoadPhotos(sitePage, blobPrefix, cdnPrefix, displayModel);
            }

            if (displayModel.Tags != null)
            {
                foreach (var tagEntry in sitePage.SitePageTags)
                {
                    displayModel.Tags.Add(tagEntry.Tag.Name);
                }

                displayModel.Tags = displayModel.Tags.OrderBy(x => x).ToList();
            }

            return displayModel;
        }

        private SitePageContentModel CreateDisplayModel(
            SitePageSection sitePageSection, 
            SitePage sitePage, 
            string blobPrefix, 
            string cdnPrefix, 
            Uri canonicalUrl, 
            SitePagePhoto? defaultPhotoUrl)
        {
            return new SitePageContentModel()
            {
                BreadcrumbName = sitePage.BreadcrumbName,
                Title = sitePage.Title,
                PageHeader = sitePage.PageHeader,
                MetaDescription = sitePage.MetaDescription,
                Content = sitePage.Content,
                LastUpdatedDateTimeUtc = sitePage.UpdateDate ?? sitePage.CreateDate,
                PublishedDateTime = sitePage.PublishDateTimeUtc,
                CanonicalUrl = canonicalUrl.ToString(),
                PhotoOriginalUrl = this.ConvertBlobToCdnUrl(blobPrefix, cdnPrefix, defaultPhotoUrl?.PhotoFullScreenUrl),
                PhotoUrlHeight = defaultPhotoUrl != null ? defaultPhotoUrl.PhotoFullScreenUrlHeight : 0,
                PhotoUrlWidth = defaultPhotoUrl != null ? defaultPhotoUrl.PhotoFullScreenUrlWidth : 0,
                MetaKeywords = sitePage.MetaKeywords,
                UrlPath = UrlBuilder.BlogUrlPath(sitePageSection.Key, sitePage.Key),
                Key = sitePage.Key,
                SectionKey = sitePageSection.Key,
                DefaultPhotoThumbCdnUrl = this.ConvertBlobToCdnUrl(blobPrefix, cdnPrefix, defaultPhotoUrl?.PhotoThumbUrl),
            };
        }

        private void LoadPhotos(SitePage sitePage, string blobPrefix, string cdnPrefix, SitePageContentModel displayModel)
        {
            sitePage.Photos = sitePage.Photos.OrderBy(x => x.Rank).ToList();

            foreach (var photo in sitePage.Photos)
            {
                displayModel.Photos.Add(new SitePagePhotoModel()
                {
                    Description = photo.Description,
                    IsDefault = photo.IsDefault,
                    PhotoOriginalCdnUrl = this.ConvertBlobToCdnUrl(blobPrefix, cdnPrefix, photo.PhotoOriginalUrl),
                    PhotoOriginalUrl = photo.PhotoOriginalUrl,
                    PhotoFullScreenCdnUrl = this.ConvertBlobToCdnUrl(blobPrefix, cdnPrefix, photo.PhotoFullScreenUrl),
                    PhotoFullScreenUrl = photo.PhotoFullScreenUrl,
                    PhotoPreviewCdnUrl = this.ConvertBlobToCdnUrl(blobPrefix, cdnPrefix, photo.PhotoPreviewUrl),
                    PhotoPreviewUrl = photo.PhotoPreviewUrl,
                    PhotoThumbCdnUrl = this.ConvertBlobToCdnUrl(blobPrefix, cdnPrefix, photo.PhotoThumbUrl),
                    PhotoThumbUrl = photo.PhotoThumbUrl,
                    SitePagePhotoId = photo.SitePagePhotoId,
                    Title = photo.Title
                });
            }
        }

        private string ConvertBlobToCdnUrl(string blobPrefix, string cdnPrefix, string blobUrl)
        {
            if (string.IsNullOrWhiteSpace(blobUrl))
            {
                return null;
            }

            return blobUrl.Replace(blobPrefix, cdnPrefix);
        }

        private StructureDataReviewModel BuildReviewModel(SitePageSection sitePageSection, SitePage sitePage)
        {
            if (sitePage.PageType != PageType.Review)
            {
                return null;
            }

            var ratingPercentage = (sitePage.ReviewRatingValue / (sitePage.ReviewBestValue - sitePage.ReviewWorstValue)) * 100;

            return new StructureDataReviewModel(this.cacheService)
            {
                ItemReviewed = new ItemReviewed()
                {
                    Name = sitePage.ReviewItemName
                },
                ReviewRating = new ReviewRating()
                {
                    BestRating = sitePage.ReviewBestValue.ToString("0.0"),
                    RatingValue = sitePage.ReviewRatingValue.ToString("0.0"),
                    WorstRating = sitePage.ReviewWorstValue.ToString("0.0"),
                    RatingPercentage = ratingPercentage.ToString("0.0")
                },

                // Author = new Author()
                // {
                //    Name = StringConstants.WebsiteAuthor //TODO: GET FROM SITEPAGE USER
                // },
                Publisher = new Publisher()
                {
                    Name = this.cacheService.GetSnippet(SiteConfigSetting.WebsiteName)
                }
            };
        }

        private StructuredDataBreadcrumbModel BuildBreadcrumbList(SitePageSection sitePageSection, SitePage sitePage)
        {
            var domain = UrlBuilder.GetCurrentDomain(this.HttpContext);

            var cacheKey = CacheHelper.GetpPageCacheKey(sitePageSection);
            SitePageSection homeSection;
            var cachedValue = this.memoryCache.Get(cacheKey);

            if (cachedValue != null)
            {
                homeSection = (SitePageSection)cachedValue;
            }
            else
            {
                homeSection = this.sitePageSectionRepository.GetHomeSection();

                this.memoryCache.Set(cacheKey, homeSection, DateTime.UtcNow.AddMinutes(10));
            }

            if (homeSection == null)
            {
                return new StructuredDataBreadcrumbModel();
            }

            var breadcrumbList = new StructuredDataBreadcrumbModel()
            {
                ItemListElement = new List<BreadcrumbListItem>()
                      {
                           new BreadcrumbListItem()
                           {
                               Position = 1,
                                Item = new BreadcrumbListItemProperties()
                                {
                                     Name = homeSection.BreadcrumbName,
                                     PageUrl = new Uri(domain)
                                }
                           }
                }
            };

            if (!sitePageSection.IsHomePageSection)
            {
                breadcrumbList.ItemListElement.Add(new BreadcrumbListItem()
                {
                    Position = 2,
                    Item = new BreadcrumbListItemProperties()
                    {
                        Name = sitePageSection.BreadcrumbName,
                        PageUrl = new Uri(new Uri(domain), sitePageSection.Key),
                    },
                });
            }

            if (!sitePage.IsSectionHomePage)
            {
                breadcrumbList.ItemListElement.Add(
                           new BreadcrumbListItem()
                           {
                               Position = 3,
                               Item = new BreadcrumbListItemProperties()
                               {
                                   Name = sitePage.BreadcrumbName,
                                   PageUrl = new Uri(new Uri(domain), UrlBuilder.BlogUrlPath(sitePageSection.Key, sitePage.Key))
                               }
                           });
            }

            return breadcrumbList;
        }

        private IActionResult Show404Page()
        {
            this.Response.StatusCode = 404;

            return this.View("Page404");
        }

    }
}
