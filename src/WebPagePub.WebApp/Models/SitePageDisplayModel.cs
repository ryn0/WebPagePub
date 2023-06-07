using WebPagePub.Data.Enums;
using WebPagePub.Services.Interfaces;

namespace WebPagePub.Web.Models
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

        public SitePageCommentModel PostComment { get; set; } = new SitePageCommentModel() { RequestId = Guid.NewGuid() };

        public PreviousAndNextModel PreviousAndNext { get; set; } = new PreviousAndNextModel();

        public bool AllowCommenting { get; set; }

        public string SectionKey { get; set; }
        public bool IsHomePageSection { get; set; }
    }
}
