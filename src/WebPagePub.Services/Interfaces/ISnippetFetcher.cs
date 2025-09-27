using System.Threading.Tasks;

namespace WebPagePub.Services.Interfaces
{
    public interface ISnippetFetcher
    {
        Task<string> GetAsync(string url);
    }
}