using Newtonsoft.Json;

namespace WebPagePub.WebApp.Models.StructuredData
{
    public class Publisher
    {
        [JsonProperty("@type")]
        public string @Type { get; set; } = "Organization";

        [JsonProperty("name")]
        public string Name { get; set; } = default!;
    }
}