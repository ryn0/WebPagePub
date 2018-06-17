using WebPagePub.Data.Constants;
using WebPagePub.Data.Enums;
using WebPagePub.Services.Interfaces;
using System;
using System.Collections.Generic;

namespace WebPagePub.Web.Models
{
    public class SitePageDisplayModel
    {
        private readonly ICacheService _cacheService;

        public SitePageDisplayModel(ICacheService cacheService)
        {
            _cacheService = cacheService;

            Organization = new StructedDataOrganizationModel(_cacheService);
            Review = new StructureDataReviewModel(_cacheService);
            Website = new StructedDataWebsiteModel(_cacheService);
        }

        public SitePageContentModel PageContent { get; set; } = new SitePageContentModel();

        public PageType PageType { get; set; }

        public StructedDataOrganizationModel Organization { get; set; }  

        public StructuredDataBreadcrumbModel BreadcrumbList { get; set; } = new StructuredDataBreadcrumbModel();

        public StructureDataReviewModel Review { get; set; } 

        public StructedDataWebsiteModel Website { get; set; } 

        public List<SitePageContentModel> Items { get; set; } = new List<SitePageContentModel>();

        public List<SitePageCommentModel> Comments { get; set; } = new List<SitePageCommentModel>();

        public SitePagePagingModel Paging { get; set; } = new SitePagePagingModel();

        public string TagKeyword { get; set; }

        public string TagKey { get; set; }

        public int SitePageId { get; set; }

        public SitePageCommentModel PostComment { get; set; } = new SitePageCommentModel() { RequestId = Guid.NewGuid()};

        public bool AllowCommenting { get; set; }
    }

    public class SitePageContentModel
    {
        private string _canonicalUrl = null;

        public string CanonicalUrl
        {
            get
            {
                return _canonicalUrl;
            }
            set
            {
                _canonicalUrl = value;

                if (!string.IsNullOrWhiteSpace(_canonicalUrl))
                {
                    _canonicalUrl = _canonicalUrl.TrimEnd('/');
                }
            }
        }

        public string PhotoUrl { get; set; }

        public int PhotoUrlHeight { get; set; }

        public int PhotoUrlWidth { get; set; }

        public string Title { get; set; }

        public string BreadcrumbName { get; set; }

        public string Key { get; set; }

        public string UrlPath { get; set; }

        public string Content { get; set; }

        public bool IsIndex { get; set; }

        public string MetaKeywords { get; set; }

        public string MetaDescription { get; set; }

        public DateTime PublishedDateTime { get; set; }

        public DateTime LastUpdatedDateTimeUtc { get; set; }

        public string FriendlyPublishDateDisplay
        {
            get
            {
                return FormatDate(PublishedDateTime);
            }
        }

        public string FriendlyLastUpdateDateDisplay
        {
            get
            {
                return FormatDate(LastUpdatedDateTimeUtc);
            }
        }

        private string FormatDate(DateTime date)
        {
            var dt = date;

            string suffix;

            switch (dt.Day)
            {
                case 1:
                case 21:
                case 31:
                    suffix = "st";
                    break;
                case 2:
                case 22:
                    suffix = "nd";
                    break;
                case 3:
                case 23:
                    suffix = "rd";
                    break;
                default:
                    suffix = "th";
                    break;
            }

            return string.Format("{0:MMMM} {1}{2}, {0:yyyy}", dt, dt.Day, suffix);
        }

        public string DefaultPhotoUrl { get; set; }

        public string DefaultPhotoCdnUrl { get; set; }

        public List<string> Tags { get; set; } = new List<string>();

        public List<SitePagePhotoModel> Photos { get; set; } = new List<SitePagePhotoModel>();

        public string DefaultPhotoThumbUrl { get; set; }

        public string DefaultPhotoThumbCdnUrl { get; set; }
        public string PreviousUrlPath { get; set; }
        public string NextUrlPath { get; set; }
        public string PreviousName { get; set; }
        public string NextName { get; set; }
        public string DefaultPreviousPhotoThumbCdnUrl { get; set; }
        public string DefaultNextPhotoThumbCdnUrl { get; set; }

        public string PageHeader { get; set; }
        public string SectionKey { get;  set; }
    }

    public class SitePagePagingModel
    {

        public int PageCount { get; set; }

        public int Total { get; set; }

        public int CurrentPageNumber { get; set; }

        public int QuantityPerPage { get; set; }
    }
   

}
