using WebPagePub.Data.Enums;
using WebPagePub.Data.Models.BaseModels;
using WebPagePub.Data.Models.Db;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebPagePub.Data.Models
{
    public class SitePage : UserStateInfo
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
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

        // should be: 140 and 170 characters
        [StringLength(160)]
        public string MetaDescription { get; set; }

        public virtual ICollection<SitePageTag> SitePageTags { get; set; }

        public virtual List<SitePagePhoto> Photos { get; set; } = new List<SitePagePhoto>();

        public virtual List<SitePageComment> Comments { get; set; } = new List<SitePageComment>();

        public bool AllowsComments { get; set; }

        [ForeignKey(nameof(SitePageSection))]
        public int SitePageSectionId { get; set; }

        public PageType PageType { get; set; }

        [StringLength(160)]
        public string ReviewItemName { get; set; }

        public double ReviewRatingValue { get; set; }

        public double ReviewWorstValue { get; set; }

        public double ReviewBestValue { get; set; }

        public SitePageSection SitePageSection { get; set; }

        [Required]
        public bool IsSectionHomePage { get; set; }
    }
}
