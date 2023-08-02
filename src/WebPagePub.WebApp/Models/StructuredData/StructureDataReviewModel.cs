using Newtonsoft.Json;
using WebPagePub.Services.Interfaces;

namespace WebPagePub.WebApp.Models.StructuredData
{
    public class StructureDataReviewModel
    {
        private readonly ICacheService cacheService;

        public StructureDataReviewModel(ICacheService cacheService)
        {
            this.cacheService = cacheService;
        }

        [JsonProperty("@context")]
        public string? Context { get; set; } = "https://schema.org/";

        [JsonProperty("@type")]
        public string? Type { get; set; } = "Product";

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