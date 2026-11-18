using Newtonsoft.Json;

namespace WebPagePub.WebApp.Helpers
{
    /// <summary>
    /// Serializes structured data (JSON-LD) that pages print raw inside a script tag. Page text such as
    /// FAQ answers or titles can contain "&lt;/script&gt;", which would end the script tag early and let the
    /// rest run as page markup. EscapeHtml writes &lt;, &gt;, &amp;, quotes as \u escapes, which JSON readers
    /// (and search engines) decode back to the same text.
    /// </summary>
    public static class JsonLd
    {
        private static readonly JsonSerializerSettings Settings = new ()
        {
            StringEscapeHandling = StringEscapeHandling.EscapeHtml,
        };

        public static string Serialize(object? value)
        {
            return JsonConvert.SerializeObject(value, Settings);
        }
    }
}
