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
    public class AuthorRepositoryTests
    {
        private readonly Mock<IApplicationDbContext> contextMock;
        private readonly AuthorRepository repository;

        public AuthorRepositoryTests()
        {
            this.contextMock = new Mock<IApplicationDbContext>();
            this.repository = new AuthorRepository(this.contextMock.Object);
        }

        [Fact]
        public void Create_ShouldAddAuthorAndReturnIt()
        {
            var author = new Author { /* initialization */ FirstName = "Joe" };

            // Mock the method used to add an Author to your context. Adjust accordingly.
            this.contextMock.Setup(c => c.Author.Add(It.IsAny<Author>()));

            // Mock the SaveChanges method to be trackable
            this.contextMock.Setup(c => c.SaveChanges()).Returns(1);  // Assuming SaveChanges returns an int which is the number of entities written to the database

            var result = this.repository.Create(author);

            Assert.Equal(author, result);
            this.contextMock.Verify(c => c.Author.Add(author), Times.Once);
            this.contextMock.Verify(c => c.SaveChanges(), Times.Once);  // Verify SaveChanges is called once
        }

        [Fact]
        public void Update_ShouldModifyAuthorAndReturnTrue()
        {
            var author = new Author { /* initialization */ };

            // Mock the method used to update an Author in your context. Adjust accordingly.
            this.contextMock.Setup(c => c.Author.Update(It.IsAny<Author>()));

            // Mock the SaveChanges method to be trackable
            this.contextMock.Setup(c => c.SaveChanges()).Returns(1);

            var result = this.repository.Update(author);

            Assert.True(result);
            this.contextMock.Verify(c => c.Author.Update(author), Times.Once);
            this.contextMock.Verify(c => c.SaveChanges(), Times.Once);
        }

        [Fact]
        public void GetAll_ShouldReturnAllAuthors()
        {
            var authors = new List<Author>
            {
                new Author() { FirstName = "Joe" },
                new Author() { FirstName = "John" }
            };
            var queryableAuthors = authors.AsQueryable();

            // Setup the DbSet to return the IQueryable
            var dbSetMock = new Mock<DbSet<Author>>();
            dbSetMock.As<IQueryable<Author>>().Setup(m => m.Provider).Returns(queryableAuthors.Provider);
            dbSetMock.As<IQueryable<Author>>().Setup(m => m.Expression).Returns(queryableAuthors.Expression);
            dbSetMock.As<IQueryable<Author>>().Setup(m => m.ElementType).Returns(queryableAuthors.ElementType);
            dbSetMock.As<IQueryable<Author>>().Setup(m => m.GetEnumerator()).Returns(queryableAuthors.GetEnumerator());

            // Setup the context to return the mocked DbSet
            this.contextMock.Setup(c => c.Author).Returns(dbSetMock.Object);

            var result = this.repository.GetAll();

            Assert.Equal(authors, result);
        }
    }
}