using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Moq;
using WebPagePub.Data.DbContextInfo.Interfaces;
using WebPagePub.Data.Models.Db;
using WebPagePub.Data.Repositories.Implementations;
using Xunit;

namespace WebPagePub.Data.UnitTests.RepositoriesTests
{
    public class BlockedIPRepositoryTests
    {
        private readonly Mock<IApplicationDbContext> contextMock;
        private readonly Mock<DbSet<BlockedIP>> blockedIpMock;
        private readonly BlockedIPRepository repository;

        public BlockedIPRepositoryTests()
        {
            this.contextMock = new Mock<IApplicationDbContext>();
            this.blockedIpMock = new Mock<DbSet<BlockedIP>>();
            this.contextMock.Setup(c => c.BlockedIP).Returns(this.blockedIpMock.Object);
            this.repository = new BlockedIPRepository(this.contextMock.Object);
        }

        [Fact]
        public async Task CreateAsync_ShouldAddBlockedIPAndReturnIt()
        {
            var blockedIP = new BlockedIP { /* initialization, for instance: IpAddress = "127.0.0.1" */ };

            var dbSetMock = new Mock<DbSet<BlockedIP>>();

            // Setup the context to return the mocked DbSet
            this.contextMock.Setup(c => c.BlockedIP).Returns(dbSetMock.Object);

            // Mock SaveChangesAsync method to be trackable
            this.contextMock.Setup(c => c.SaveChangesAsync(default)).ReturnsAsync(1);

            var result = await this.repository.CreateAsync(blockedIP);

            Assert.Equal(blockedIP, result);
            dbSetMock.Verify(d => d.Add(It.IsAny<BlockedIP>()), Times.Once);
            this.contextMock.Verify(c => c.SaveChangesAsync(default), Times.Once);
        }

        [Fact]
        public void IsBlockedIp_ShouldReturnFalse_WhenIpAddressIsNotBlocked()
        {
            var ipAddress = "127.0.0.2";
            var blockedIPs = new List<BlockedIP>().AsQueryable();
            var dbSetMock = new Mock<DbSet<BlockedIP>>();

            dbSetMock.As<IQueryable<BlockedIP>>().Setup(m => m.Provider).Returns(blockedIPs.Provider);
            dbSetMock.As<IQueryable<BlockedIP>>().Setup(m => m.Expression).Returns(blockedIPs.Expression);
            dbSetMock.As<IQueryable<BlockedIP>>().Setup(m => m.ElementType).Returns(blockedIPs.ElementType);
            dbSetMock.As<IQueryable<BlockedIP>>().Setup(m => m.GetEnumerator()).Returns(blockedIPs.GetEnumerator());

            this.contextMock.Setup(c => c.BlockedIP).Returns(dbSetMock.Object);

            var isBlocked = this.repository.IsBlockedIp(ipAddress);

            Assert.False(isBlocked);
        }

        [Fact]
        public void IsBlockedIp_ShouldReturnTrue_WhenIpAddressIsBlocked()
        {
            var ipAddress = "127.0.0.2";
            var blockedIP = new BlockedIP { IpAddress = ipAddress };

            var blockedIPs = new List<BlockedIP> { blockedIP }.AsQueryable();
            var dbSetMock = new Mock<DbSet<BlockedIP>>();

            dbSetMock.As<IQueryable<BlockedIP>>().Setup(m => m.Provider).Returns(blockedIPs.Provider);
            dbSetMock.As<IQueryable<BlockedIP>>().Setup(m => m.Expression).Returns(blockedIPs.Expression);
            dbSetMock.As<IQueryable<BlockedIP>>().Setup(m => m.ElementType).Returns(blockedIPs.ElementType);
            dbSetMock.As<IQueryable<BlockedIP>>().Setup(m => m.GetEnumerator()).Returns(blockedIPs.GetEnumerator());

            this.contextMock.Setup(c => c.BlockedIP).Returns(dbSetMock.Object);

            var isBlocked = this.repository.IsBlockedIp(ipAddress);

            Assert.True(isBlocked);
        }
    }
}