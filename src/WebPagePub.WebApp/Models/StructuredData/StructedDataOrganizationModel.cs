using Newtonsoft.Json;
using WebPagePub.Data.Enums;
using WebPagePub.Services.Interfaces;

namespace WebPagePub.WebApp.Models.StructuredData
{
    public class StructedDataOrganizationModel
    {
        private readonly ICacheService cacheService;

        public StructedDataOrganizationModel(ICacheService cacheService)
        {
            this.cacheService = cacheService;
            SetProperties();
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

        private void SetProperties()
        {
            Logo = cacheService.GetSnippet(SiteConfigSetting.LogoUrl);

            SameAs = new[]
            {
               cacheService.GetSnippet(SiteConfigSetting.FacebookUrl),
               cacheService.GetSnippet(SiteConfigSetting.YouTubeUrl),
               cacheService.GetSnippet(SiteConfigSetting.TwitterUrl)
            };

            Url = cacheService.GetSnippet(SiteConfigSetting.CanonicalDomain);
            Name = cacheService.GetSnippet(SiteConfigSetting.WebsiteName);
        }
    }
}