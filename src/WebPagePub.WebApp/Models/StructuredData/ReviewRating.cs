using Newtonsoft.Json;

namespace WebPagePub.WebApp.Models.StructuredData
{
    public class ReviewRating
    {
        [JsonProperty("@type")]
        public string @Type { get; set; } = "Rating";

        [JsonProperty("ratingValue")]
        public string RatingValue { get; set; } = default!;

        [JsonProperty("bestRating")]
        public string BestRating { get; set; } = default!;

        [JsonProperty("worstRating")]
        public string WorstRating { get; set; } = default!;

        [JsonIgnore]
        public string RatingPercentage { get; set; } = default!;
    }
}