using WebPagePub.Data.DbContextInfo;
using WebPagePub.Data.Models;
using System;
using System.Collections.Generic;

namespace WebPagePub.Data.Repositories.Interfaces
{
    public interface ISitePageTagRepository : IDisposable
    {
        IApplicationDbContext Context { get; }

        SitePageTag Create(SitePageTag model);

        bool Update(SitePageTag model);

        SitePageTag Get(int tagId, int sitePageId);

        List<SitePageTag> GetTagsForBlog(int SitePageId);

        bool Delete(int tagId, int SitePageId); 
    }
}
