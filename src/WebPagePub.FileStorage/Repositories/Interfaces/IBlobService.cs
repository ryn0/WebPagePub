using Azure.Storage.Blobs;

namespace WebPagePub.FileStorage.Repositories.Interfaces
{
    public interface IBlobService
    {
        string BlobPrefix { get; }

        BlobContainerClient? GetContainerReference(string containerName);
    }
}