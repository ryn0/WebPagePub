﻿using System;
using System.Collections.Generic;
using WebPagePub.Data.DbContextInfo;
using WebPagePub.Data.Models.Db;

namespace WebPagePub.Data.Repositories.Interfaces
{
    public interface IRedirectPathRepository : IDisposable
    {
        IApplicationDbContext Context { get; }

        List<RedirectPath> GetAll();

        RedirectPath Get(int redirectPathId);

        RedirectPath Create(RedirectPath model);

        bool Delete(int redirectPathId);

        RedirectPath Get(string path);
    }
}