using Microsoft.AspNetCore.Http;

namespace WebPagePub.Services.Interfaces
{
    public interface ICaptchaService
    {
        bool IsValid(HttpRequest request);
    }
}
