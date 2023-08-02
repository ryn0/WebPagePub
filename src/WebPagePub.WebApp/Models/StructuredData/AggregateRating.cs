using Newtonsoft.Json;

namespace WebPagePub.WebApp.Models.StructuredData
{
    public class AggregateRating
    {
        [JsonProperty("@type")]
        public string? Type { get; set; }

        [JsonProperty("ratingValue")]
        public double RatingValue { get; set; }

        [JsonProperty("reviewCount")]
        public int ReviewCount { get; set; }
    }
}
