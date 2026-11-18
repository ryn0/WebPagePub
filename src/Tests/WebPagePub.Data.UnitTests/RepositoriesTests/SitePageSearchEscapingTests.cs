using WebPagePub.Data.Repositories.Implementations;
using Xunit;

namespace WebPagePub.Data.UnitTests.RepositoriesTests
{
    public class SitePageSearchEscapingTests
    {
        [Theory]
        [InlineData("monero", "monero")]
        [InlineData("100%", "100\\%")]
        [InlineData("%", "\\%")]
        [InlineData("my_page", "my\\_page")]
        [InlineData("a\\b", "a\\\\b")]
        [InlineData("\\%_", "\\\\\\%\\_")]
        [InlineData("", "")]
        [InlineData(null, "")]
        public void EscapeLikeTerm_MakesWildcardsLiteral(string input, string expected)
        {
            Assert.Equal(expected, SitePageRepository.EscapeLikeTerm(input));
        }
    }
}
