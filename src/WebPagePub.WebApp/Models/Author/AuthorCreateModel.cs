namespace WebPagePub.WebApp.Models.Author
{
    public class AuthorCreateModel
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string? AuthorBio { get; set; } = default;
    }
}
