using System.ComponentModel.DataAnnotations.Schema;
using WebPagePub.Data.DbModels.BaseDbModels;
using WebPagePub.Data.Enums;

namespace WebPagePub.Data.Models.Db
{
    public class ContentSnippet : StateInfo
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ContentSnippetId { get; set; }

        public SiteConfigSetting SnippetType { get; set; }

        public string Content { get; set; }
    }
}
