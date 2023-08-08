using System;
using System.Collections.Generic;
using WebPagePub.Data.DbContextInfo;
using WebPagePub.Data.Models;

namespace WebPagePub.Data.Repositories.Interfaces
{
    public interface ISitePageRepository : IDisposable
    {
        IApplicationDbContext Context { get; }

        SitePage Create(SitePage model);

        bool Update(SitePage model);

        SitePage Get(int sitePageId);

        bool Delete(int sitePageId);

        SitePage Get(string key);

        SitePage GetPreviousEntry(DateTime currentSitePagePublishDateTimeUtc, DateTime now, int sitePageSectionId);

        SitePage GetNextEntry(DateTime currentSitePagePublishDateTimeUtc, DateTime now, int sitePageSectionId);

        List<SitePage> GetPage(int pageNumber, int sitePageSectionId, int quantityPerPage, out int total);

        List<SitePage> GetLivePage(int pageNumber, int quantityPerPage, out int total);

        List<SitePage> GetLivePageByTag(string tagKey, int pageNumber, int quantityPerPage, out int total);

        SitePage Get(int sitePageSectionId, string key);

        List<SitePage> GetLivePagesForSection(int sitePageSectionId);

        List<SitePage> GetLivePageBySection(int sitePageSectionId, int pageNumber, int quantityPerPage, out int total);

        SitePage GetSectionHomePage(int sitePageSectionId);

        List<SitePage> GetIgnoredPages();
    }
}
