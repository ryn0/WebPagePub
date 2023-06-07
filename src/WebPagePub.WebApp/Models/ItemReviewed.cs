using Newtonsoft.Json;

namespace WebPagePub.Web.Models
{
    public class ItemReviewed
    {
        [JsonProperty("@type")]
        public string @Type { get; set; } = "Thing";

        [JsonProperty("name")]
        public string Name { get; set; } = default!;
    }
}