using Newtonsoft.Json;
using WebPagePub.Data.Constants;
using WebPagePub.Data.Enums;
using WebPagePub.Services.Interfaces;

namespace WebPagePub.Web.Models
{
    public class StructedDataWebsiteModel
    {
        private readonly ICacheService _cacheService;

        public StructedDataWebsiteModel(ICacheService cacheService)
        {
            _cacheService = cacheService;

            Url = _cacheService.GetSnippet(SiteConfigSetting.CanonicalDomain);

            Name = _cacheService.GetSnippet(SiteConfigSetting.WebsiteName);
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
