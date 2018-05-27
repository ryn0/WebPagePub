﻿using WebPagePub.Data.DbContextInfo;
using WebPagePub.Data.Enums;
using WebPagePub.Data.Models.Db;
using System;
using System.Collections.Generic;

namespace WebPagePub.Data.Repositories.Interfaces
{
    public interface IContentSnippetRepository : IDisposable
    {
        IApplicationDbContext Context { get; }

        ContentSnippet Create(ContentSnippet model);

        bool Update(ContentSnippet model);

        ContentSnippet Get(int contentSnippetId);

        ContentSnippet Get(SiteConfigSetting snippetType);

        bool Delete(int tagId);

        List<ContentSnippet> GetAll();
    }
}
