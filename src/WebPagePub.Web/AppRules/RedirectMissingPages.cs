using Microsoft.AspNetCore.Rewrite;
using System;
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
                var resolvedKey = ResolveKey(pathRedirect.Key);
          
                if (_pathRedirects.ContainsKey(resolvedKey))
                    continue;

                _pathRedirects.Add(resolvedKey, pathRedirect.Value);
            }
        }

        public void ApplyRule(RewriteContext context)
        {
            var req = context.HttpContext.Request;

            var newPath = MapOldPathToNewPath(req.Path);

            if (string.IsNullOrWhiteSpace(newPath))
                return;

            context.HttpContext.Response.Redirect(newPath, true);
            context.Result = RuleResult.EndResponse;
        }

        private string MapOldPathToNewPath(string key)
        {
            var resolvedKey = ResolveKey(key);

            if (_pathRedirects.ContainsKey(resolvedKey))
            {
                return _pathRedirects[resolvedKey];
            }

            return null;
        }


        private string ResolveKey(string key)
        {
            if (key.StartsWith("/"))
                key = key.TrimStart('/');

            if (key.EndsWith("/"))
                key = key.TrimEnd('/');

            return key;
        }
    }
}