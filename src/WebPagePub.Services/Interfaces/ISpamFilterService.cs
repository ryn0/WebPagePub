namespace WebPagePub.Services.Interfaces
{
    public interface ISpamFilterService
    {
        bool IsBlocked(string ipAddress);
        void Create(string ipAddress);
    }
}
