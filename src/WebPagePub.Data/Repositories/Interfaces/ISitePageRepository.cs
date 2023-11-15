using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WebPagePub.Data.DbContextInfo.Interfaces;
using WebPagePub.Data.Models;

namespace WebPagePub.Data.Repositories.Interfaces
{
    public interface ISitePageRepository : IDisposable
    {
        IApplicationDbContext Context { get; }

        Task<SitePage> CreateAsync(SitePage model);

        Task<bool> UpdateAsync(SitePage model);

        SitePage Get(int sitePageId);

        bool Delete(int sitePageId);

        SitePage Get(string key);

        SitePage GetPreviousEntry(DateTime currentSitePagePublishDateTimeUtc, DateTime now, int sitePageSectionId);

        SitePage GetNextEntry(DateTime currentSitePagePublishDateTimeUtc, DateTime now, int sitePageSectionId);

        IList<SitePage> GetPage(int pageNumber, int sitePageSectionId, int quantityPerPage, out int total);

        IList<SitePage> GetLivePage(int pageNumber, int quantityPerPage, out int total);

        IList<SitePage> GetLivePageByTag(string tagKey, int pageNumber, int quantityPerPage, out int total);

        SitePage Get(int sitePageSectionId, string key);

        IList<SitePage> GetLivePagesForSection(int sitePageSectionId);

        IList<SitePage> GetLivePageBySection(int sitePageSectionId, int pageNumber, int quantityPerPage, out int total);

        SitePage GetSectionHomePage(int sitePageSectionId);

        IList<SitePage> GetIgnoredPages();

        SitePage GetPreviouslyCreatedEntry(DateTime createDate, int sitePageId, int sitePageSectionId);

        SitePage GetNextCreatedEntry(DateTime createDate, int sitePageId, int sitePageSectionId);

        IList<SitePage> SearchForTerm(string term, int pageNumber, int quantityPerPage, out int total);
    }
}