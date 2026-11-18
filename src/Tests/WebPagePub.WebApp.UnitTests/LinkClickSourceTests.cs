using WebPagePub.Web.Controllers;
using Xunit;

namespace WebPagePub.WebApp.UnitTests
{
    /// <summary>The outgoing-link click log should count readers, not scanners.</summary>
    public class LinkClickSourceTests
    {
        private const string Host = "example-site.com";

        [Theory]
        [InlineData("https://example-site.com/")]
        [InlineData("https://example-site.com/reviews/some-product")]
        [InlineData("https://EXAMPLE-site.com/articles/a-guide?x=1")]
        public void ClickFromOneOfOurPages_IsAReader(string referrer)
        {
            Assert.True(LinkController.CameFromOneOfOurPages(referrer, Host));
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        [InlineData("https://other-site.com/page")]
        [InlineData("https://example-site.com/search?hl=en&q=testing")]
        [InlineData("https://example-site.com/go/some-link")]
        [InlineData("https://example-site.com/search?q=testing'\"")]
        [InlineData("if(now()=sysdate(),sleep(15),0)")]
        public void DirectHitsAndScannerProbes_AreAutomated(string referrer)
        {
            Assert.False(LinkController.CameFromOneOfOurPages(referrer, Host));
        }
    }
}
