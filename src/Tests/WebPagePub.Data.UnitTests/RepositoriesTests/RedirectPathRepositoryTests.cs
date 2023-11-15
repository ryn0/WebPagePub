using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Moq;
using WebPagePub.Data.DbContextInfo.Interfaces;
using WebPagePub.Data.Models.Db;
using WebPagePub.Data.Repositories.Implementations;
using Xunit;

namespace WebPagePub.Data.UnitTests.RepositoriesTests
{
    public class RedirectPathRepositoryTests : IDisposable
    {
        private readonly Mock<IApplicationDbContext> dbContextMock;
        private readonly RedirectPathRepository repository;
        private readonly List<RedirectPath> redirectPaths;

        public RedirectPathRepositoryTests()
        {
            this.dbContextMock = new Mock<IApplicationDbContext>();
            this.repository = new RedirectPathRepository(this.dbContextMock.Object);

            this.redirectPaths = new List<RedirectPath>
            {
                new RedirectPath { RedirectPathId = 1, Path = "/test1" },
                new RedirectPath { RedirectPathId = 2, Path = "/test2" }
            };

            var dbSetMock = new Mock<DbSet<RedirectPath>>();
            dbSetMock.As<IQueryable<RedirectPath>>().Setup(m => m.Provider).Returns(this.redirectPaths.AsQueryable().Provider);
            dbSetMock.As<IQueryable<RedirectPath>>().Setup(m => m.Expression).Returns(this.redirectPaths.AsQueryable().Expression);
            dbSetMock.As<IQueryable<RedirectPath>>().Setup(m => m.ElementType).Returns(this.redirectPaths.AsQueryable().ElementType);
            dbSetMock.As<IQueryable<RedirectPath>>().Setup(m => m.GetEnumerator()).Returns(this.redirectPaths.GetEnumerator());

            dbSetMock.Setup(d => d.Add(It.IsAny<RedirectPath>())).Callback<RedirectPath>((s) => this.redirectPaths.Add(s));
            dbSetMock.Setup(d => d.Find(It.IsAny<int>())).Returns((int id) => this.redirectPaths.SingleOrDefault(x => x.RedirectPathId == id));

            this.dbContextMock.Setup(x => x.RedirectPath).Returns(dbSetMock.Object);

            dbSetMock.Setup(d => d.Remove(It.IsAny<RedirectPath>()))
                .Callback<RedirectPath>((s) => this.redirectPaths.RemoveAll(x => x.RedirectPathId == s.RedirectPathId));

            this.dbContextMock.Setup(x => x.SaveChanges()).Returns(1); // Mock the SaveChanges to return 1 (indicating one row affected).
            dbSetMock.Setup(d => d.Find(It.IsAny<int>())).Returns((int id) => this.redirectPaths
                .SingleOrDefault(x => x.RedirectPathId == id));
            dbSetMock.Setup(d => d.Find(It.IsAny<int>())).Returns((object[] args) =>
            {
                var id = (int)args[0];
                return this.redirectPaths.SingleOrDefault(x => x.RedirectPathId == id);
            });
        }

        [Fact]
        public void Create_AddsRedirectPath_ReturnsModel()
        {
            var newRedirectPath = new RedirectPath { Path = "/test3" };

            var result = this.repository.Create(newRedirectPath);

            Assert.Contains(result, this.redirectPaths);
        }

        [Fact]
        public void Delete_ValidId_ReturnsTrue()
        {
            var result = this.repository.Delete(1);

            Assert.True(result);
            Assert.DoesNotContain(this.redirectPaths, x => x.RedirectPathId == 1);
        }

        [Fact]
        public void Get_ById_ReturnsRedirectPath()
        {
            var result = this.repository.Get(1);

            Assert.NotNull(result);
            Assert.Equal("/test1", result.Path);
        }

        [Fact]
        public void Get_ByPath_ReturnsRedirectPath()
        {
            var result = this.repository.Get("/test1");

            Assert.NotNull(result);
            Assert.Equal(1, result.RedirectPathId);
        }

        [Fact]
        public void GetAll_ReturnsAllRedirectPaths()
        {
            var result = this.repository.GetAll();

            Assert.Equal(2, result.Count);
        }

        public void Dispose()
        {
            this.repository.Dispose();
        }
    }
}