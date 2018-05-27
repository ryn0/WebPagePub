using Newtonsoft.Json;
using WebPagePub.Services.Interfaces;

namespace WebPagePub.Web.Models
{
    public class StructureDataReviewModel
    {
        private readonly ICacheService _cacheService;

        public StructureDataReviewModel(ICacheService cacheService)
        {
            _cacheService = cacheService;
         
            Author = new StructedDataOrganizationModel(_cacheService); 
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

    public class ItemReviewed
    {
        [JsonProperty("@type")]
        public string @Type { get; set; } = "Thing";

        [JsonProperty("name")]
        public string Name { get; set; }
    }

    public class Author
    {
        [JsonProperty("@type")]
        public string @Type { get; set; } = "Person";

        [JsonProperty("name")]
        public string Name { get; set; }
    }

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

    public class Publisher
    {
        [JsonProperty("@type")]
        public string @Type { get; set; } = "Organization";

        [JsonProperty("name")]
        public string Name { get; set; }
    }
}