using System;
using System.ComponentModel.DataAnnotations;
using WebPagePub.Data.Enums;
using WebPagePub.Data.Models.BaseModels;

namespace WebPagePub.Data.Models
{
    public class SitePageAudit : UserStateInfo
    {
        public int SitePageId { get; set; }

        [Required]
        [StringLength(80)]
        public string Title { get; set; }

        [Required]
        [StringLength(255)]
        public string PageHeader { get; set; }

        [Required]
        [StringLength(255)]
        public string Key { get; set; }

        [Required]
        [StringLength(255)]
        public string BreadcrumbName { get; set; }

        [StringLength(255)]
        public string MetaKeywords { get; set; }

        public bool ExcludePageFromSiteMapXml { get; set; }

        public string Content { get; set; }

        public bool IsLive { get; set; }

        public DateTime PublishDateTimeUtc { get; set; }

        [StringLength(160)]
        public string MetaDescription { get; set; }

        public bool AllowsComments { get; set; }

        public int SitePageSectionId { get; set; }

        public PageType PageType { get; set; }

        [StringLength(160)]
        public string ReviewItemName { get; set; }

        public double ReviewRatingValue { get; set; }

        public double ReviewWorstValue { get; set; }

        public double ReviewBestValue { get; set; }

        public int? AuthorId { get; set; }

        [Required]
        public bool IsSectionHomePage { get; set; }
    }
}
