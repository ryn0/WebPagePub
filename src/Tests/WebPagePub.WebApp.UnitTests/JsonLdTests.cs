using Newtonsoft.Json.Linq;
using WebPagePub.WebApp.Helpers;
using Xunit;

namespace WebPagePub.WebApp.UnitTests
{
    public class JsonLdTests
    {
        [Fact]
        public void Serialize_CannotCloseTheScriptTag()
        {
            var text = "Answer </script><script>alert(1)</script> & more";
            var json = JsonLd.Serialize(new Dictionary<string, object> { ["text"] = text });

            Assert.DoesNotContain("<", json);
            Assert.DoesNotContain(">", json);
            Assert.DoesNotContain("&", json);

            // Readers still get the original text back.
            Assert.Equal(text, JObject.Parse(json)["text"]!.Value<string>());
        }
    }
}
