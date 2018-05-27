using Microsoft.AspNetCore.Mvc;
using WebPagePub.Web.Helpers;
using WebPagePub.Data.Repositories.Interfaces;
using WebPagePub.Web.Models;
using WebPagePub.Data.Models;
using System;
using WebPagePub.Data.Constants;
using WebPagePub.Data.Models.Db;
using WebPagePub.Data.Enums;
using System.Linq;
using Microsoft.Extensions.Caching.Memory;
using System.Collections.Generic;
using WebPagePub.Services.Interfaces;
using Microsoft.AspNetCore.Http;

namespace WebPagePub.Web.Controllers
{
    public class HomeController : Controller
    {
        const string PreviewKey = "preview";
        const int PageSize = 10;

        private IHttpContextAccessor _accessor;
        private readonly IBlockedIPRepository _blockedIPRepository;
        private readonly ISitePageCommentRepository _stePageCommentRepository;
        private readonly ISitePageRepository _sitePageRepository;
        private readonly ISitePageSectionRepository _sitePageSectionRepository;
        private readonly ITagRepository _tagRepository;
        private readonly IMemoryCache _memoryCache;
        private readonly ICacheService _cacheService;

        public HomeController(
            IHttpContextAccessor accessor,
            IBlockedIPRepository blockedIPRepository,
            ISitePageCommentRepository stePageCommentRepository,
            ISitePageRepository sitePageRepository,
            ISitePageSectionRepository sitePageSectionRepository,
            ITagRepository  tagRepository,
            IMemoryCache memoryCache,
            ICacheService cacheService)
        {
            _accessor = accessor;
            _blockedIPRepository = blockedIPRepository;
            _stePageCommentRepository = stePageCommentRepository;
            _sitePageRepository = sitePageRepository;
            _sitePageSectionRepository = sitePageSectionRepository;
            _tagRepository = tagRepository;
            _memoryCache = memoryCache;
            _cacheService = cacheService;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return CatchAllRequests(StringConstants.HomeSectionKey, StringConstants.HomeIndexPageKey);
        }

        [Route("{sectionKey}/page/{pageNumber}")]
        [HttpGet]
        public IActionResult SectionPage(string sectionKey, int pageNumber = 1)
        {
            return CatchAllRequests(sectionKey, StringConstants.HomeIndexPageKey, false, pageNumber);
        }

        [Route("tag/{tagName}/page/{pageNumber}")]
        [HttpGet]
        public IActionResult TagPage(string tagName, int pageNumber = 1)
        {
            return CatchAllRequests(
                        tagKey: tagName,
                        pageKey: StringConstants.HomeIndexPageKey,
                        isPreview: false,
                        pageNumber: pageNumber);
        }

        [Route("tag/{tagName}")]
        [HttpGet]
        public IActionResult TagPage(string tagName)
        {
            return CatchAllRequests(
                        tagKey: tagName,
                        pageKey: StringConstants.HomeIndexPageKey,
                        isPreview: false );
        }

        [Route("{sectionKey}/{pageKey}")]
        [HttpGet]
        public IActionResult Index(string sectionKey, string pageKey)
        {
            return CatchAllRequests(sectionKey, pageKey);
        }

        [Route("{sectionKey}")]
        [HttpGet]
        public IActionResult Index(string sectionKey)
        {
            return CatchAllRequests(sectionKey, StringConstants.HomeIndexPageKey);
        }

      
        [Route(PreviewKey + "/{sitePageId}")]
        [HttpGet]
        public IActionResult Preview(int sitePageId)
        {
            var dbModel = _sitePageRepository.Get(sitePageId);
            var siteSection = _sitePageSectionRepository.Get(dbModel.SitePageSectionId);
 
            return CatchAllRequests(siteSection.Key, dbModel.Key, true);
        }

