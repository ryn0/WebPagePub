using WebPagePub.Data.Constants;

namespace WebPagePub.Web.Helpers
{
    public static partial class RequestExtensions
    {
        public static string UserAgent(this HttpRequest request)
        {
            if (request?.Headers == null)
            {
                return string.Empty;
            }

            if (request.Headers.TryGetValue(StringConstants.UserAgent, out var userAgentValue))
            {
                return userAgentValue.ToString();
            }

            return string.Empty;
        }
    }
}