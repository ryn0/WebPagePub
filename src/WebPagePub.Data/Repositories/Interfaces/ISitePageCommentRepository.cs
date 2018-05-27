﻿using WebPagePub.Data.DbContextInfo;
using WebPagePub.Data.Enums;
using WebPagePub.Data.Models.Db;
using System;
using System.Collections.Generic;

namespace WebPagePub.Data.Repositories.Interfaces
{
    public interface ISitePageCommentRepository : IDisposable
    {
        IApplicationDbContext Context { get; }

        SitePageComment Create(SitePageComment model);

        bool Update(SitePageComment model);

        SitePageComment Get(Guid requestId);

        SitePageComment Get(int sitePageCommentId);

        List<SitePageComment> GetCommentsForPage(int sitePageId);

        List<SitePageComment> GetCommentsForPage(int sitePageId, CommentStatus commentStatus);

        List<SitePageComment> GetPage(int pageNumber, int quantityPerPage, out int total);

        int GetCommentCountForStatus(CommentStatus commentStatus);
    }
}
