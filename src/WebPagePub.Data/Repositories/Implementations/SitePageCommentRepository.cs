using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using log4net;
using WebPagePub.Data.Constants;
using WebPagePub.Data.DbContextInfo;
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

        public IApplicationDbContext Context { get; set; }

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
                return this.Context.SitePageComment
                              .FirstOrDefault(x => x.SitePageCommentId == sitePageCommentId);
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
                return this.Context.SitePageComment
                              .Where(x => x.CommentStatus == commentStatus)
                              .Count();
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

        public void Dispose()
        {
            this.Context.Dispose();
        }

        public IList<SitePageComment> GetPage(int pageNumber, int quantityPerPage, out int total)
        {
            try
            {
                var model = this.Context.SitePageComment
                                   .OrderByDescending(x => x.CreateDate)
                                   .Skip(quantityPerPage * (pageNumber - 1))
                                   .Take(quantityPerPage)
                                   .ToList();

                total = this.Context.SitePageComment.Count();

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
                var model = this.Context.SitePageComment.Where(x => x.CommentStatus == commentStatus);

                this.Context.SitePageComment.RemoveRange(model);
                this.Context.SaveChanges();

                return model.Count() > 0;
            }
            catch (Exception ex)
            {
                Log.Fatal(ex);

                throw new Exception(StringConstants.DBErrorMessage, ex.InnerException);
            }
        }
    }
}
