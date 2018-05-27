using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebPagePub.Data.DbModels.BaseDbModels
{
    public class ApplicationUserStateInfo : IdentityUser
    {
        [Column(TypeName = "datetime2")]
        public DateTime CreateDate { get; set; }

        [Column(TypeName = "datetime2")]
        public DateTime? UpdateDate { get; set; }
    }
}
