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
    public class SitePagePhotoRepositoryTests
    {
        private readonly Mock<IApplicationDbContext> mockContext;
        private readonly Mock<DbSet<SitePagePhoto>> mockDbSet;
        private readonly List<SitePagePhoto> dbSetData;
        private readonly SitePagePhotoRepository repo;

        public SitePagePhotoRepositoryTests()
        {
            this.mockContext = new Mock<IApplicationDbContext>();
            this.mockDbSet = new Mock<DbSet<SitePagePhoto>>();
            this.dbSetData = new List<SitePagePhoto>();

            IQueryable<SitePagePhoto> queryable = this.dbSetData.AsQueryable();
            this.mockDbSet.As<IQueryable<SitePagePhoto>>().Setup(m => m.Provider).Returns(queryable.Provider);
            this.mockDbSet.As<IQueryable<SitePagePhoto>>().Setup(m => m.Expression).Returns(queryable.Expression);
            this.mockDbSet.As<IQueryable<SitePagePhoto>>().Setup(m => m.ElementType).Returns(queryable.ElementType);
            this.mockDbSet.As<IQueryable<SitePagePhoto>>().Setup(m => m.GetEnumerator()).Returns(() => queryable.GetEnumerator());

            this.mockDbSet.Setup(m => m.Find(It.IsAny<object[]>()))
                .Returns((object[] id) => this.dbSetData.FirstOrDefault(d => d.SitePagePhotoId == (int)id[0]));

            this.mockContext.Setup(ctx => ctx.SitePagePhoto).Returns(this.mockDbSet.Object);

            this.repo = new SitePagePhotoRepository(this.mockContext.Object);
        }

        [Fact]
        public void Create_ValidModel_AddsToContext()
        {
            var model = new SitePagePhoto();
            this.repo.Create(model);

            this.mockDbSet.Verify(m => m.Add(It.Is<SitePagePhoto>(p => p == model)), Times.Once);
            this.mockContext.Verify(ctx => ctx.SaveChanges(), Times.Once);
        }

        [Fact]
        public void SetDefaultPhoto_ValidPhotoId_SetsOnlySpecifiedPhotoAsDefault()
        {
            int id = 1;
            this.dbSetData.Add(new SitePagePhoto { SitePagePhotoId = id, SitePageId = 10, IsDefault = false });
            this.dbSetData.Add(new SitePagePhoto { SitePagePhotoId = 2, SitePageId = 10, IsDefault = true });

            this.repo.SetDefaultPhoto(id);

            Assert.True(this.dbSetData.First(p => p.SitePagePhotoId == id).IsDefault);
            Assert.False(this.dbSetData.First(p => p.SitePagePhotoId == 2).IsDefault);
        }

        [Fact]
        public void Delete_ValidPhotoId_RemovesFromContext()
        {
            int id = 1;
            var model = new SitePagePhoto { SitePagePhotoId = id };
            this.dbSetData.Add(model);

            var result = this.repo.Delete(id);

            Assert.True(result);
            this.mockDbSet.Verify(m => m.Remove(It.Is<SitePagePhoto>(p => p == model)), Times.Once);
            this.mockContext.Verify(ctx => ctx.SaveChanges(), Times.Once);
        }
    }
}