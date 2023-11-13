using Microsoft.WindowsAzure.Storage.Blob;
using Moq;
using WebPagePub.Data.Repositories.Implementations;
using Xunit;

namespace WebPagePub.Data.UnitTests.RepositoriesTests
{
    public class BlobServiceTests
    {
        private readonly Mock<CloudBlobClient> mockBlobClient;
        private readonly Mock<CloudBlobContainer> mockBlobContainer;

        public BlobServiceTests()
        {
            // Mock CloudBlobClient
            this.mockBlobClient = new Mock<CloudBlobClient>(new System.Uri("http://tempuri.org"));
            this.mockBlobContainer = new Mock<CloudBlobContainer>(new System.Uri("http://tempuri.org/container"));

            // Set up mocked method behavior
            this.mockBlobClient.Setup(client => client.GetContainerReference(It.IsAny<string>())).Returns(this.mockBlobContainer.Object);
        }
    }
}