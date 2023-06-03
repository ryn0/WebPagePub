using Newtonsoft.Json;
using WebPagePub.Services.Interfaces;

namespace WebPagePub.Web.Models
{
    public class StructureDataReviewModel
    {
        private readonly ICacheService cacheService;

        public StructureDataReviewModel(ICacheService cacheService)
        {
            this.cacheService = cacheService;

            this.Author = new StructedDataOrganizationModel(this.cacheService);
        }

        [JsonProperty("@context")]
        public string Context { get; set; } = "http://schema.org";

        [JsonProperty("@type")]
        public string @Type { get; set; } = "Review";

        [JsonProperty("itemReviewed")]
        public ItemReviewed ItemReviewed { get; set; } = new ItemReviewed();

        [JsonProperty("author")]
        public StructedDataOrganizationModel Author { get; set; }

        [JsonProperty("reviewRating")]
        public ReviewRating ReviewRating { get; set; } = new ReviewRating();

        [JsonProperty("publisher")]
        public Publisher Publisher { get; set; } = new Publisher();
    }
}