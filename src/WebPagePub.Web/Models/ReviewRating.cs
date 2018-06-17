using Newtonsoft.Json;
using WebPagePub.Services.Interfaces;

namespace WebPagePub.Web.Models
{

    public class ReviewRating
    {
        [JsonProperty("@type")]
        public string @Type { get; set; } = "Rating";

        [JsonProperty("ratingValue")]
        public string RatingValue { get; set; }

        [JsonProperty("bestRating")]
        public string BestRating { get; set; }

        [JsonProperty("worstRating")]
        public string WorstRating { get; set; }

        [JsonIgnore]
        public string RatingPercentage { get; set; }
    }
}