using Newtonsoft.Json;
using OpenAI_API.Images;

namespace WebPagePub.PageManager.Console.Models.ChatModels
{
    // serves as our input model
    [JsonObject("input")]
    public class ImageInputRequest
    {
        public string? Prompt { get; set; }

        public short? Quanaity { get; set; } = 1;

        public ImageSize ImageSize { get; set; } = ImageSize._1792x1024;

        public string Quality = "standard";
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

        public bool Success { get; set; } = false;
    }
}
