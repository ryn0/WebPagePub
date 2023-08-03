using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using WebPagePub.Data.Enums;

namespace WebPagePub.Web.Models
{
    public class SitePageEditModel
    {
        public int SitePageId { get; set; }

        [Required]
        public string Title { get; set; } = default!;

        [Required]
        [Display(Name = "Page Header (H1)")]
        public string PageHeader { get; set; } = default!;

        [Required]
        public string Key { get; set; } = default!;

        public string Content { get; set; } = default!;

        public DateTime PublishDateTimeUtc { get; set; } = DateTime.UtcNow;

        [Display(Name = "Is Live")]
        public bool IsLive { get;   set; }

        [Display(Name = "Exclude From XML")]
        public bool ExcludePageFromSiteMapXml { get; set; }

        public string LiveUrlPath { get; set; } = default!;

        public string PreviewUrlPath { get; set; } = default!;

        public List<SitePagePhotoModel> BlogPhotos { get; set; } = new List<SitePagePhotoModel>();

        public List<string> BlogTags { get; set; } = new List<string>();

        public string Tags { get; set; } = default!;

        public PageType PageType { get; set; }

        [StringLength(160)]
        public string ReviewItemName { get; set; } = default!;

        public double ReviewRatingValue { get; set; }

        public double ReviewWorstValue { get; set; }

        public double ReviewBestValue { get; set; }

        [StringLength(160)]
        public string MetaDescription { get; set; } = default!;
        public string BreadcrumbName { get;   set; } = default!;
        public string MetaKeywords { get;  set; } = default!;

        [Display(Name = "Allows Comments")]
        public bool AllowsComments { get; set; }

        public bool IsSectionHomePage { get; set; }

        [Display(Name = "Author")]
        public int? AuthorId { get; set; }

        public List<SelectListItem> Authors = new List<SelectListItem>();

    }
}
