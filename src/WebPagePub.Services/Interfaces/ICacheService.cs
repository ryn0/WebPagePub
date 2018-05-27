using WebPagePub.Data.Enums;
using WebPagePub.Services.Models;

namespace WebPagePub.Services.Interfaces
{

    public interface ICacheService
    {
        string GetSnippet(SiteConfigSetting snippetType);

        void ClearSnippetCache(SiteConfigSetting snippetType);
    }
}
