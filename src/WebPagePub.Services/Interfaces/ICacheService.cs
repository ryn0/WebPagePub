using WebPagePub.Data.Enums;

namespace WebPagePub.Services.Interfaces
{
    public interface ICacheService
    {
        string GetSnippet(SiteConfigSetting snippetType);

        void ClearSnippetCache(SiteConfigSetting snippetType);
    }
}
