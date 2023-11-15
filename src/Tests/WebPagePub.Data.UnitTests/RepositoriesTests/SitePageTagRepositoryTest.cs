using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Moq;
using WebPagePub.Data.DbContextInfo.Interfaces;
using WebPagePub.Data.Models;
using WebPagePub.Data.Repositories.Implementations;
using Xunit;

namespace WebPagePub.Data.UnitTests.RepositoriesTests
{
    public class SitePageTagRepositoryTest
    {
        private readonly Mock<IApplicationDbContext> contextMock;
        private readonly SitePageTagRepository repository;

        public SitePageTagRepositoryTest()
        {
            this.contextMock = new Mock<IApplicationDbContext>();
            this.repository = new SitePageTagRepository(this.contextMock.Object);
        }

        [Fact]
        public void Create_ValidSitePageTag_ReturnsCreatedSitePageTag()
        {
            // Arrange
            var tag = new SitePageTag();
            this.contextMock.Setup(c => c.SitePageTag.Add(tag));

            // Act
            var result = this.repository.Create(tag);

            // Assert
            Assert.Equal(tag, result);
        }

        [Fact]
        public void Update_ValidSitePageTag_ReturnsTrue()
        {
            // Arrange
            var tag = new SitePageTag();
            this.contextMock.Setup(c => c.SitePageTag.Update(tag));

            // Act
            var result = this.repository.Update(tag);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void Delete_ValidId_ReturnsTrue()
        {
            // Arrange
            var tagId = 1;
            var sitePageId = 2;
            var tag = new SitePageTag { TagId = tagId, SitePageId = sitePageId };

            var mockSet = new Mock<DbSet<SitePageTag>>();
            mockSet.As<IQueryable<SitePageTag>>().Setup(m => m.Provider).Returns(new List<SitePageTag> { tag }.AsQueryable().Provider);
            mockSet.As<IQueryable<SitePageTag>>().Setup(m => m.Expression).Returns(new List<SitePageTag> { tag }.AsQueryable().Expression);
            mockSet.As<IQueryable<SitePageTag>>().Setup(m => m.ElementType).Returns(new List<SitePageTag> { tag }.AsQueryable().ElementType);
            mockSet.As<IQueryable<SitePageTag>>().Setup(m => m.GetEnumerator()).Returns(new List<SitePageTag> { tag }.GetEnumerator());

            this.contextMock.Setup(c => c.SitePageTag).Returns(mockSet.Object);

            // Act
            var result = this.repository.Delete(tagId, sitePageId);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void Get_ValidIds_ReturnsSitePageTag()
        {
            // Arrange
            var tagId = 1;
            var sitePageId = 2;
            var tag = new SitePageTag { TagId = tagId, SitePageId = sitePageId };

            var data = new List<SitePageTag> { tag }.AsQueryable();

            var mockSet = new Mock<DbSet<SitePageTag>>();
            mockSet.As<IQueryable<SitePageTag>>().Setup(m => m.Provider).Returns(data.Provider);
            mockSet.As<IQueryable<SitePageTag>>().Setup(m => m.Expression).Returns(data.Expression);
            mockSet.As<IQueryable<SitePageTag>>().Setup(m => m.ElementType).Returns(data.ElementType);
            mockSet.As<IQueryable<SitePageTag>>().Setup(m => m.GetEnumerator()).Returns(data.GetEnumerator());

            this.contextMock.Setup(c => c.SitePageTag).Returns(mockSet.Object);

            // Act
            var result = this.repository.Get(tagId, sitePageId);

            // Assert
            Assert.Equal(tag, result);
        }

        [Fact]
        public void GetAll_ReturnsAllTags()
        {
            // Arrange
            var tags = new List<SitePageTag>
            {
                new SitePageTag { TagId = 1, SitePageId = 2 },
                new SitePageTag { TagId = 3, SitePageId = 4 },

                // ... other tags as needed
            }.AsQueryable();

            var mockSet = new Mock<DbSet<SitePageTag>>();
            mockSet.As<IQueryable<SitePageTag>>().Setup(m => m.Provider).Returns(tags.Provider);
            mockSet.As<IQueryable<SitePageTag>>().Setup(m => m.Expression).Returns(tags.Expression);
            mockSet.As<IQueryable<SitePageTag>>().Setup(m => m.ElementType).Returns(tags.ElementType);
            mockSet.As<IQueryable<SitePageTag>>().Setup(m => m.GetEnumerator()).Returns(tags.GetEnumerator());

            this.contextMock.Setup(c => c.SitePageTag).Returns(mockSet.Object);

            // Act
            var result = this.repository.GetAll();

            // Assert
            Assert.Equal(tags.Count(), result.Count);
        }

        [Fact]
        public void GetTagsForLivePages_ReturnsTagsForLivePages()
        {
            // Arrange
            var tags = new List<SitePageTag>
            {
                new SitePageTag { TagId = 1, SitePageId = 2, SitePage = new SitePage { IsLive = true } },
                new SitePageTag { TagId = 3, SitePageId = 4, SitePage = new SitePage { IsLive = false } },

                // ... other tags as needed
            }.AsQueryable();

            var mockSet = new Mock<DbSet<SitePageTag>>();
            mockSet.As<IQueryable<SitePageTag>>().Setup(m => m.Provider).Returns(tags.Provider);
            mockSet.As<IQueryable<SitePageTag>>().Setup(m => m.Expression).Returns(tags.Expression);
            mockSet.As<IQueryable<SitePageTag>>().Setup(m => m.ElementType).Returns(tags.ElementType);
            mockSet.As<IQueryable<SitePageTag>>().Setup(m => m.GetEnumerator()).Returns(tags.GetEnumerator());

            this.contextMock.Setup(c => c.SitePageTag).Returns(mockSet.Object);

            // Act
            var result = this.repository.GetTagsForLivePages();

            // Assert
            Assert.True(result.All(tag => tag.SitePage.IsLive));
        }
    }
}