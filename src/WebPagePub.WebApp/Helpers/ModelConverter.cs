using WebPagePub.Data.Enums;
using WebPagePub.Services.Interfaces;

namespace WebPagePub.Web.Helpers
{
    public class ModelConverter
    {
        private readonly ICacheService cacheService;

        public ModelConverter(ICacheService cacheService)
        {
            this.cacheService = cacheService;

            this.BlobPrefix = this.cacheService.GetSnippet(SiteConfigSetting.BlobPrefix);
            this.CdnPrefix = this.cacheService.GetSnippet(SiteConfigSetting.CdnPrefixWithProtocol);
        }

        public string BlobPrefix { get; private set; }
        public string CdnPrefix { get; private set; }
    }
}