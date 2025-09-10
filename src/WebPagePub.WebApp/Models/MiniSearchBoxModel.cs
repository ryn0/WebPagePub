 

namespace WebPagePub.WebApp.Models
{
    public class MiniSearchBoxModel
    {
        public string Controller { get; set; } = "SitePageSearch";
        public string Action { get; set; } = "Index";
        public string QueryName { get; set; } = "term";   // input name expected by your action
        public string? Placeholder { get; set; } = "Search pages…";
        public string ButtonText { get; set; } = "Search";
        public string? InitialValue { get; set; }         // to persist the term if you want
        public string? CssClass { get; set; }             // extra classes for the wrapper
    }
}
