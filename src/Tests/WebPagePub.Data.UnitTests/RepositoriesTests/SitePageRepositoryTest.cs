using System;
using System.Collections.Generic;
using System.Linq;
using Moq;
using WebPagePub.Data.DbContextInfo;
using WebPagePub.Data.Models;
using WebPagePub.Data.Repositories.Interfaces;
using Xunit;

namespace WebPagePub.Data.UnitTests.RepositoriesTests
{
    public class SitePageRepositoryTest
    {
        private readonly Mock<ISitePageRepository> repoMock;
        private readonly SitePage testSitePage;

        public SitePageRepositoryTest()
        {
            this.repoMock = new Mock<ISitePageRepository>();
            this.testSitePage = new SitePage
            {
                SitePageId = 1,
                Title = "Test Title"
            };
        }

        [Fact]
        public void Get_SitePageId_ReturnsSitePage()
        {
            this.repoMock.Setup(r => r.Get(It.IsAny<int>())).Returns(this.testSitePage);

            var result = this.repoMock.Object.Get(1);

            Assert.Equal(this.testSitePage.SitePageId, result.SitePageId);
        }

        [Fact]
        public void GetLivePagesForSection_ReturnsListOfSitePages()
        {
            var testList = new List<SitePage> { this.testSitePage };
            this.repoMock.Setup(r => r.GetLivePagesForSection(It.IsAny<int>())).Returns(testList);

            var result = this.repoMock.Object.GetLivePagesForSection(1);

            Assert.Single(result);
            Assert.Equal(this.testSitePage.SitePageId, result.First().SitePageId);
        }
    }
}
