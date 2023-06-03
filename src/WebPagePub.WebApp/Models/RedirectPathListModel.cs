using System.Collections.Generic;

namespace WebPagePub.Web.Models
{
    public class RedirectPathListModel
    {
        public List<RedirectPathItemModel> Items { get; set; } = new List<RedirectPathItemModel>();

        public class RedirectPathItemModel
        {
            public string Path { get; set; }

            public string PathDestination { get; set; }
            public int RedirectPathId { get; set; }
        }
    }
}
