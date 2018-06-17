using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using WebPagePub.Data.Models.AzureStorage.Blob;

namespace WebPagePub.Data.Repositories.Interfaces
{
    public interface ISiteFilesRepository
    {
        SiteFileDirectory ListFiles(string prefix = null);

        Task DeleteFileAsync(string blobPath);

        Task<Uri> UploadAsync(IFormFile file, string directory = null);

        Task<Uri> UploadAsync(Stream stream, string fileName, string directory = null);

        Task CreateFolderAsync(string folderPath, string directory = null);

        Task DeleteFolderAsync(string folderPath);
    }
}
