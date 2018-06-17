using WebPagePub.Data.DbContextInfo;
using WebPagePub.Data.Models.Db;
using System;
using System.Collections.Generic;

namespace WebPagePub.Data.Repositories.Interfaces
{
    public interface ISitePageSectionRepository : IDisposable
    {
        IApplicationDbContext Context { get; }

        SitePageSection Create(SitePageSection model);

        bool Update(SitePageSection model);

        SitePageSection Get(int sitePageSectionId);

        SitePageSection GetHomeSection();

        List<SitePageSection> GetAll();

        SitePageSection Get(string key);
    }
}
