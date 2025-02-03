namespace WebPagePub.Web.Models
{
    public class EmailSubscribeEditListModel
    {
        public EmailSubscribeEditListModel()
        {
            this.Items = new List<EmailSubscribeEditModel>();
        }

        public List<EmailSubscribeEditModel> Items { get; set; }

        // Paging Properties
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int TotalItems { get; set; }
        public int TotalPages { get; set; }

        // Current page helpers
        public bool HasPreviousPage => this.PageNumber > 1;
        public bool HasNextPage => this.PageNumber < this.TotalPages;

        // Unsubscribe link
        public string UnsubscribeLink { get; set; }
    }
}