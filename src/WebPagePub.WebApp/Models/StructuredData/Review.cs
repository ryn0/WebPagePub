using Newtonsoft.Json;

namespace WebPagePub.WebApp.Models.StructuredData
{
    public class Review
    {
        [JsonProperty("@type")]
        public string? Type { get; set; }

        [JsonProperty("reviewRating")]
        public ReviewRating? ReviewRating { get; set; }

        [JsonProperty("author")]
        public Author? Author { get; set; }
    }
}
