using System;
using System.Collections.Generic;
using WebPagePub.Data.DbContextInfo;
using WebPagePub.Data.Models;

namespace WebPagePub.Data.Repositories.Interfaces
{
    public interface ISitePageTagRepository : IDisposable
    {
        IApplicationDbContext Context { get; }

        SitePageTag Create(SitePageTag model);

        bool Update(SitePageTag model);

        SitePageTag Get(int tagId, int sitePageId);

        IList<SitePageTag> GetTagsForBlog(int sitePageId);

        bool Delete(int tagId, int sitePageId);

        IList<SitePageTag> GetAll();

        IList<SitePageTag> GetTagsForLivePages();
    }
}
