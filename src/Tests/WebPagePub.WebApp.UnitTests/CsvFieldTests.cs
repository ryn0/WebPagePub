using WebPagePub.WebApp.Helpers;
using Xunit;

namespace WebPagePub.WebApp.UnitTests
{
    public class CsvFieldTests
    {
        [Theory]
        [InlineData("https://example.com/a", "\"https://example.com/a\"")]
        [InlineData("", "\"\"")]
        [InlineData(null, "\"\"")]
        [InlineData("a,b", "\"a,b\"")]
        [InlineData("say \"hi\"", "\"say \"\"hi\"\"\"")]
        [InlineData("line1\nline2", "\"line1\nline2\"")]
        [InlineData("=HYPERLINK(\"http://x\")", "\"'=HYPERLINK(\"\"http://x\"\")\"")]
        [InlineData("+1+1", "\"'+1+1\"")]
        [InlineData("-2", "\"'-2\"")]
        [InlineData("@SUM(A1)", "\"'@SUM(A1)\"")]
        [InlineData("\tcmd", "\"'\tcmd\"")]
        [InlineData("\rcmd", "\"'\rcmd\"")]
        [InlineData("a=b", "\"a=b\"")]
        public void Escape_QuotesAndNeutralizesFormulas(string? input, string expected)
        {
            Assert.Equal(expected, CsvField.Escape(input));
        }

        [Fact]
        public void Row_JoinsEscapedFields()
        {
            Assert.Equal("\"1.2.3.4\",\"'=cmd\",\"x,y\"", CsvField.Row("1.2.3.4", "=cmd", "x,y"));
        }
    }
}
