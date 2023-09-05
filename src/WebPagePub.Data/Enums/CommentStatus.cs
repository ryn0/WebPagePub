namespace WebPagePub.Data.Enums
{

    public enum CommentStatus : byte
    {
        Unknown = 0,
        AwaitingModeration = 1,
        Rejected = 2,
        Approved = 3,
        Removed = 4,
        Spam = 5
    }
}