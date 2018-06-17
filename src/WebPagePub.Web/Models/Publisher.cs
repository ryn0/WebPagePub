using Newtonsoft.Json;
using WebPagePub.Services.Interfaces;

namespace WebPagePub.Web.Models
{

    public class Publisher
    {
        [JsonProperty("@type")]
        public string @Type { get; set; } = "Organization";

        [JsonProperty("name")]
        public string Name { get; set; }
    }
}