using System;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace WebPagePub.Data.DbModels.BaseDbModels
{
    public class ApplicationUserStateInfo : IdentityUser
    {
        public DateTime CreateDate { get; set; }

        public DateTime? UpdateDate { get; set; }
    }
}
