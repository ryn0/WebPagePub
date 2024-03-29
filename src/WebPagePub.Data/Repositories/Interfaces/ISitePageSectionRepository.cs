﻿using System;
using System.Collections.Generic;
using WebPagePub.Data.DbContextInfo.Interfaces;
using WebPagePub.Data.Models.Db;

namespace WebPagePub.Data.Repositories.Interfaces
{
    public interface ISitePageSectionRepository : IDisposable
    {
        IApplicationDbContext Context { get; }

        SitePageSection Create(SitePageSection model);

        bool Update(SitePageSection model);

        SitePageSection Get(int sitePageSectionId);

        SitePageSection GetHomeSection();

        IList<SitePageSection> GetAll();

        SitePageSection Get(string key);

        bool Delete(int sitePageSectionId);
    }
}
