using Newtonsoft.Json;

namespace WebPagePub.Web.Models
{
    public class BreadcrumbListItem
    {
        [JsonProperty("@type")]
        public string @Type { get; set; } = "ListItem";

        [JsonProperty("position")]
        public int Position { get; set; }

        [JsonProperty("item")]
        public BreadcrumbListItemProperties Item { get; set; } = new BreadcrumbListItemProperties();
    }
}