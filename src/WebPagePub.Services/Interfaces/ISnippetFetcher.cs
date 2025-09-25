using System;
using System.Threading.Tasks;

namespace WebPagePub.Services.Interfaces
{
    // ISnippetFetcher.cs
    public interface ISnippetFetcher
    {
        Task<string> GetAsync(string url, TimeSpan cacheFor);
    }
}