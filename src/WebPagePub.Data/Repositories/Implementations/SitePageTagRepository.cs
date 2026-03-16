using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using log4net;
using Microsoft.EntityFrameworkCore;
using WebPagePub.Data.Constants;
using WebPagePub.Data.DbContextInfo.Interfaces;
using WebPagePub.Data.Models;
using WebPagePub.Data.Repositories.Interfaces;

namespace WebPagePub.Data.Repositories.Implementations
{
    public class SitePageTagRepository : ISitePageTagRepository
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public SitePageTagRepository(IApplicationDbContext context)
        {
            this.Context = context;
        }

        public IApplicationDbContext Context { get; private set; }

        // FIX 1: Do NOT dispose the context here.
        //
        // The context is registered via AddDbContextPool<ApplicationDbContext> in
        // Program.cs, which means the DI container owns the context's lifetime. When
        // the container's scope ends it resets and returns the context to the pool.
        //
        // Calling Context.Dispose() from the repository:
        //   (a) Tears down the context before the DI scope is done with it — any other
        //       scoped service that holds the same IApplicationDbContext reference will
        //       then operate on a disposed object and throw an ObjectDisposedException.
        //   (b) With pooling specifically, "disposing" a context resets it and returns
        //       it to the pool; the repository still holds its reference, so the next
        //       call through this repository touches a context that may already be in
        //       use by a completely different request.
        //
        // The interface forces IDisposable, so the method must exist — it is a no-op.
        public void Dispose()
        {
            // Intentionally empty. Context lifetime is managed by the DI container.
        }

        public bool Delete(int tagId, int sitePageId)
        {
            try
            {
                // FIX 2: The original called Remove(entry) without checking whether
                // FirstOrDefault returned null. Passing null to DbSet.Remove throws an
                // ArgumentNullException with no context about which record was missing.
                // Guard here and return false so the caller can handle a missing row
                // gracefully rather than crashing.
                var entry = this.Context.SitePageTag
                    .FirstOrDefault(x => x.TagId == tagId && x.SitePageId == sitePageId);

                if (entry == null)
                {
                    return false;
                }

                this.Context.SitePageTag.Remove(entry);
                this.Context.SaveChanges();

                return true;
            }
            catch (Exception ex)
            {
                Log.Fatal(ex);
                return false;
            }
        }

        public SitePageTag Create(SitePageTag model)
        {
            try
            {
                this.Context.SitePageTag.Add(model);
                this.Context.SaveChanges();

                return model;
            }
            catch (Exception ex)
            {
                Log.Fatal(ex);
                throw new Exception(StringConstants.DBErrorMessage, ex.InnerException);
            }
        }

        public bool Update(SitePageTag model)
        {
            try
            {
                this.Context.SitePageTag.Update(model);
                this.Context.SaveChanges();

                return true;
            }
            catch (Exception ex)
            {
                Log.Fatal(ex);
                throw new Exception(StringConstants.DBErrorMessage, ex.InnerException);
            }
        }

        public SitePageTag Get(int tagId, int sitePageId)
        {
            try
            {
                return this.Context.SitePageTag
                    .AsNoTracking()
                    .FirstOrDefault(x => x.TagId == tagId && x.SitePageId == sitePageId);
            }
            catch (Exception ex)
            {
                throw new Exception(StringConstants.DBErrorMessage, ex.InnerException);
            }
        }

        public IList<SitePageTag> GetTagsForBlog(int sitePageId)
        {
            try
            {
                return this.Context.SitePageTag
                    .AsNoTracking()
                    .Where(x => x.SitePageId == sitePageId)
                    .ToList();
            }
            catch (Exception ex)
            {
                throw new Exception(StringConstants.DBErrorMessage, ex.InnerException);
            }
        }

        public IList<SitePageTag> GetAll()
        {
            try
            {
                return this.Context.SitePageTag
                    .AsNoTracking()
                    .ToList();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex);
                throw new Exception(StringConstants.DBErrorMessage, ex.InnerException);
            }
        }

        public IList<SitePageTag> GetTagsForLivePages()
        {
            try
            {
                return this.Context.SitePageTag
                    .AsNoTracking()
                    .Where(x => x.SitePage.IsLive == true)
                    .ToList();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex);
                throw new Exception(StringConstants.DBErrorMessage, ex.InnerException);
            }
        }
    }
}