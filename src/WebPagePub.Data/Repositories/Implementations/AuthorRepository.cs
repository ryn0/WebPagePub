using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using log4net;
using WebPagePub.Data.Constants;
using WebPagePub.Data.DbContextInfo;
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
                return this.Context.Author.FirstOrDefault(x => x.AuthorId == authorId);
            }
            catch (Exception ex)
            {
                throw new Exception(StringConstants.DBErrorMessage, ex.InnerException);
            }
        }

        public List<Author> GetAll()
        {
            try
            {
                return this.Context.Author.ToList();
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
    }
}