using Newtonsoft.Json;
using WebPagePub.Data.Constants;
using WebPagePub.Data.Enums;
using WebPagePub.Services.Interfaces;
using System.Collections.Generic;

namespace WebPagePub.Web.Models
{
    public class StructedDataOrganizationModel
    {
        private readonly ICacheService _cacheService;

        public StructedDataOrganizationModel(ICacheService cacheService)
        {
            _cacheService = cacheService;
            SetProperties();
        }

        private void SetProperties()
        {
            Logo = _cacheService.GetSnippet(SiteConfigSetting.LogoUrl);

            SameAs = new[]
        {
           _cacheService.GetSnippet(SiteConfigSetting.FacebookUrl),
           _cacheService.GetSnippet(SiteConfigSetting.YouTubeUrl),
           _cacheService.GetSnippet(SiteConfigSetting.TwitterUrl)
        };

            Url = _cacheService.GetSnippet(SiteConfigSetting.CanonicalDomain);
            Name = _cacheService.GetSnippet(SiteConfigSetting.WebsiteName);
        }

        [JsonProperty("@context")]
        public string Context { get; set; } = "http://schema.org";

        [JsonProperty("@type")]
        public string @Type { get; set; } = "Organization";

        [JsonProperty("name")]
        public string Name { get; set; } 

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("logo")]
        public string Logo { get; set; } 

        [JsonProperty("sameAs")]
        public string[] SameAs { get; set; } 

    }
    
}
