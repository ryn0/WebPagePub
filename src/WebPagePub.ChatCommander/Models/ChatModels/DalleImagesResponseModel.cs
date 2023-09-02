using Newtonsoft.Json;

namespace WebPagePub.ChatCommander.Models.ChatModels
{
    // serves as our input model
    [JsonObject("input")]
    public class ImageInputRequest
    {
        [JsonProperty("prompt")]
        public string? Prompt { get; set; }

        [JsonProperty("n")]
        public short? Quanaity { get; set; } = 1;

        [JsonProperty("size")]
        public string? ImageSize { get; set; } = "1024x1024";
    }

    // model for the image url
    [JsonObject("Link")]
    public class Link
    {
        [JsonProperty("url")]
        public string? Url { get; set; }
    }

    // model for the DALL E api response
    public class DalleImagesResponseModel
    {
        [JsonProperty("created")]
        public long Created { get; set; }

        [JsonProperty("data")]
        public List<Link>? Data { get; set; }
    }
}
