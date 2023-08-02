using Newtonsoft.Json;

namespace WebPagePub.WebApp.Models.StructuredData
{
    public class ReviewRatingModel
    {
        [JsonProperty("@type")]
        public string? Type { get; set; }

        [JsonProperty("ratingValue")]
        public int RatingValue { get; set; }

        [JsonProperty("bestRating")]
        public int BestRating { get; set; }
    }
}
