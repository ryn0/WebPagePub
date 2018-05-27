using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Rewrite;
using System.Text;

namespace WebPagePub.Web.AppRules
{
    public class NonWwwRule : IRule
    {
        public void ApplyRule(RewriteContext context)
        {
            var req = context.HttpContext.Request;
            var currentHost = req.Host;
            if (currentHost.Host.StartsWith("www."))
            {
                var newHost = new HostString(currentHost.Host.Substring(4), currentHost.Port ?? 443);
                var newUrl = new StringBuilder().Append("https://").Append(newHost).Append(req.PathBase).Append(req.Path).Append(req.QueryString);
                context.HttpContext.Response.Redirect(newUrl.ToString(), true);
                context.Result = RuleResult.EndResponse;
            }
        }
    }
}
