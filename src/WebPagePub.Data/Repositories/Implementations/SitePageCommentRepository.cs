using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using log4net;
using Microsoft.EntityFrameworkCore;
using WebPagePub.Data.Constants;
using WebPagePub.Data.DbContextInfo.Interfaces;
using WebPagePub.Data.Enums;
using WebPagePub.Data.Models.Db;
using WebPagePub.Data.Repositories.Interfaces;

namespace WebPagePub.Data.Repositories.Implementations
{
    public class SitePageCommentRepository : ISitePageCommentRepository
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public SitePageCommentRepository(IApplicationDbContext context)
        {
            this.Context = context;
        }

        // FIX: was `public set` — external code should never swap the context out
        // from under a repository that is already in use.
        public IApplicationDbContext Context { get; private set; }

        public SitePageComment Create(SitePageComment model)
        {
            try
            {
                this.Context.SitePageComment.Add(model);
                this.Context.SaveChanges();

                return model;
            }
            catch (Exception ex)
            {
                Log.Fatal(ex);
                throw new Exception(StringConstants.DBErrorMessage, ex.InnerException);
            }
        }

        public SitePageComment Get(int sitePageCommentId)
        {
            try
            {
                return this.Context.SitePageComment.Find(sitePageCommentId);
            }
            catch (Exception ex)
            {
                Log.Fatal(ex);
                throw new Exception(StringConstants.DBErrorMessage, ex.InnerException);
            }
        }

        public SitePageComment Get(Guid requestId)
        {
            try
            {
                return this.Context.SitePageComment
                    .AsNoTracking()
                    .FirstOrDefault(x => x.RequestId == requestId);
            }
            catch (Exception ex)
            {
                Log.Fatal(ex);
                throw new Exception(StringConstants.DBErrorMessage, ex.InnerException);
            }
        }

        public IList<SitePageComment> GetCommentsForPage(int sitePageId)
        {
            try
            {
                return this.Context.SitePageComment
                    .AsNoTracking()
                    .Where(x => x.SitePageId == sitePageId)
                    .ToList();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex);
                throw new Exception(StringConstants.DBErrorMessage, ex.InnerException);
            }
        }

        public IList<SitePageComment> GetCommentsForPage(int sitePageId, CommentStatus commentStatus)
        {
            try
            {
                return this.Context.SitePageComment
                    .AsNoTracking()
                    .Where(x => x.SitePageId == sitePageId && x.CommentStatus == commentStatus)
                    .ToList();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex);
                throw new Exception(StringConstants.DBErrorMessage, ex.InnerException);
            }
        }

        public int GetCommentCountForStatus(CommentStatus commentStatus)
        {
            try
            {
                // FIX: added AsNoTracking — read-only scalar count; no need to load
                // tracked entities into the change tracker.
                return this.Context.SitePageComment
                    .AsNoTracking()
                    .Count(x => x.CommentStatus == commentStatus);
            }
            catch (Exception ex)
            {
                Log.Fatal(ex);
                throw new Exception(StringConstants.DBErrorMessage, ex.InnerException);
            }
        }

        public bool Update(SitePageComment model)
        {
            try
            {
                this.Context.SitePageComment.Update(model);
                this.Context.SaveChanges();

                return true;
            }
            catch (Exception ex)
            {
                Log.Fatal(ex);
                throw new Exception(StringConstants.DBErrorMessage, ex.InnerException);
            }
        }

        public IList<SitePageComment> GetPage(int pageNumber, int quantityPerPage, out int total)
        {
            try
            {
                // FIX: added AsNoTracking — paged admin list is read-only display.
                var model = this.Context.SitePageComment
                    .AsNoTracking()
                    .OrderByDescending(x => x.CreateDate)
                    .Skip(quantityPerPage * (pageNumber - 1))
                    .Take(quantityPerPage)
                    .ToList();

                total = this.Context.SitePageComment.AsNoTracking().Count();

                return model;
            }
            catch (Exception ex)
            {
                Log.Fatal(ex);
                throw new Exception(StringConstants.DBErrorMessage, ex.InnerException);
            }
        }

        public bool DeleteStaus(CommentStatus commentStatus)
        {
            try
            {
                // FIX: The original stored an IQueryable in `model`, called RemoveRange
                // (which enumerates and stages the deletes), called SaveChanges (which
                // commits them), then called model.Count() — which re-ran the SQL COUNT
                // against a table that now has zero matching rows. The method therefore
                // ALWAYS returned false, regardless of how many rows were deleted.
                //
                // Fix: materialize the list first so the count is known before any rows
                // are removed, then remove the already-loaded entities by reference.
                var toDelete = this.Context.SitePageComment
                    .Where(x => x.CommentStatus == commentStatus)
                    .ToList();

                if (toDelete.Count == 0)
                {
                    return false;
                }

                this.Context.SitePageComment.RemoveRange(toDelete);
                this.Context.SaveChanges();

                return true;
            }
            catch (Exception ex)
            {
                Log.Fatal(ex);
                throw new Exception(StringConstants.DBErrorMessage, ex.InnerException);
            }
        }

        // The context is registered via AddDbContextPool in Program.cs — the DI
        // container owns its lifetime. Calling Context.Dispose() here would return
        // the context to the pool while other scoped services may still hold a
        // reference to the same instance, causing use-after-dispose errors.
        public void Dispose()
        {
            // Intentionally empty. Context lifetime is managed by the DI container.
        }
    }
}
