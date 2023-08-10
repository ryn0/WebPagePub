namespace WebPagePub.Web.Models
{
    public class LinkListModel
    {
        public List<LinkEditModel> Items { get; set; } = new List<LinkEditModel>();

        public LinkEditModel? NewestLink { get; set; }

        public List<char> UniqueFirstLetters { get; set; } = new List<char>();
    }
}
