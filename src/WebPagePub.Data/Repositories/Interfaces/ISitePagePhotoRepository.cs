using WebPagePub.Data.DbContextInfo;
using WebPagePub.Data.Models;
using System;
using System.Collections.Generic;

namespace WebPagePub.Data.Repositories.Interfaces
{
    public interface ISitePagePhotoRepository : IDisposable
    {
        IApplicationDbContext Context { get; }

        SitePagePhoto Create(SitePagePhoto model);

        bool Update(SitePagePhoto model);

        SitePagePhoto Get(int SitePagePhotoId);

        bool Delete(int SitePagePhotoId);

        List<SitePagePhoto> GetBlogPhotos(int SitePageId);

        void SetDefaultPhoto(int SitePagePhotoId);
    }
}
