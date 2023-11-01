using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using WebPagePub.Data.Models;
using WebPagePub.Data.Models.Db;
using WebPagePub.Managers.Models.SitePages;

namespace WebPagePub.Managers.Interfaces
{
    public interface ISitePageManager
    {
        public SitePageSection CreateSiteSection(
            string sitePageName,
            string sitePageSectionKey,
            string createdByUserId);

        public SitePageSection GetSiteSection(int sitePageSectionId);

        public SitePageSection GetSiteSection(string key);

        public bool DoesPageExist(int siteSectionId, string pageKey);

        public bool DoesPageExistSimilar(int siteSectionId, string pageKey);

        SitePage GetPageForUrl(Uri sourcePage);

        public SitePage CreatePage(int siteSectionId, string pageTitle, string createdByUserId);

        public IList<SitePage> GetSitePages(int pageNumber, int siteSectionId, int quantityPerPage, out int total);

        public void DeleteSiteSection(int siteSectionId);

        public bool UpdateSiteSection(SitePageSection siteSection);

        public Task<SitePagePhoto> SetPhotoAsDefaultAsync(int sitePagePhotoId);

        public IEnumerable<SitePageSection> GetAllSiteSection();

        public SitePage CreatePage(SitePage sitePage);

        public Task<int> DeletePhotoAsync(int sitePagePhotoId);

        public SitePagePhoto RankPhotoUp(int sitePagePhotoId);

        public SitePagePhoto RankPhotoDown(int sitePagePhotoId);

        /// <summary>
        /// Upload photo to page.
        /// </summary>
        /// <param name="sitePageId"></param>
        /// <param name="fileNameAndImageMemoryStream">Filename, photo memory stream</param>
        /// <returns></returns>
        public Task UploadPhotos(int sitePageId, IList<Tuple<string, MemoryStream>> fileNameAndImageMemoryStream);

        public Task DeletePage(int sitePageId);

        public Task<int> Rotate90DegreesAsync(int sitePagePhotoId);

        public SitePage GetSitePage(int sitePageId);

        public bool UpdateSitePage(SitePage dbModel);

        public IList<SitePagePhoto> GetBlogPhotos(int sitePageId);

        public Task UpdatePhotoProperties(int sitePageId, IList<SitePagePhotoModel> newSitePagePhotos);

        void UpdateBlogTags(SitePageEditModel model, SitePage dbModel);

        public IList<Author> GetAllAuthors();

        public IList<SitePage> GetLivePage(int pageNumber, int maxPageSizeForSiteMap, out int total);

        public int? PreviouslyCreatedPage(DateTime createDate, int sitePageId, int sitePageSectionId);

        public int? NextCreatedPage(DateTime createDate, int sitePageId, int sitePageSectionId);

        IList<SitePage> SearchForTerm(string term, int pageNumber, int quantityPerPage, out int total);
    }
}