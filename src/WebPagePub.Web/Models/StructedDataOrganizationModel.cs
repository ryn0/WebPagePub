using Newtonsoft.Json;
using WebPagePub.Data.Enums;
using WebPagePub.Services.Interfaces;

namespace WebPagePub.Web.Models
{
    public class StructedDataOrganizationModel
    {
        private readonly ICacheService cacheService;

        public StructedDataOrganizationModel(ICacheService cacheService)
        {
            this.cacheService = cacheService;
            this.SetProperties();
        }

        private void SetProperties()
        {
            this.Logo = this.cacheService.GetSnippet(SiteConfigSetting.LogoUrl);

            this.SameAs = new[]
            {
               this.cacheService.GetSnippet(SiteConfigSetting.FacebookUrl),
               this.cacheService.GetSnippet(SiteConfigSetting.YouTubeUrl),
               this.cacheService.GetSnippet(SiteConfigSetting.TwitterUrl)
            };

            this.Url = this.cacheService.GetSnippet(SiteConfigSetting.CanonicalDomain);
            this.Name = this.cacheService.GetSnippet(SiteConfigSetting.WebsiteName);
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