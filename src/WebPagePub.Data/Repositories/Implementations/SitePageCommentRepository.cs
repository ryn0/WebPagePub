using log4net;
using WebPagePub.Data.DbContextInfo;
using WebPagePub.Data.Enums;
using WebPagePub.Data.Models.Db;
using WebPagePub.Data.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace WebPagePub.Data.Repositories.Implementations
{
    public class SitePageCommentRepository : ISitePageCommentRepository
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public SitePageCommentRepository(IApplicationDbContext context)
        {
            Context = context;
        }

        public IApplicationDbContext Context { get; set; }

        public SitePageComment Create(SitePageComment model)
        {
            try
            {
                Context.SitePageComment.Add(model);
                Context.SaveChanges();

                return model;
            }
            catch (Exception ex)
            {
                log.Fatal(ex);
                throw new Exception("DB error", ex.InnerException);
            }
        }

        public SitePageComment Get(int sitePageCommentId)
        {
            try
            {
                return Context.SitePageComment
                              .FirstOrDefault(x => x.SitePageCommentId == sitePageCommentId);
            }
            catch (Exception ex)
            {
                log.Fatal(ex);
                throw new Exception("DB error", ex.InnerException);
            }
        }

        public SitePageComment Get(Guid requestId)
        {
            try
            {
                return Context.SitePageComment
                              .FirstOrDefault(x => x.RequestId == requestId);
            }
            catch (Exception ex)
            {
                log.Fatal(ex);
                throw new Exception("DB error", ex.InnerException);
            }
        }

        public List<SitePageComment> GetCommentsForPage(int sitePageId)
        {
            try
            {
                return Context.SitePageComment
                              .Where(x => x.SitePageId == sitePageId)
                              .ToList();
            }
            catch (Exception ex)
            {
                log.Fatal(ex);
                throw new Exception("DB error", ex.InnerException);
            }
        }

        public List<SitePageComment> GetCommentsForPage(int sitePageId, CommentStatus commentStatus)
        {
            try
            {
                return Context.SitePageComment
                              .Where(x => x.SitePageId == sitePageId && x.CommentStatus == commentStatus)
                              .ToList();
            }
            catch (Exception ex)
            {
                log.Fatal(ex);
                throw new Exception("DB error", ex.InnerException);
            }
        }

        public int GetCommentCountForStatus(CommentStatus commentStatus)
        {
            try
            {
                return Context.SitePageComment
                              .Where(x => x.CommentStatus == commentStatus)
                              .Count();
            }
            catch (Exception ex)
            {
                log.Fatal(ex);
                throw new Exception("DB error", ex.InnerException);
            }
        }

        public bool Update(SitePageComment model)
        {
            try
            {
                Context.SitePageComment.Update(model);
                Context.SaveChanges();

                return true;
            }
            catch (Exception ex)
            {
                log.Fatal(ex);
                throw new Exception("DB error", ex.InnerException);
            }
        }

        public void Dispose()
        {
            Context.Dispose();
        }

        public List<SitePageComment> GetPage(int pageNumber, int quantityPerPage, out int total)
        {
            try
            {
                var model = Context.SitePageComment
                                   .OrderByDescending(x => x.CreateDate)
                                   .Skip(quantityPerPage * (pageNumber - 1))
                                   .Take(quantityPerPage)
                                   .ToList();

                total = Context.SitePageComment.Count();

                return model;
            }
            catch (Exception ex)
            {
                log.Fatal(ex);

                throw new Exception("DB error", ex.InnerException);
            }
        }
    }
}
