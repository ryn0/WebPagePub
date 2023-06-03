using System.Collections.Generic;
using Newtonsoft.Json;

namespace WebPagePub.Web.Models
{
    public class StructuredDataBreadcrumbModel
    {
        [JsonProperty("@context")]
        public string Context { get; set; } = "http://schema.org";

        [JsonProperty("@type")]
        public string @Type { get; set; } = "BreadcrumbList";

        [JsonProperty("itemListElement")]
        public List<BreadcrumbListItem> ItemListElement { get; set; } = new List<BreadcrumbListItem>();
    }
}