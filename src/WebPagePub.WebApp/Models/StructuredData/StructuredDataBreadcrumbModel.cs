using System.Collections.Generic;
using Newtonsoft.Json;
using WebPagePub.Web.Models;

namespace WebPagePub.WebApp.Models.StructuredData
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