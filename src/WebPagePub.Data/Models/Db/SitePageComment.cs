using WebPagePub.Data.DbModels.BaseDbModels;
using WebPagePub.Data.Enums;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebPagePub.Data.Models.Db
{
    public class SitePageComment : CreatedStateInfo
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int SitePageCommentId { get; set; }

        [ForeignKey(nameof(SitePage))]
        public int SitePageId { get; set; }

        public virtual SitePage SitePage { get; set; }

        public Guid RequestId { get; set; }

        [StringLength(75)]
        public string Email { get; set; }

        [StringLength(255)]
        public string WebSite { get; set; }

        [StringLength(75)]
        public string IpAddress { get; set; }

        [StringLength(75)]
        public string Name { get; set; }

        public string Comment { get; set; }

        public CommentStatus CommentStatus { get; set; }
    }
}
