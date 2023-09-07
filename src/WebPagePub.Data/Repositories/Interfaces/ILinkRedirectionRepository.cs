using System;
using System.Collections.Generic;
using WebPagePub.Data.DbContextInfo;
using WebPagePub.Data.Models.Db;

namespace WebPagePub.Data.Repositories.Interfaces
{
    public interface ILinkRedirectionRepository : IDisposable
    {
        IApplicationDbContext Context { get; }

        LinkRedirection Create(LinkRedirection model);

        bool Update(LinkRedirection model);

        LinkRedirection Get(int linkRedirectionId);

        LinkRedirection Get(string key);

        IList<LinkRedirection> GetAll();

        bool Delete(int linkRedirectionId);
    }
}
