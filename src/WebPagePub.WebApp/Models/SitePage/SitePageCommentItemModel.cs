using WebPagePub.Data.Enums;

namespace WebPagePub.WebApp.Models.SitePage
{
    public class SitePageCommentItemModel
    {
        public int SitePageCommentId { get; set; }

        public DateTime CreateDate { get; set; }

        public string Name { get; set; } = default!;

        public string? Comment { get; set; }

        public string? Website { get; set; }

        public CommentStatus CommentStatus { get; set; }
    }
}
