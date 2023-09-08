using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Moq;
using WebPagePub.Data.DbContextInfo;
using WebPagePub.Data.Models.Db;
using WebPagePub.Data.Repositories.Implementations;
using Xunit;

namespace WebPagePub.Data.UnitTests.RepositoriesTests
{
    public class SitePageSectionRepositoryTest
    {
        private readonly Mock<IApplicationDbContext> contextMock;
        private readonly SitePageSectionRepository repository;

        public SitePageSectionRepositoryTest()
        {
            this.contextMock = new Mock<IApplicationDbContext>();
            this.repository = new SitePageSectionRepository(this.contextMock.Object);

            this.contextMock.Setup(c => c.SitePageSection.Add(It.IsAny<SitePageSection>()));
            this.contextMock.Setup(c => c.SaveChanges()).Returns(1); // Assuming save changes will be successful
        }

        [Fact]
        public void Create_SitePageSection_ReturnsCreatedSitePageSection()
        {
            var section = new SitePageSection();

            this.contextMock.Setup(c => c.SaveChanges()).Returns(1); // Assuming save changes will be successful

            var result = this.repository.Create(section);

            Assert.NotNull(result);
        }

        [Fact]
        public void Delete_ValidSitePageSectionId_ReturnsTrue()
        {
            var section = new SitePageSection { SitePageSectionId = 1 };
            this.contextMock.Setup(c => c.SitePageSection.Find(1)).Returns(section);
            this.contextMock.Setup(c => c.SaveChanges()).Returns(1);

            var result = this.repository.Delete(1);

            Assert.True(result);
        }

        [Fact]
        public void Get_ValidSitePageSectionId_ReturnsSitePageSection()
        {
            var section = new SitePageSection { SitePageSectionId = 1 };
            this.contextMock.Setup(c => c.SitePageSection.Find(1)).Returns(section);

            var result = this.repository.Get(1);

            Assert.NotNull(result);
            Assert.Equal(1, result.SitePageSectionId);
        }

        [Fact]
        public void Get_ValidKey_ReturnsSitePageSection()
        {
            // Arrange
            var section = new SitePageSection { Key = "testKey" };

            var mockSet = new Mock<DbSet<SitePageSection>>();
            mockSet.As<IQueryable<SitePageSection>>().Setup(m => m.Provider).Returns(new List<SitePageSection> { section }.AsQueryable().Provider);
            mockSet.As<IQueryable<SitePageSection>>().Setup(m => m.Expression).Returns(new List<SitePageSection> { section }.AsQueryable().Expression);
            mockSet.As<IQueryable<SitePageSection>>().Setup(m => m.ElementType).Returns(new List<SitePageSection> { section }.AsQueryable().ElementType);
            mockSet.As<IQueryable<SitePageSection>>().Setup(m => m.GetEnumerator()).Returns(new List<SitePageSection> { section }.AsQueryable().GetEnumerator());

            this.contextMock.Setup(c => c.SitePageSection).Returns(mockSet.Object);

            // Act
            var result = this.repository.Get("testKey");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("testKey", result.Key);
        }
    }
}