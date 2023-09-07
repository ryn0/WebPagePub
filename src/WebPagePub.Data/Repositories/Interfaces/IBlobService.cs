using Microsoft.WindowsAzure.Storage.Blob;

namespace WebPagePub.Data.Repositories.Interfaces
{
    public interface IBlobService
    {
        CloudBlobContainer GetContainerReference(string containerName);
    }
}