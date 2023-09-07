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

        [Fact]
        public void Constructor_NullBlobClient_BlobClientIsNull()
        {
            var blobService = new BlobService(null);

            Assert.Null(blobService.BlobClient);
        }

        [Fact]
        public void Constructor_ValidBlobClient_BlobClientIsNotNull()
        {
            var blobService = new BlobService(this.mockBlobClient.Object);

            Assert.NotNull(blobService.BlobClient);
        }

        [Fact]
        public void GetContainerReference_WithValidName_ReturnsContainer()
        {
            var blobService = new BlobService(this.mockBlobClient.Object);

            var containerName = "testContainer";
            var container = blobService.GetContainerReference(containerName);

            Assert.NotNull(container);
            this.mockBlobClient.Verify(client => client.GetContainerReference(containerName), Times.Once);
        }
    }
}