        [HttpPost]
        public IActionResult Comment(SitePageCommentModel model)
        {
            var existingComment = _stePageCommentRepository.Get(model.RequestId);

            if (existingComment != null)
            {
                return View("Commented");
            }

            if (!ModelState.IsValid)
            {
                return View("CommentError");
            }

            var ipAddress = _accessor.HttpContext.Connection.RemoteIpAddress.ToString();

            if (_blockedIPRepository.IsBlockedIp(ipAddress))
            {
                return BadRequest();
            }

            _stePageCommentRepository.Create(new SitePageComment()
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

            return View("Commented");
        }

        private IActionResult CatchAllRequests(
                                string sectionKey = null, 
                                string pageKey = null, 
                                bool isPreview = false,
                                int pageNumber = 1,
                                string tagKey = null)
        {
            var cacheKey = CacheHelper.GetpPageCacheKey(sectionKey, pageKey, isPreview, pageNumber, tagKey);           
            var cachedPage = _memoryCache.Get(cacheKey);

            SitePageDisplayModel model;

            if (cachedPage == null)
            {
                if (string.IsNullOrWhiteSpace(tagKey))
                {
                    var siteSection = _sitePageSectionRepository.Get(sectionKey);
                    if (siteSection == null)
                        return null;

                    var dbModel = _sitePageRepository.Get(siteSection.SitePageSectionId, pageKey);

                    if (dbModel == null || (!isPreview && !dbModel.IsLive))
                    {
                        Response.StatusCode = 404;

                        return View("Page404");
                    }

                    switch (dbModel.PageType)
                    {
                        case PageType.PageList:
                            model = CreateDisplayListModel(siteSection, dbModel, tagKey, pageNumber);
                            break;
                        case PageType.Content:
                        case PageType.Review:
                        default:
                            model = CreateDisplayModel(siteSection, dbModel);
                            break;
                    }

                    model.SitePageId = dbModel.SitePageId;
                }
                else
                {
                    model = CreateDisplayListModel( tagKey: tagKey, pageNumber: pageNumber);
                }

                _memoryCache.Set(cacheKey, model, DateTime.UtcNow.AddMinutes(10));
            }
            else
            {
                model = (SitePageDisplayModel)cachedPage;
            }

            switch (model.PageType)
            {
                case PageType.PageList:
                    return View("SectionList", model);
                case PageType.Review:
                    return View("Review", model);
                case PageType.Content:
                default:
                    return View("Content", model);
            }
        }

        private SitePageDisplayModel CreateDisplayListModel(
            SitePageSection sitePageSection = null, 
            SitePage sitePage = null,
            string tagKey = null,
            int pageNumber = 1)
         {
            var displayModel = new SitePageDisplayModel(_cacheService);
            List<SitePage> pages;
            int total = 0;

            if (string.IsNullOrWhiteSpace(tagKey))
            {
                var contentModel = CreatePageContentModel(sitePageSection, sitePage);

                displayModel = new SitePageDisplayModel(_cacheService)
                {
                    BreadcrumbList = BuildBreadcrumbList(sitePageSection, sitePage),
                    PageType = sitePage.PageType,
                    Review = BuildReviewModel(sitePageSection, sitePage),
                    PageContent = contentModel
                };

                pages = _sitePageRepository.GetLivePageBySection(
                                                            sitePageSection.SitePageSectionId,
                                                            pageNumber,
                                                            PageSize,
                                                            out total);
            }
            else
            {
                displayModel.TagKey = tagKey;
                displayModel.TagKeyword = _tagRepository.Get(tagKey).Name;
                displayModel.PageContent.Title = $"{displayModel.TagKeyword} - Pages Tagged";
                displayModel.PageContent.PageHeader = displayModel.PageContent.Title;
                displayModel.PageContent.MetaDescription = $"Pages which are tagged {displayModel.TagKeyword}";
                displayModel.PageType = PageType.PageList;

                pages = _sitePageRepository.GetLivePageByTag(
                                                            tagKey,
                                                            pageNumber,
                                                            PageSize,
                                                            out total);
            }

            var pageCount = (double)total / PageSize;

            displayModel.Paging = new SitePagePagingModel()
            {
                CurrentPageNumber = pageNumber,
                PageCount = (int)Math.Ceiling(pageCount),
                QuantityPerPage = PageSize,
                Total = total
            };

            foreach (var page in pages)
            {
                var itemPageModel = CreatePageContentModel(page.SitePageSection, page);

                if (itemPageModel.IsIndex)
                    continue;

                displayModel.Items.Add(itemPageModel);
            }

            return displayModel;
        }

        private SitePageDisplayModel CreateDisplayModel(SitePageSection sitePageSection, SitePage sitePage)
        {
            var contentModel = CreatePageContentModel(sitePageSection, sitePage);
            var comments = BuildComments(sitePage);

            var displayModel = new SitePageDisplayModel(_cacheService)
            {
                BreadcrumbList = BuildBreadcrumbList(sitePageSection, sitePage),
                PageType = sitePage.PageType,
                Review = BuildReviewModel(sitePageSection, sitePage),
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
                return commentModel;

            var pageComments = _stePageCommentRepository.GetCommentsForPage(sitePage.SitePageId, CommentStatus.Approved);

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
            var blobPrefix = _cacheService.GetSnippet(SiteConfigSetting.BlobPrefix);
            var cdnPrefix = _cacheService.GetSnippet(SiteConfigSetting.CdnPrefixWithProtocol);

            var canonicalUrl = new Uri(UrlBuilder.GetCurrentDomain(HttpContext) +
                    UrlBuilder.BlogUrlPath(sitePageSection.Key, sitePage.Key));

            var defaultPhotoUrl = sitePage?.Photos.FirstOrDefault(x => x.IsDefault == true);
           
            var displayModel = new SitePageContentModel()
            {
                BreadcrumbName = sitePage.BreadcrumbName,
                Title = sitePage.Title,
                PageHeader = sitePage.PageHeader,
                MetaDescription = sitePage.MetaDescription,
                Content = sitePage.Content,
                LastUpdatedDateTimeUtc = sitePage.UpdateDate ?? sitePage.CreateDate,
                PublishedDateTime = sitePage.PublishDateTimeUtc,
                CanonicalUrl = canonicalUrl.ToString(),
                PhotoUrl = ConvertBlobToCdnUrl(blobPrefix, cdnPrefix, defaultPhotoUrl?.PhotoFullScreenUrl),
                PhotoUrlHeight = defaultPhotoUrl != null ? defaultPhotoUrl.PhotoFullScreenUrlHeight : 0,
                PhotoUrlWidth = defaultPhotoUrl != null ? defaultPhotoUrl.PhotoFullScreenUrlWidth : 0,
                MetaKeywords = sitePage.MetaKeywords,
                UrlPath = UrlBuilder.BlogUrlPath(sitePageSection.Key, sitePage.Key),
                Key = sitePage.Key,
                SectionKey = sitePageSection.Key,
                DefaultPhotoThumbCdnUrl = ConvertBlobToCdnUrl(blobPrefix, cdnPrefix, defaultPhotoUrl?.PhotoThumbUrl) 
            };

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

        private  string ConvertBlobToCdnUrl(string blobPrefix, string cdnPrefix, string blobUrl)
        {
            if (string.IsNullOrWhiteSpace(blobUrl))
                return null;

            return blobUrl.Replace(blobPrefix, cdnPrefix);
        }

        private StructureDataReviewModel BuildReviewModel(SitePageSection sitePageSection, SitePage sitePage)
        {
            if (sitePage.PageType != PageType.Review)
                return null;

            var ratingPercentage = (sitePage.ReviewRatingValue / (sitePage.ReviewBestValue - sitePage.ReviewWorstValue)) * 100;

            return new StructureDataReviewModel(_cacheService)
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
                //Author = new Author()
                //{
                //    Name = StringConstants.WebsiteAuthor //TODO: GET FROM SITEPAGE USER
                //},
                Publisher = new Publisher()
                {
                    Name = _cacheService.GetSnippet(SiteConfigSetting.WebsiteName)
                }
            };
        }

        private StructuredDataBreadcrumbModel BuildBreadcrumbList(SitePageSection sitePageSection, SitePage sitePage)
        {
            var domain = UrlBuilder.GetCurrentDomain(HttpContext);

            var cacheKey = CacheHelper.GetpPageCacheKey(sitePageSection);
            SitePageSection homeSection;
            var cachedValue = _memoryCache.Get(cacheKey);

            if (cachedValue != null)
            {
                homeSection = (SitePageSection)cachedValue;
            }
            else
            {
                homeSection = _sitePageSectionRepository.Get(StringConstants.HomeSectionKey);
                _memoryCache.Set(cacheKey, homeSection, DateTime.UtcNow.AddMinutes(10));
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
                                     Name =  homeSection.BreadcrumbName,
                                     PageUrl = new Uri(domain)
                                }
                           }
                }
            };


            if (sitePageSection.Key != StringConstants.HomeSectionKey)
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
             
            if (sitePage.Key != StringConstants.HomeIndexPageKey)
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




    }
}
