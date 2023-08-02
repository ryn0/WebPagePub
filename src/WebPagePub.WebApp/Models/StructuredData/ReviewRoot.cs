using Newtonsoft.Json;

namespace WebPagePub.WebApp.Models.StructuredData
{
    public class ReviewRoot
    {
        [JsonProperty("@context")]
        public string? Context { get; set; }

        [JsonProperty("@type")]
        public string? Type { get; set; }

        [JsonProperty("name")]
        public string? Name { get; set; }

        [JsonProperty("description")]
        public string? Description { get; set; }

        [JsonProperty("review")]
        public Review? Review { get; set; }

        [JsonProperty("aggregateRating")]
        public AggregateRating? AggregateRating { get; set; }
    }
}
