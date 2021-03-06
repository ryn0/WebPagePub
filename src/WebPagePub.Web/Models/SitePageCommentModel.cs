﻿using System;
using System.ComponentModel.DataAnnotations;
using WebPagePub.Data.Enums;

namespace WebPagePub.Web.Models
{
    public class SitePageCommentModel
    {
        public int SitePageCommentId { get; set; }

        public int SitePageId { get; set; }

        public DateTime CreateDate { get; set; }

        [Display(Name = "Email")]
        [EmailAddress]
        [Required]
        public string Email { get; set; }

        [Display(Name = "Website (optional)")]
        [Url]
        public string Website { get; set; }

        [Display(Name = "Name")]
        [Required]
        public string Name { get; set; }

        [Display(Name = "Comment")]
        [Required]
        public string Comment { get; set; }

        public CommentStatus CommentStatus { get; set; }

        public Guid RequestId { get; set; }

        public int Number1 { get; set; }

        public int Number2 { get; set; }

        public int SumOf2Numbers { get; set; }
    }
}
