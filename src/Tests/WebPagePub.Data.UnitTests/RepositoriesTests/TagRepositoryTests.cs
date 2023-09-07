using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Moq;
using WebPagePub.Data.DbContextInfo;
using WebPagePub.Data.Models;
using WebPagePub.Data.Repositories.Implementations;
using Xunit;

namespace WebPagePub.Data.UnitTests.RepositoriesTests
{
    public class TagRepositoryTests
    {
        private readonly TagRepository repository;
        private readonly Mock<IApplicationDbContext> contextMock;

        public TagRepositoryTests()
        {
            this.contextMock = new Mock<IApplicationDbContext>();
            this.repository = new TagRepository(this.contextMock.Object);
        }

        [Fact]
        public void Create_ValidTag_ReturnsTag()
        {
            // Arrange
            var tag = new Tag();
            var mockTagSet = new Mock<DbSet<Tag>>();

            // You just want to verify that Add was called, not its return.
            mockTagSet.Setup(m => m.Add(It.IsAny<Tag>())).Callback<Tag>(t => tag = t);
            this.contextMock.Setup(c => c.Tag).Returns(mockTagSet.Object);
            this.contextMock.Setup(c => c.SaveChanges()).Verifiable();

            // Act
            var result = this.repository.Create(tag);

            // Assert
            Assert.Equal(tag, result);
            this.contextMock.Verify(c => c.SaveChanges(), Times.Once);  // Verifies SaveChanges() was called
            mockTagSet.Verify(m => m.Add(It.IsAny<Tag>()), Times.Once);  // Verifies Add() was called
        }

        [Fact]
        public void Get_ById_ReturnsTag()
        {
            // Arrange
            var tagId = 1;
            var tag = new Tag { TagId = tagId };
            this.contextMock.Setup(c => c.Tag.Find(tagId)).Returns(tag);

            // Act
            var result = this.repository.Get(tagId);

            // Assert
            Assert.Equal(tag, result);
        }

        [Fact]
        public void Get_ByKey_ReturnsTag()
        {
            // Arrange
            var key = "testKey";
            var tag = new Tag { Key = key };
            var tags = new List<Tag> { tag }.AsQueryable();

            var mockSet = new Mock<DbSet<Tag>>();

            mockSet.As<IQueryable<Tag>>().Setup(m => m.Provider).Returns(tags.Provider);
            mockSet.As<IQueryable<Tag>>().Setup(m => m.Expression).Returns(tags.Expression);
            mockSet.As<IQueryable<Tag>>().Setup(m => m.ElementType).Returns(tags.ElementType);
            mockSet.As<IQueryable<Tag>>().Setup(m => m.GetEnumerator()).Returns(tags.GetEnumerator());

            this.contextMock.Setup(c => c.Tag).Returns(mockSet.Object);

            // Act
            var result = this.repository.Get(key);

            // Assert
            Assert.Equal(tag, result);
        }


        [Fact]
        public void Update_ValidTag_ReturnsTrue()
        {
            // Arrange
            var tag = new Tag();
            this.contextMock.Setup(c => c.Tag.Update(tag)).Verifiable();
            this.contextMock.Setup(c => c.SaveChanges()).Verifiable();

            // Act
            var result = this.repository.Update(tag);

            // Assert
            Assert.True(result);
            this.contextMock.Verify();
        }

        [Fact]
        public void Delete_ValidId_ReturnsTrue()
        {
            // Arrange
            var tagId = 1;
            var tag = new Tag { TagId = tagId };
            this.contextMock.Setup(c => c.Tag.Find(tagId)).Returns(tag);
            this.contextMock.Setup(c => c.Tag.Remove(tag)).Verifiable();
            this.contextMock.Setup(c => c.SaveChanges()).Verifiable();

            // Act
            var result = this.repository.Delete(tagId);

            // Assert
            Assert.True(result);
            this.contextMock.Verify();
        }

        [Fact]
        public void GetAll_ReturnsAllTags()
        {
            // Arrange
            var tagsList = new List<Tag> { new Tag(), new Tag() };
            var tags = tagsList.AsQueryable();

            var mockSet = new Mock<DbSet<Tag>>();

            mockSet.As<IQueryable<Tag>>().Setup(m => m.Provider).Returns(tags.Provider);
            mockSet.As<IQueryable<Tag>>().Setup(m => m.Expression).Returns(tags.Expression);
            mockSet.As<IQueryable<Tag>>().Setup(m => m.ElementType).Returns(tags.ElementType);
            mockSet.As<IQueryable<Tag>>().Setup(m => m.GetEnumerator()).Returns(tags.GetEnumerator());

            this.contextMock.Setup(c => c.Tag).Returns(mockSet.Object);

            // Act
            var result = this.repository.GetAll();

            // Assert
            Assert.Equal(tagsList, result);
        }
    }
}