using Newtonsoft.Json;

namespace WebPagePub.Web.Models
{
    public class Author
    {
        [JsonProperty("@type")]
        public string @Type { get; set; } = "Person";

        [JsonProperty("name")]
        public string Name { get; set; } = default!;
    }
}