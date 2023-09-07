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
    public class ContentSnippetRepositoryTests
    {
        private readonly Mock<IApplicationDbContext> contextMock;
        private readonly ContentSnippetRepository repository;
        private readonly Mock<DbSet<ContentSnippet>> dbSetMock;

        public ContentSnippetRepositoryTests()
        {
            this.contextMock = new Mock<IApplicationDbContext>();
            this.dbSetMock = new Mock<DbSet<ContentSnippet>>();
            this.contextMock.Setup(c => c.ContentSnippet).Returns(this.dbSetMock.Object);
            this.repository = new ContentSnippetRepository(this.contextMock.Object);
        }

        [Fact]
        public void Create_ShouldAddContentSnippetAndReturnIt()
        {
            var snippet = new ContentSnippet { ContentSnippetId = 1 };
            this.contextMock.Setup(c => c.SaveChanges()).Returns(1);

            var result = this.repository.Create(snippet);

            Assert.Equal(snippet, result);
            this.dbSetMock.Verify(d => d.Add(It.IsAny<ContentSnippet>()), Times.Once);
        }

        [Fact]
        public void Get_ById_ShouldReturnCorrectContentSnippet()
        {
            var snippet = new ContentSnippet { ContentSnippetId = 1 };

            // Mock the DbSet's Find method to return our snippet
            this.dbSetMock.Setup(d => d.Find(It.IsAny<object[]>())).Returns<object[]>(ids => ids.Contains(1) ? snippet : null);

            var result = this.repository.Get(1);

            Assert.Equal(snippet, result);
        }

        [Fact]
        public void Update_ShouldModifyContentSnippetAndReturnTrue()
        {
            var snippet = new ContentSnippet { ContentSnippetId = 1 };
            this.contextMock.Setup(c => c.SaveChanges()).Returns(1);

            var result = this.repository.Update(snippet);

            Assert.True(result);
            this.dbSetMock.Verify(d => d.Update(It.IsAny<ContentSnippet>()), Times.Once);
        }

        [Fact]
        public void Delete_ShouldRemoveContentSnippetAndReturnTrue()
        {
            var snippet = new ContentSnippet { ContentSnippetId = 1 };
            var snippets = new List<ContentSnippet> { snippet };

            // Mock the DbSet to behave like a list
            this.dbSetMock.Setup(d => d.Find(It.IsAny<int>())).Returns(snippet);
            this.dbSetMock.Setup(d => d.Remove(snippet));
            this.contextMock.Setup(c => c.SaveChanges()).Returns(1);

            var result = this.repository.Delete(1);

            Assert.True(result);
            this.dbSetMock.Verify(d => d.Remove(It.IsAny<ContentSnippet>()), Times.Once);
            this.contextMock.Verify(c => c.SaveChanges(), Times.Once); // Verify SaveChanges was called
        }

        [Fact]
        public void GetAll_ShouldReturnAllContentSnippets()
        {
            var snippets = new List<ContentSnippet>
            {
                new ContentSnippet { ContentSnippetId = 1 },
                new ContentSnippet { ContentSnippetId = 2 }
            };

            // Mock the DbSet
            var mockSet = new Mock<DbSet<ContentSnippet>>();
            mockSet.As<IQueryable<ContentSnippet>>().Setup(m => m.Provider).Returns(snippets.AsQueryable().Provider);
            mockSet.As<IQueryable<ContentSnippet>>().Setup(m => m.Expression).Returns(snippets.AsQueryable().Expression);
            mockSet.As<IQueryable<ContentSnippet>>().Setup(m => m.ElementType).Returns(snippets.AsQueryable().ElementType);
            mockSet.As<IQueryable<ContentSnippet>>().Setup(m => m.GetEnumerator()).Returns(snippets.GetEnumerator());

            // Mock the database context to return the mock set
            var mockContext = new Mock<IApplicationDbContext>();
            mockContext.Setup(c => c.ContentSnippet).Returns(mockSet.Object);

            var repository = new ContentSnippetRepository(mockContext.Object);

            // Act
            var result = repository.GetAll();

            // Assert
            Assert.Equal(2, result.Count);
        }
    }
}