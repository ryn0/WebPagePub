using System.Net;
using System.Net.Sockets;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;

namespace WebPagePub.WebApp.Helpers
{
    /// <summary>
    /// Brake on sign-in attempts (POST /account/login only). The account lockout alone stops guessing
    /// one account's password, but it lets one source keep trying forever (and keep the single admin
    /// locked out). Two limits apply together:
    /// per client address, about 10 tries per 10 minutes, and for the whole site about 300 per
    /// 10 minutes, so many addresses together still can't run an unlimited guessing campaign.
    /// Every other request passes through untouched.
    /// </summary>
    public static class SignInRateLimit
    {
        public const int PerClientLimit = 10;
        public const int SiteWideLimit = 300;
        public const string SignInPath = "/account/login";
        public const string LocalPartition = "local";
        public const string RejectedMessage = "Too many sign-in attempts. Please wait a few minutes and try again.";

        public static readonly TimeSpan Window = TimeSpan.FromMinutes(10);

        public static void Configure(RateLimiterOptions options)
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            options.GlobalLimiter = CreateLimiter();
            options.OnRejected = async (context, cancellationToken) =>
            {
                context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                context.HttpContext.Response.ContentType = "text/plain; charset=utf-8";
                context.HttpContext.Response.Headers.RetryAfter = ((int)Window.TotalSeconds).ToString();
                await context.HttpContext.Response.WriteAsync(RejectedMessage, cancellationToken);
            };
        }

        public static PartitionedRateLimiter<HttpContext> CreateLimiter()
        {
            var perClient = PartitionedRateLimiter.Create<HttpContext, string>(context =>
                IsSignInPost(context.Request)
                    ? RateLimitPartition.GetSlidingWindowLimiter(
                        "client:" + ClientKey(context.Connection.RemoteIpAddress),
                        _ => NewWindow(PerClientLimit))
                    : RateLimitPartition.GetNoLimiter(string.Empty));

            var siteWide = PartitionedRateLimiter.Create<HttpContext, string>(context =>
                IsSignInPost(context.Request)
                    ? RateLimitPartition.GetSlidingWindowLimiter("site", _ => NewWindow(SiteWideLimit))
                    : RateLimitPartition.GetNoLimiter(string.Empty));

            return PartitionedRateLimiter.CreateChained(perClient, siteWide);
        }

        public static bool IsSignInPost(HttpRequest request)
        {
            if (!HttpMethods.IsPost(request.Method))
            {
                return false;
            }

            // Routing also accepts a trailing slash and any letter case, so the check must too.
            var path = (request.Path.Value ?? string.Empty).TrimEnd('/');
            return string.Equals(path, SignInPath, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// The bucket a request counts against. Kestrel only listens on 127.0.0.1 and the forwarded
        /// headers step replaces that with the visitor's address, so a missing or loopback address means
        /// the real one is unknown; those all share one bucket rather than each getting a fresh one.
        /// IPv6 visitors usually hold a whole /64, so the /64 is the bucket.
        /// </summary>
        public static string ClientKey(IPAddress? address)
        {
            if (address == null || IPAddress.IsLoopback(address))
            {
                return LocalPartition;
            }

            if (address.IsIPv4MappedToIPv6)
            {
                address = address.MapToIPv4();
                if (IPAddress.IsLoopback(address))
                {
                    return LocalPartition;
                }
            }

            if (address.AddressFamily == AddressFamily.InterNetworkV6)
            {
                var bytes = address.GetAddressBytes();
                for (var i = 8; i < bytes.Length; i++)
                {
                    bytes[i] = 0;
                }

                return new IPAddress(bytes).ToString() + "/64";
            }

            return address.ToString();
        }

        private static SlidingWindowRateLimiterOptions NewWindow(int permits)
        {
            return new SlidingWindowRateLimiterOptions
            {
                PermitLimit = permits,
                Window = Window,
                SegmentsPerWindow = 10,
                QueueLimit = 0,
                AutoReplenishment = true,
            };
        }
    }
}
