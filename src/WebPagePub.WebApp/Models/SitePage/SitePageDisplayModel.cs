using Schema.NET;
using WebPagePub.Data.Enums;
using WebPagePub.Services.Interfaces;
using WebPagePub.Web.Models;
using WebPagePub.WebApp.Models.StructuredData;

namespace WebPagePub.WebApp.Models.SitePage
{
    public class SitePageDisplayModel
    {
        private readonly ICacheService cacheService;

        public SitePageDisplayModel(ICacheService cacheService)
        {
            this.cacheService = cacheService;

            this.Organization = new StructedDataOrganizationModel(this.cacheService);
            this.Review = new StructureDataReviewModel(this.cacheService);
            this.Website = new StructedDataWebsiteModel(this.cacheService);
            this.Article = new Article();
        }

        public SitePageContentModel PageContent { get; set; } = new SitePageContentModel();

        public PageType PageType { get; set; }

        public StructedDataOrganizationModel Organization { get; set; }

        public StructuredDataBreadcrumbModel BreadcrumbList { get; set; } = new StructuredDataBreadcrumbModel();

        public StructureDataReviewModel Review { get; set; }

        public Article Article { get; set; }

        public StructedDataWebsiteModel Website { get; set; }

        public List<SitePageContentModel> Items { get; set; } = new List<SitePageContentModel>();

        public List<SitePageCommentDisplayModel> Comments { get; set; } = new List<SitePageCommentDisplayModel>();

        public SitePagePagingModel Paging { get; set; } = new SitePagePagingModel();

        public string TagKeyword { get; set; } = string.Empty;

        public string TagKey { get; set; } = string.Empty;

        public int SitePageId { get; set; }

        public SitePageCommentModel PostComment { get; set; } = new SitePageCommentModel() { RequestId = Guid.NewGuid() };

        public PreviousAndNextModel PreviousAndNext { get; set; } = new PreviousAndNextModel();

        public bool AllowCommenting { get; set; }
        public bool IsLive { get; set; }
        public string SectionKey { get; set; } = string.Empty;
        public bool IsHomePageSection { get; set; }
        public bool IsSectionHomePage { get; set; }
        public string AuthorName { get; set; } = string.Empty;
        public string ArticleSchema { get; set; } = string.Empty;
    }
}