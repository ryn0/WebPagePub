using System;
using Newtonsoft.Json;

namespace WebPagePub.Web.Models
{
    public class BreadcrumbListItemProperties
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("@id")]
        public Uri PageUrl { get; set; }

        [JsonProperty("image", NullValueHandling = NullValueHandling.Ignore)]
        public Uri ImageUrl { get; set; }
    }
}