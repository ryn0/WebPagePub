﻿using WebPagePub.Data.DbModels.BaseDbModels;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebPagePub.Data.Models.Db
{
    public class SitePageSection : StateInfo
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int SitePageSectionId { get; set; }

        [Required]
        [StringLength(255)]
        public string Title { get; set; }

        [Required]
        [StringLength(255)]
        public string Key { get; set; }

        [Required]
        [StringLength(255)]
        public string BreadcrumbName { get; set; }
    }
}
