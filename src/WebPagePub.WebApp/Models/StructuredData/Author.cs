using Newtonsoft.Json;

namespace WebPagePub.WebApp.Models.StructuredData
{
    public class Author
    {
        [JsonProperty("@type")]
        public string @Type { get; set; } = "Person";

        [JsonProperty("name")]
        public string Name { get; set; } = default!;
    }
}