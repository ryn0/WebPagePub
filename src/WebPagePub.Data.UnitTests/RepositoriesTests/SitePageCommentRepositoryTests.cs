using System;
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
    public class SitePageCommentRepositoryTests
    {
        private readonly Mock<IApplicationDbContext> mockDbContext;
        private readonly Mock<DbSet<SitePageComment>> mockDbSet;
        private readonly SitePageCommentRepository repo;

        public SitePageCommentRepositoryTests()
        {
            this.mockDbContext = new Mock<IApplicationDbContext>();
            this.mockDbSet = new Mock<DbSet<SitePageComment>>();
            this.mockDbContext.Setup(m => m.SitePageComment).Returns(this.mockDbSet.Object);
            this.repo = new SitePageCommentRepository(this.mockDbContext.Object);
        }

        [Fact]
        public void Create_ValidModel_AddsToContextAndSaves()
        {
            var comment = new SitePageComment();

            this.repo.Create(comment);

            this.mockDbSet.Verify(m => m.Add(comment), Times.Once);
            this.mockDbContext.Verify(m => m.SaveChanges(), Times.Once);
        }

        [Fact]
        public void Get_ValidRequestId_ReturnsCorrectComment()
        {
            var requestId = Guid.NewGuid();
            var comment = new SitePageComment { RequestId = requestId };
            var data = new List<SitePageComment> { comment }.AsQueryable();

            var mockSet = new Mock<DbSet<SitePageComment>>();
            mockSet.As<IQueryable<SitePageComment>>().Setup(m => m.Provider).Returns(data.Provider);
            mockSet.As<IQueryable<SitePageComment>>().Setup(m => m.Expression).Returns(data.Expression);
            mockSet.As<IQueryable<SitePageComment>>().Setup(m => m.ElementType).Returns(data.ElementType);
            mockSet.As<IQueryable<SitePageComment>>().Setup(m => m.GetEnumerator()).Returns(data.GetEnumerator());

            var mockContext = new Mock<IApplicationDbContext>();
            mockContext.Setup(m => m.SitePageComment).Returns(mockSet.Object);

            var repo = new SitePageCommentRepository(mockContext.Object);

            var result = repo.Get(requestId);

            Assert.Equal(comment, result);
        }

    }
}