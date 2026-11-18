using WebPagePub.Services.Implementations;
using WebPagePub.Services.Models.Sponsors;
using Xunit;

namespace WebPagePub.WebApp.UnitTests
{
    public class SponsorFeedMappingTests
    {
        [Theory]
        [InlineData("https://sponsor.example/", "https://sponsor.example/")]
        [InlineData("http://sponsor.example/page?a=1", "http://sponsor.example/page?a=1")]
        [InlineData("  https://sponsor.example/  ", "https://sponsor.example/")]
        [InlineData("javascript:alert(1)", "")]
        [InlineData("JavaScript:alert(1)", "")]
        [InlineData("data:text/html,<script>alert(1)</script>", "")]
        [InlineData("/relative/path", "")]
        [InlineData("//sponsor.example/", "")]
        [InlineData("ftp://sponsor.example/", "")]
        [InlineData("file:///etc/passwd", "")]
        [InlineData("", "")]
        [InlineData("not a url", "")]
        public void SafeHttpUrlOrEmpty_KeepsOnlyAbsoluteHttp(string input, string expected)
        {
            Assert.Equal(expected, SponsorJsonClient.SafeHttpUrlOrEmpty(input));
        }

        [Fact]
        public void MapFeedItems_DropsUnsafeLinksAndNullItems()
        {
            var items = new List<SponsorCardItem>
            {
                new SponsorCardItem { Name = "Good", Link = "https://good.example/", ReviewLink = "https://reviews.example/good" },
                new SponsorCardItem { Name = "Bad", Link = "javascript:alert(document.cookie)", ReviewLink = "data:text/html,x" },
                null!,
            };

            var result = SponsorJsonClient.MapFeedItems(items);

            Assert.Equal(2, result.Count);
            Assert.Equal("https://good.example/", result[0].Link);
            Assert.Equal("https://reviews.example/good", result[0].ReviewLink);
            Assert.Equal("Bad", result[1].Name);
            Assert.Equal(string.Empty, result[1].Link);
            Assert.Equal(string.Empty, result[1].ReviewLink);
        }

        [Fact]
        public void MapFeedItems_HandlesNullFeed()
        {
            Assert.Empty(SponsorJsonClient.MapFeedItems(null!));
        }
    }
}
