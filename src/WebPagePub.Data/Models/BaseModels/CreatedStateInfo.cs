using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebPagePub.Data.DbModels.BaseDbModels
{
    public class CreatedStateInfo
    {
        [Column(TypeName = "datetime2")]
        public DateTime CreateDate { get; set; }
    }
}
