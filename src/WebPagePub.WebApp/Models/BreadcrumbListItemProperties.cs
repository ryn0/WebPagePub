using Newtonsoft.Json;

namespace WebPagePub.Web.Models
{
    public class BreadcrumbListItemProperties
    {
        [JsonProperty("name")]
        public string Name { get; set; } = default!;

        [JsonProperty("@id")]
        public Uri PageUrl { get; set; } = default!;

        [JsonProperty("image", NullValueHandling = NullValueHandling.Ignore)]
        public Uri ImageUrl { get; set; } = default!;
    }
}