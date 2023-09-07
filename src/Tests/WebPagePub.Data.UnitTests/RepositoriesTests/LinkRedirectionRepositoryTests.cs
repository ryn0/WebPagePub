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
    public class LinkRedirectionRepositoryTests
    {
        private readonly Mock<IApplicationDbContext> dbContextMock;
        private readonly LinkRedirectionRepository repository;
        private readonly List<LinkRedirection> linkRedirections;

        public LinkRedirectionRepositoryTests()
        {
            this.dbContextMock = new Mock<IApplicationDbContext>();
            this.repository = new LinkRedirectionRepository(this.dbContextMock.Object);
            this.linkRedirections = new List<LinkRedirection>
            {
                new LinkRedirection { LinkRedirectionId = 1, LinkKey = "testKey1", UrlDestination = "http://example.com/1" },
                new LinkRedirection { LinkRedirectionId = 2, LinkKey = "testKey2", UrlDestination = "http://example.com/2" }
            };

            var dbSetMock = new Mock<DbSet<LinkRedirection>>();
            dbSetMock.As<IQueryable<LinkRedirection>>().Setup(m => m.Provider).Returns(this.linkRedirections.AsQueryable().Provider);
            dbSetMock.As<IQueryable<LinkRedirection>>().Setup(m => m.Expression).Returns(this.linkRedirections.AsQueryable().Expression);
            dbSetMock.As<IQueryable<LinkRedirection>>().Setup(m => m.ElementType).Returns(this.linkRedirections.AsQueryable().ElementType);
            dbSetMock.As<IQueryable<LinkRedirection>>().Setup(m => m.GetEnumerator()).Returns(this.linkRedirections.GetEnumerator());
            dbSetMock.Setup(d => d.Find(It.IsAny<int>())).Returns((int id) => this.linkRedirections.SingleOrDefault(x => x.LinkRedirectionId == id));
            dbSetMock.Setup(d => d.Add(It.IsAny<LinkRedirection>())).Callback<LinkRedirection>((s) => this.linkRedirections.Add(s));
            dbSetMock.Setup(d => d.Remove(It.IsAny<LinkRedirection>()))
             .Callback<LinkRedirection>((s) => this.linkRedirections.RemoveAll(x => x.LinkRedirectionId == s.LinkRedirectionId));

            this.dbContextMock.Setup(x => x.LinkRedirection).Returns(dbSetMock.Object);

            dbSetMock.Setup(d => d.Remove(It.IsAny<LinkRedirection>())).Callback<LinkRedirection>((s) => this.linkRedirections.Remove(s));

            dbSetMock.Setup(d => d.Find(It.IsAny<object[]>())).Returns((object[] args) =>
            {
                var id = (int)args[0];
                return this.linkRedirections.SingleOrDefault(x => x.LinkRedirectionId == id);
            });

            dbSetMock.Setup(d => d.Update(It.IsAny<LinkRedirection>()))
            .Callback<LinkRedirection>((updatedEntity) =>
            {
                var existingEntity = this.linkRedirections.SingleOrDefault(x => x.LinkRedirectionId == updatedEntity.LinkRedirectionId);
                if (existingEntity != null)
                {
                    existingEntity.LinkKey = updatedEntity.LinkKey;
                    existingEntity.UrlDestination = updatedEntity.UrlDestination;

                    // ... Update any other necessary properties
                }
            });
        }

        [Fact]
        public void Create_ShouldAddLinkRedirection()
        {
            var newLinkRedirection = new LinkRedirection
            {
                LinkRedirectionId = 3,
                LinkKey = "testKey3",
                UrlDestination = "http://example.com/3"
            };

            var result = this.repository.Create(newLinkRedirection);

            Assert.Equal(newLinkRedirection, result);
            Assert.Contains(newLinkRedirection, this.linkRedirections);
        }

        [Fact]
        public void Get_ById_ShouldReturnCorrectLinkRedirection()
        {
            var result = this.repository.Get(1);
            Assert.Equal("testKey1", result.LinkKey);
        }

        [Fact]
        public void Get_ByLinkKey_ShouldReturnCorrectLinkRedirection()
        {
            var result = this.repository.Get("testKey1");
            Assert.Equal(1, result.LinkRedirectionId);
        }

        [Fact]
        public void Update_ShouldUpdateLinkRedirection()
        {
            var updatedLinkRedirection = new LinkRedirection
            {
                LinkRedirectionId = 1,
                LinkKey = "updatedTestKey",
                UrlDestination = "http://example.com/updated"
            };

            var result = this.repository.Update(updatedLinkRedirection);

            Assert.True(result);
            Assert.Equal("updatedTestKey", this.linkRedirections.First(x => x.LinkRedirectionId == 1).LinkKey);
        }

        [Fact]
        public void Delete_ShouldDeleteLinkRedirection()
        {
            var entityToDelete = this.linkRedirections.SingleOrDefault(x => x.LinkRedirectionId == 1);
            Assert.NotNull(entityToDelete); // Initial check

            this.repository.Delete(1); // Invoke the Delete method

            Assert.DoesNotContain(this.linkRedirections, x => x.LinkRedirectionId == 1); // Assert after deletion
        }

        [Fact]
        public void GetAll_ShouldReturnAllLinkRedirections()
        {
            var result = this.repository.GetAll();

            Assert.Equal(2, result.Count);
        }
    }
}