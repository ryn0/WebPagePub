using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using WebPagePub.Data.Enums;

namespace WebPagePub.Managers.Models.SitePages
{
    public class SitePageEditModel
    {
        public int SitePageId { get; set; }

        [Display(Name = "Section")]
        public int SitePageSectionId { get; set; }

        [Display(Name = "Title")]
        [Required]
        public string Title { get; set; } = default!;

        [Required]
        [Display(Name = "Page Header (H1)")]
        public string PageHeader { get; set; } = default!;

        [Display(Name = "Key")]
        [Required]
        public string Key { get; set; } = default!;

        [Display(Name = "Content")]
        public string Content { get; set; } = default!;

        [Display(Name = "Publish Date Time UTC")]
        public DateTime PublishDateTimeUtc { get; set; } = DateTime.UtcNow;

        [Display(Name = "Is Live")]
        public bool IsLive { get; set; }

        [Display(Name = "Exclude From Indexing")]
        public bool ExcludePageFromSiteMapXml { get; set; }

        [Display(Name = "Live URL Path")]
        public string LiveUrlPath { get; set; } = default!;

        [Display(Name = "Preview URL Path")]
        public string PreviewUrlPath { get; set; } = default!;

        public List<SitePagePhotoModel> BlogPhotos { get; set; } = new List<SitePagePhotoModel>();

        public List<string> BlogTags { get; set; } = new List<string>();

        [Display(Name = "Tags")]
        public string Tags { get; set; } = default!;

        [Display(Name = "Page Type")]
        public PageType PageType { get; set; }

        [Display(Name = "Review Item Name")]
        [StringLength(160)]
        public string ReviewItemName { get; set; } = default!;

        [Display(Name = "Review Rating Value")]
        public double ReviewRatingValue { get; set; }

        [Display(Name = "Review Worst Value")]
        public double ReviewWorstValue { get; set; } = 0;

        [Display(Name = "Review Best Value")]
        public double ReviewBestValue { get; set; } = 5.0;

        [Display(Name = "Meta Description")]
        [StringLength(160)]
        public string MetaDescription { get; set; } = default!;

        [Display(Name = "Breadcrumb Name")]
        public string BreadcrumbName { get; set; } = default!;

        [Display(Name = "Meta Keywords")]
        public string MetaKeywords { get; set; } = default!;

        [Display(Name = "Allows Comments")]
        public bool AllowsComments { get; set; }

        [Display(Name = "Is Section HomePage")]
        public bool IsSectionHomePage { get; set; }

        [Display(Name = "Author")]
        public int? AuthorId { get; set; }
    }
}
