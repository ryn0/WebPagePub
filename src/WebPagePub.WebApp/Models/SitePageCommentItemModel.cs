using WebPagePub.Data.Enums;

namespace WebPagePub.Web.Models
{
    public class SitePageCommentItemModel
    {
        public int SitePageCommentId { get; set; }

        public DateTime CreateDate { get; set; }

        public string Name { get; set; } = default!;

        public CommentStatus CommentStatus { get;   set; }
    }
}
