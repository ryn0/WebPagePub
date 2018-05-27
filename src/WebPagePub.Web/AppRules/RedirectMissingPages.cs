using Microsoft.AspNetCore.Rewrite;
using System.Collections.Generic;

namespace WebPagePub.Web.AppRules
{
    public class RedirectMissingPages : IRule
    {
        private Dictionary<string, string> _pathRedirects = new Dictionary<string, string>();

        public RedirectMissingPages(Dictionary<string, string> pathRedirects)
        {
           foreach(var pathRedirect in pathRedirects)
            {
                var key = pathRedirect.Key;

                if (key.StartsWith("/"))
                    key = key.TrimStart('/');

                if (key.EndsWith("/"))
                    key = key.TrimEnd('/');

                if (_pathRedirects.ContainsKey(key))
                    continue;

                _pathRedirects.Add(key, pathRedirect.Value);
            }
        }

        public void ApplyRule(RewriteContext context)
        {
            var req = context.HttpContext.Request;

            var newPath = RedirectOldRequests(req.Path);

            if (string.IsNullOrWhiteSpace(newPath))
                return;

            context.HttpContext.Response.Redirect(newPath, true);
            context.Result = RuleResult.EndResponse;
        }


        private string RedirectOldRequests(string key)
        {
            if (_pathRedirects.ContainsKey(key))
            {
                return _pathRedirects[key];
            }

            return null;
        }
    }
}