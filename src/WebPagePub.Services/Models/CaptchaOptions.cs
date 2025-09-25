
using WebPagePub.Services.Enums;

namespace WebPagePub.Services.Models
{
    public class CaptchaOptions
    {
        public CaptchaProvider Provider { get; set; } = CaptchaProvider.Turnstile;
        public string SiteKey { get; set; } = string.Empty;
        public string SecretKey { get; set; } = string.Empty;
        public bool FailOpenForLocalhost { get; set; } = true; // helps during local dev
    }
}
