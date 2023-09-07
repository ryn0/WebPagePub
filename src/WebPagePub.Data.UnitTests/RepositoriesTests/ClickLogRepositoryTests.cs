using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Moq;
using WebPagePub.Data.DbContextInfo;
using WebPagePub.Data.Models.Db;
using WebPagePub.Data.Repositories.Implementations;
using Xunit;

namespace WebPagePub.Data.UnitTests.RepositoriesTests
{
    public class ClickLogRepositoryTests
    {
        private readonly Mock<IApplicationDbContext> contextMock;
        private readonly ClickLogRepository repository;

        public ClickLogRepositoryTests()
        {
            this.contextMock = new Mock<IApplicationDbContext>();
            this.repository = new ClickLogRepository(this.contextMock.Object);
        }

        [Fact]
        public async Task CreateAsync_ShouldAddClickLogAndReturnIt()
        {
            var clickLog = new ClickLog { /* initialization, for instance: URL = "http://test.com" */ };

            var dbSetMock = new Mock<DbSet<ClickLog>>();
            this.contextMock.Setup(c => c.ClickLog).Returns(dbSetMock.Object);
            this.contextMock.Setup(c => c.SaveChangesAsync(default)).ReturnsAsync(1);

            var result = await this.repository.CreateAsync(clickLog);

            Assert.Equal(clickLog, result);
            dbSetMock.Verify(d => d.Add(It.IsAny<ClickLog>()), Times.Once);
            this.contextMock.Verify(c => c.SaveChangesAsync(default), Times.Once);
        }

        [Fact]
        public void GetClicksInRange_ShouldReturnClicksInSpecifiedDateRange()
        {
            var startDate = new DateTime(2023, 1, 1);
            var endDate = new DateTime(2023, 12, 31);
            var clickLog1 = new ClickLog { CreateDate = new DateTime(2023, 5, 5) };
            var clickLog2 = new ClickLog { CreateDate = new DateTime(2023, 8, 8) };

            var clickLogs = new List<ClickLog> { clickLog1, clickLog2 }.AsQueryable();
            var dbSetMock = new Mock<DbSet<ClickLog>>();
            dbSetMock.As<IQueryable<ClickLog>>().Setup(m => m.Provider).Returns(clickLogs.Provider);
            dbSetMock.As<IQueryable<ClickLog>>().Setup(m => m.Expression).Returns(clickLogs.Expression);
            dbSetMock.As<IQueryable<ClickLog>>().Setup(m => m.ElementType).Returns(clickLogs.ElementType);
            dbSetMock.As<IQueryable<ClickLog>>().Setup(m => m.GetEnumerator()).Returns(clickLogs.GetEnumerator());

            this.contextMock.Setup(c => c.ClickLog).Returns(dbSetMock.Object);

            var result = this.repository.GetClicksInRange(startDate, endDate);

            Assert.Contains(clickLog1, result);
            Assert.Contains(clickLog2, result);
        }
    }
}
