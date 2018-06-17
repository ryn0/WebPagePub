using Newtonsoft.Json;
using WebPagePub.Services.Interfaces;

namespace WebPagePub.Web.Models
{

    public class Author
    {
        [JsonProperty("@type")]
        public string @Type { get; set; } = "Person";

        [JsonProperty("name")]
        public string Name { get; set; }
    }
}