using WebPagePub.Data.Constants;

namespace WebPagePub.WebApp.Models.Author
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

        public string FirstName { get; private set; } = StringConstants.DefaultAuthorName;
        public string LastName { get; private set; } = StringConstants.DefaultAuthorName;
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
