using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebPagePub.Data.DbModels.BaseDbModels
{
    public class StateInfo : CreatedStateInfo
    {
        public DateTime? UpdateDate { get; set; }
    }
}
