using Newtonsoft.Json;

namespace WebPagePub.WebApp.Models.StructuredData
{
    public class ItemReviewed
    {
        [JsonProperty("@type")]
        public string @Type { get; set; } = "Product";

        [JsonProperty("name")]
        public string Name { get; set; } = default!;
    }
}