namespace WebPagePub.ChatCommander.Helpers
{
    public class WebPageChecker
    {
        private static readonly HttpClient client = new();

        public static async Task<bool> IsWebPageOnlineAsync(Uri uri)
        {
            try
            {
                HttpResponseMessage response = await client.GetAsync(uri);

                var responseCode = response.StatusCode;

                if (responseCode == System.Net.HttpStatusCode.Moved ||
                    response?.RequestMessage?.RequestUri != uri)
                {
                    return false;
                }

                return response.IsSuccessStatusCode;
            }
            catch (HttpRequestException)
            {
                return false;
            }
        }
    }
}