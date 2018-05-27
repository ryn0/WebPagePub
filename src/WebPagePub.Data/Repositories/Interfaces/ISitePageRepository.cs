using WebPagePub.Data.DbContextInfo;
using WebPagePub.Data.Models;
using System;
using System.Collections.Generic;

namespace WebPagePub.Data.Repositories.Interfaces
{
    public interface ISitePageRepository : IDisposable
    {
        IApplicationDbContext Context { get; }

        SitePage Create(SitePage model);

        bool Update(SitePage model);

        SitePage Get(int SitePageId);

        bool Delete(int SitePageId);

        SitePage Get(string key);

        SitePage GetPreviousEntry(DateTime currentSitePagePublishDateTimeUtc);

        SitePage GetNextEntry(DateTime currentSitePagePublishDateTimeUtc);

        List<SitePage> GetPage(int pageNumber, int sitePageSectionId, int quantityPerPage, out int total);

        List<SitePage> GetLivePage(int pageNumber, int quantityPerPage, out int total);

        List<SitePage> GetLivePageByTag(string tagKey, int pageNumber, int quantityPerPage, out int total);

        SitePage Get(int sitePageSectionId, string key);

        List<SitePage> GetLivePagesForSection(int sitePageSectionId);

        List<SitePage> GetLivePageBySection(int sectionId, int pageNumber, int quantityPerPage, out int total);
    }
}
