using System;
using System.Collections.Generic;
using WebPagePub.Data.DbContextInfo.Interfaces;
using WebPagePub.Data.Enums;
using WebPagePub.Data.Models.Db;

namespace WebPagePub.Data.Repositories.Interfaces
{
    public interface ISitePageCommentRepository : IDisposable
    {
        IApplicationDbContext Context { get; }

        SitePageComment Create(SitePageComment model);

        bool Update(SitePageComment model);

        SitePageComment Get(Guid requestId);

        SitePageComment Get(int sitePageCommentId);

        IList<SitePageComment> GetCommentsForPage(int sitePageId);

        IList<SitePageComment> GetCommentsForPage(int sitePageId, CommentStatus commentStatus);

        IList<SitePageComment> GetPage(int pageNumber, int quantityPerPage, out int total);

        int GetCommentCountForStatus(CommentStatus commentStatus);

        bool DeleteStaus(CommentStatus commentStatus);
    }
}
