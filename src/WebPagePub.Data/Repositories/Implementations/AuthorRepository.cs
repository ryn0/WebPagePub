using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using log4net;
using Microsoft.EntityFrameworkCore;
using WebPagePub.Data.Constants;
using WebPagePub.Data.DbContextInfo.Interfaces;
using WebPagePub.Data.Models.Db;
using WebPagePub.Data.Repositories.Interfaces;

namespace WebPagePub.Data.Repositories.Implementations
{
    public class AuthorRepository : IAuthorRepository
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public AuthorRepository(IApplicationDbContext context)
        {
            this.Context = context;
        }

        public IApplicationDbContext Context { get; private set; }

        public Author Create(Author model)
        {
            try
            {
                this.Context.Author.Add(model);
                this.Context.SaveChanges();

                return model;
            }
            catch (Exception ex)
            {
                Log.Fatal(ex);
                throw new Exception(StringConstants.DBErrorMessage, ex.InnerException);
            }
        }

        public bool Update(Author model)
        {
            try
            {
                this.Context.Author.Update(model);
                this.Context.SaveChanges();

                return true;
            }
            catch (Exception ex)
            {
                Log.Fatal(ex);
                throw new Exception(StringConstants.DBErrorMessage, ex.InnerException);
            }
        }

        public Author Get(int authorId)
        {
            try
            {
                return this.Context.Author.AsNoTracking().FirstOrDefault(x => x.AuthorId == authorId);
            }
            catch (Exception ex)
            {
                throw new Exception(StringConstants.DBErrorMessage, ex.InnerException);
            }
        }

        public IList<Author> GetAll()
        {
            try
            {
                return this.Context.Author.AsNoTracking().ToList();
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
