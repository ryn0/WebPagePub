using WebPagePub.Data.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WebPagePub.Web.Models
{
    public class SitePageEditModel
    {
        [Required]
        public string Title { get; set; }

        [Required]
        [Display( Name = "Page Header (H1)")]
        public string PageHeader { get; set; }

        [Required]
        public string Key { get; set; }

        public string Content { get; set; }

        public DateTime PublishDateTimeUtc { get; set; } = DateTime.UtcNow;

        public int SitePageId { get;  set; }

        [Display(Name = "Is Live")]
        public bool IsLive { get;   set; }

        [Display(Name = "Exclude From XML")]
        public bool ExcludePageFromSiteMapXml { get; set; }

        public string LiveUrlPath { get; set; }

        public string PreviewUrlPath { get; set; }

        public List<SitePagePhotoModel> BlogPhotos { get; set; } = new List<SitePagePhotoModel>();

        public List<string> BlogTags { get; set; } = new List<string>();

        public string Tags { get; set; }

        public PageType PageType { get; set; }

        [StringLength(160)]
        public string ReviewItemName { get; set; }

        public double ReviewRatingValue { get; set; }

        public double ReviewWorstValue { get; set; }

        public double ReviewBestValue { get; set; }

        [StringLength(160)]
        public string MetaDescription { get; set; }
        public string BreadcrumbName { get;   set; }
        public string MetaKeywords { get;  set; }

        [Display(Name = "Allows Comments")]
        public bool AllowsComments { get; set; }

        public bool IsSectionHomePage { get; set; }
    }
}
