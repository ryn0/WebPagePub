using Newtonsoft.Json;
using WebPagePub.Services.Interfaces;

namespace WebPagePub.Web.Models
{
    public class ItemReviewed
    {
        [JsonProperty("@type")]
        public string @Type { get; set; } = "Thing";

        [JsonProperty("name")]
        public string Name { get; set; }
    }
}