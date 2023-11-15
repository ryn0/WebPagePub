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
    public class EmailSubscriptionRepositoryTests
    {
        private readonly Mock<IApplicationDbContext> dbContextMock;
        private readonly EmailSubscriptionRepository repository;
        private readonly List<EmailSubscription> emailSubscriptions;

        public EmailSubscriptionRepositoryTests()
        {
            this.dbContextMock = new Mock<IApplicationDbContext>();
            this.repository = new EmailSubscriptionRepository(this.dbContextMock.Object);

            this.emailSubscriptions = new List<EmailSubscription>
            {
                new EmailSubscription { EmailSubscriptionId = 1, Email = "test1@example.com" },
                new EmailSubscription { EmailSubscriptionId = 2, Email = "test2@example.com" }
            };

            var dbSetMock = new Mock<DbSet<EmailSubscription>>();

            // IQueryable setup
            var queryableList = this.emailSubscriptions.AsQueryable();
            dbSetMock.As<IQueryable<EmailSubscription>>().Setup(m => m.Provider).Returns(queryableList.Provider);
            dbSetMock.As<IQueryable<EmailSubscription>>().Setup(m => m.Expression).Returns(queryableList.Expression);
            dbSetMock.As<IQueryable<EmailSubscription>>().Setup(m => m.ElementType).Returns(queryableList.ElementType);
            dbSetMock.As<IQueryable<EmailSubscription>>().Setup(m => m.GetEnumerator()).Returns(() => queryableList.GetEnumerator());

            // Mock the Find method
            dbSetMock.Setup(db => db.Find(It.IsAny<int>())).Returns((int id) => this.emailSubscriptions
                .SingleOrDefault(x => x.EmailSubscriptionId == id));

            // Mock the Remove method
            dbSetMock.Setup(db => db.Remove(It.IsAny<EmailSubscription>()))
                .Callback<EmailSubscription>((entity) => this.emailSubscriptions
                .Remove(entity));

            // Set the mocked DbSet to the mocked DbContext
            this.dbContextMock.Setup(db => db.EmailSubscription).Returns(dbSetMock.Object);

            dbSetMock.Setup(m => m.Find(It.IsAny<object[]>())).Returns<object[]>(ids => this.emailSubscriptions.SingleOrDefault(x => x.EmailSubscriptionId == (int)ids[0]));
        }

        [Fact]
        public void Create_ShouldAddEmailSubscription()
        {
            var newEmailSubscription = new EmailSubscription { EmailSubscriptionId = 3, Email = "test3@example.com" };

            this.dbContextMock.Setup(db => db.EmailSubscription.Add(newEmailSubscription)).Verifiable();
            this.dbContextMock.Setup(db => db.SaveChanges()).Verifiable();

            var result = this.repository.Create(newEmailSubscription);

            Assert.Equal(newEmailSubscription, result);
            this.dbContextMock.Verify();
        }

        [Fact]
        public void Get_ById_ShouldReturnCorrectEmailSubscription()
        {
            var result = this.repository.Get(1);
            Assert.Equal("test1@example.com", result.Email);
        }

        [Fact]
        public void Get_ByEmail_ShouldReturnCorrectEmailSubscription()
        {
            var result = this.repository.Get("test1@example.com");
            Assert.Equal(1, result.EmailSubscriptionId);
        }

        [Fact]
        public void Update_ShouldUpdateEmailSubscription()
        {
            var updatedEmailSubscription = new EmailSubscription { EmailSubscriptionId = 1, Email = "updatedtest1@example.com" };

            this.dbContextMock.Setup(db => db.EmailSubscription.Update(updatedEmailSubscription)).Verifiable();
            this.dbContextMock.Setup(db => db.SaveChanges()).Verifiable();

            var result = this.repository.Update(updatedEmailSubscription);

            Assert.True(result);
            this.dbContextMock.Verify();
        }

        [Fact]
        public void Delete_ShouldDeleteEmailSubscription()
        {
            var emailSubscriptionToRemove = new EmailSubscription { EmailSubscriptionId = 1, Email = "test1@example.com" };

            // Mock the Find method to return the email subscription
            this.dbContextMock.Setup(db => db.EmailSubscription.Find(1)).Returns(emailSubscriptionToRemove);

            // Mock the Remove method to remove the email subscription
            this.dbContextMock.Setup(db => db.EmailSubscription.Remove(emailSubscriptionToRemove)).Verifiable();

            // Mock the SaveChanges method to simulate saving to the database
            this.dbContextMock.Setup(db => db.SaveChanges()).Verifiable();

            var result = this.repository.Delete(1);

            Assert.True(result);
            this.dbContextMock.Verify(db => db.EmailSubscription.Remove(emailSubscriptionToRemove), Times.Once());
            this.dbContextMock.Verify(db => db.SaveChanges(), Times.Once());
        }

        [Fact]
        public void GetAll_ShouldReturnAllEmailSubscriptions()
        {
            var result = this.repository.GetAll();

            Assert.Equal(2, result.Count);
        }
    }
}