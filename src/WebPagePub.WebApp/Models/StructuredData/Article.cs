using Newtonsoft.Json;

namespace WebPagePub.WebApp.Models.StructuredData
{
    public class Article
    {
        [JsonProperty("@context")]
        public string Context { get; set; } = "https://schema.org/";

        [JsonProperty("@type")]
        public string Type { get; set; } = "Article";

        [JsonProperty("headline")]
        public string Headline { get; set; } = string.Empty;

        [JsonProperty("image")]
        public Uri[]? Image { get; set; }

        [JsonProperty("datePublished")]
        public DateTimeOffset DatePublished { get; set; }

        [JsonProperty("dateModified")]
        public DateTimeOffset DateModified { get; set; }

        [JsonProperty("author")]
        public Author[]? Author { get; set; }
    }
}
