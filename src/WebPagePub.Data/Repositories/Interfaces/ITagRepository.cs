using System;
using WebPagePub.Data.DbContextInfo;
using WebPagePub.Data.Models;

namespace WebPagePub.Data.Repositories.Interfaces
{
    public interface ITagRepository : IDisposable
    {
        IApplicationDbContext Context { get; }

        Tag Create(Tag model);

        bool Update(Tag model);

        Tag Get(int tagId);

        Tag Get(string key);

        bool Delete(int tagId);
    }
}
