using System;
using System.Collections.Generic;
using WebPagePub.Data.DbContextInfo;
using WebPagePub.Data.Models.Db;

namespace WebPagePub.Data.Repositories.Interfaces
{
    public interface IAuthorRepository : IDisposable
    {
        IApplicationDbContext Context { get; }

        Author Create(Author model);

        bool Update(Author model);

        Author Get(int authorId);

        IList<Author> GetAll();
    }
}
