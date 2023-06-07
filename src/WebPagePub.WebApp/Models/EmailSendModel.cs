namespace WebPagePub.Web.Models
{
    public class EmailSendModel
    {
        public string SendToEmails { get; set; } = default!;

        public string EmailTitle { get; set; } = default!;

        public string EmailMessage { get; set; } = default!;
    }
}
