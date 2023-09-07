using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.Blob;
using WebPagePub.Data.BaseClasses;
using WebPagePub.Data.Constants;
using WebPagePub.Data.Repositories.Interfaces;

namespace WebPagePub.Data.Repositories.Implementations
{
    public class BlobService : BaseBlobFiles, IBlobService
    {
        public BlobService(CloudBlobClient blobClient)
        {
            this.BlobClient = blobClient;

            if (this.BlobClient == null)
            {
                return;
            }

            var container = this.BlobClient.GetContainerReference(StringConstants.ContainerName);

            // todo: run async elsewhere
            Task.Run(async () =>
            {
                await this.CreateIfNotExists(container);
                await this.SetCorsAsync(blobClient);
            });
        }

        public CloudBlobClient BlobClient { get; private set; }

        public CloudBlobContainer GetContainerReference(string containerName)
        {
            if (this.BlobClient == null)
            {
                return null;
            }

            return this.BlobClient.GetContainerReference(containerName);
        }

        private async Task CreateIfNotExists(CloudBlobContainer container)
        {
            if (await container.CreateIfNotExistsAsync())
            {
                await this.SetPublicContainerPermissionsAsync(container);
            }
        }
    }
}