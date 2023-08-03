namespace WebPagePub.Web.Models
{
    public class AuthorItem
    {
        public AuthorItem(
            int authorId,
            string firstName,
            string lastName)
        {
            this.AuthorId = authorId;
            this.FirstName = firstName;
            this.LastName = lastName;
        }

        public string FirstName { get; private set; } = string.Empty;
        public string LastName { get; private set; } = string.Empty;
        public int AuthorId { get; private set; }

        public string FullName
        {
            get
            {
                return string.Format("{0} {1}", this.FirstName, this.LastName);
            }
        }
    }
}
