using Newtonsoft.Json;

namespace WebPagePub.WebApp.Models.StructuredData
{
    public class Review
    {
        [JsonProperty("@type")]
        public string? Type { get; set; } = "Review";

        [JsonProperty("reviewRating")]
        public ReviewRating? ReviewRating { get; set; } = new ReviewRating();

        [JsonProperty("author")]
        public Author? Author { get; set; }
    }
}
