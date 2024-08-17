using WebPagePub.FileStorage.Models;

namespace WebPagePub.FileStorage.Repositories.Interfaces
{
    public interface ISiteFilesRepository
    {
        string BlobPrefix { get; }

        Task<SiteFileDirectory> ListFilesAsync(string? prefix = null);

        Task DeleteFileAsync(string blobPath);

        Task<Uri> UploadAsync(Stream? stream, string? fileName, string? directory = null, string? expiresDate = null);

        Task CreateFolderAsync(string? folderPath, string? directory = null);

        Task DeleteFolderAsync(string? folderPath);

        Task ChangeFileName(string? currentFileName, string? newFileName);
    }
}