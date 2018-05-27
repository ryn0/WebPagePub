using Newtonsoft.Json;
using System;
using System.Collections.Generic;

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

    public class BreadcrumbListItem
    {
        [JsonProperty("@type")]
        public string @Type { get; set; } = "ListItem";

        [JsonProperty("position")]
        public int Position { get; set; }

        [JsonProperty("item")]
        public BreadcrumbListItemProperties Item { get; set; } = new BreadcrumbListItemProperties();
    }

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
