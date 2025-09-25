using System;
using System.Net.Http;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using WebPagePub.Services.Interfaces;
using WebPagePub.Services.Models;

namespace WebPagePub.Services.Implementations
{
    public class CaptchaService : ICaptchaService
    {
        private readonly HttpClient http;
        private readonly CaptchaOptions options;

        public CaptchaService(IHttpClientFactory http, IOptions<CaptchaOptions> opts)
        {
            this.http = http.CreateClient();
            this.options = opts.Value;
        }

        public bool IsValid(HttpRequest request)
        {
            // posted captcha
            var posted = (request.Form["Captcha"].ToString() ?? string.Empty).Trim();
            if (string.IsNullOrEmpty(posted))
            {
                return false;
            }

            // context from form (preferred) or query (fallback) — mirrors your markup
            var ctx = (request.Form["CaptchaContext"].ToString() ?? string.Empty).Trim();
            if (string.IsNullOrEmpty(ctx))
            {
                ctx = (request.Query["ctx"].ToString() ?? string.Empty).Trim();
            }

            if (string.IsNullOrEmpty(ctx))
            {
                ctx = "default";
            }

            var keyCtx = $"CaptchaCode:{ctx}";
            var sess = request.HttpContext.Session;

            // read
            var expected = sess.GetString(keyCtx) ?? sess.GetString("CaptchaCode");

            // consume (one-time)
            sess.Remove(keyCtx);
            sess.Remove("CaptchaCode");

            // compare, case-insensitive
            return !string.IsNullOrEmpty(expected) &&
                   string.Equals(expected, posted, StringComparison.OrdinalIgnoreCase);
        }
    }
}
