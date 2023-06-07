using Microsoft.AspNetCore.Rewrite;

namespace WebPagePub.Web.AppRules
{
    public class RedirectMissingPages : IRule
    {
        private readonly Dictionary<string, string> pathRedirects = new Dictionary<string, string>();

        public RedirectMissingPages(Dictionary<string, string> pathRedirects)
        {
           foreach (var pathRedirect in pathRedirects)
            {
                var resolvedKey = this.ResolveKey(pathRedirect.Key);

                if (this.pathRedirects.ContainsKey(resolvedKey))
                {
                    continue;
                }

                this.pathRedirects.Add(resolvedKey, pathRedirect.Value);
            }
        }

        public void ApplyRule(RewriteContext context)
        {
            var req = context.HttpContext.Request;

            var newPath = this.MapOldPathToNewPath(req.Path);

            if (string.IsNullOrWhiteSpace(newPath))
            {
                return;
            }

            context.HttpContext.Response.Redirect(newPath, true);
            context.Result = RuleResult.EndResponse;
        }

        private string MapOldPathToNewPath(string key)
        {
            var resolvedKey = this.ResolveKey(key);

            if (this.pathRedirects.ContainsKey(resolvedKey))
            {
                return this.pathRedirects[resolvedKey];
            }

            return null;
        }

        private string ResolveKey(string key)
        {
            if (key.StartsWith("/"))
            {
                key = key.TrimStart('/');
            }

            if (key.EndsWith("/"))
            {
                key = key.TrimEnd('/');
            }

            return key;
        }
    }
}