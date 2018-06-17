using System;
using System.Collections.Generic;
using WebPagePub.Data.DbContextInfo;
using WebPagePub.Data.Models;

namespace WebPagePub.Data.Repositories.Interfaces
{
    public interface ISitePagePhotoRepository : IDisposable
    {
        IApplicationDbContext Context { get; }

        SitePagePhoto Create(SitePagePhoto model);

        bool Update(SitePagePhoto model);

        SitePagePhoto Get(int sitePagePhotoId);

        bool Delete(int sitePagePhotoId);

        List<SitePagePhoto> GetBlogPhotos(int sitePageId);

        void SetDefaultPhoto(int sitePagePhotoId);
    }
}
