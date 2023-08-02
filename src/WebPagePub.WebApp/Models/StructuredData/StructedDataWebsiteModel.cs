using Newtonsoft.Json;
using WebPagePub.Data.Enums;
using WebPagePub.Services.Interfaces;

namespace WebPagePub.WebApp.Models.StructuredData
{
    public class StructedDataWebsiteModel
    {
        private readonly ICacheService cacheService;

        public StructedDataWebsiteModel(ICacheService cacheService)
        {
            this.cacheService = cacheService;
            Url = this.cacheService.GetSnippet(SiteConfigSetting.CanonicalDomain);
            Name = this.cacheService.GetSnippet(SiteConfigSetting.WebsiteName);
        }

        [JsonProperty("@context")]
        public string Context { get; set; } = "http://schema.org";

        [JsonProperty("@type")]
        public string @Type { get; set; } = "WebSite";

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }
    }
}