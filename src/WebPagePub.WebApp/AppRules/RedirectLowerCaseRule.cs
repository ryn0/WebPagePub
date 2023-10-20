using Microsoft.AspNetCore.Rewrite;
using Microsoft.Net.Http.Headers;
using System.Net;
using System.Text;

namespace WebPagePub.WebApp.AppRules
{
    public class RedirectLowerCaseRule : IRule
    {
        public int StatusCode { get; } = (int)HttpStatusCode.MovedPermanently;

        public void ApplyRule(RewriteContext context)
        {
            HttpRequest request = context.HttpContext.Request;
            PathString path = context.HttpContext.Request.Path;
            HostString host = context.HttpContext.Request.Host;

            if (path.HasValue && path.Value.Any(char.IsUpper) || host.HasValue && host.Value.Any(char.IsUpper))
            {
                HttpResponse response = context.HttpContext.Response;
                response.StatusCode = StatusCode;
                var sb = new StringBuilder(request.Scheme.ToLower());
                sb.Append("://");
                sb.Append(host.Value.ToLower());
                if (!string.IsNullOrEmpty(request.PathBase.Value))
                {
                    sb.Append(request.PathBase.Value.ToLower());
                }

                if (!string.IsNullOrEmpty(request.Path.Value))
                {
                    sb.Append(request.Path.Value.ToLower());
                }
                sb.Append(request.QueryString);

                response.Headers[HeaderNames.Location] = sb.ToString();

                context.Result = RuleResult.EndResponse;
            }
            else
            {
                context.Result = RuleResult.ContinueRules;
            }
        }
    }
}
