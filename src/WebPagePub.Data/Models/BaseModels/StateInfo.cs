using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebPagePub.Data.DbModels.BaseDbModels
{
    public class StateInfo : CreatedStateInfo
    {
        [Column(TypeName = "datetime2")]
        public DateTime? UpdateDate { get; set; }
    }
}
