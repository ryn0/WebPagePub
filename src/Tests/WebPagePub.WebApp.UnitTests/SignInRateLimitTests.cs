using System.Net;
using Microsoft.AspNetCore.Http;
using WebPagePub.WebApp.Helpers;
using Xunit;

namespace WebPagePub.WebApp.UnitTests
{
    public class SignInRateLimitTests
    {
        [Theory]
        [InlineData(null, "local")]
        [InlineData("127.0.0.1", "local")]
        [InlineData("::1", "local")]
        [InlineData("::ffff:127.0.0.1", "local")]
        [InlineData("203.0.113.9", "203.0.113.9")]
        [InlineData("::ffff:203.0.113.9", "203.0.113.9")]
        [InlineData("2001:db8:1:2:aaaa:bbbb:cccc:dddd", "2001:db8:1:2::/64")]
        [InlineData("2001:db8:1:2:1::1", "2001:db8:1:2::/64")]
        public void ClientKey_GroupsAddresses(string? address, string expected)
        {
            var ip = address == null ? null : IPAddress.Parse(address);
            Assert.Equal(expected, SignInRateLimit.ClientKey(ip));
        }

        [Theory]
        [InlineData("POST", "/account/login", true)]
        [InlineData("POST", "/Account/Login/", true)]
        [InlineData("GET", "/account/login", false)]
        [InlineData("POST", "/account/logoff", false)]
        [InlineData("POST", "/search", false)]
        public void IsSignInPost_MatchesOnlySignInPost(string method, string path, bool expected)
        {
            var context = NewContext(method, path, "203.0.113.1");
            Assert.Equal(expected, SignInRateLimit.IsSignInPost(context.Request));
        }

        [Fact]
        public void OneClient_IsStoppedAfterLimit_OthersStillPass()
        {
            using var limiter = SignInRateLimit.CreateLimiter();

            for (var i = 0; i < SignInRateLimit.PerClientLimit; i++)
            {
                using var ok = limiter.AttemptAcquire(NewContext("POST", "/account/login", "203.0.113.1"));
                Assert.True(ok.IsAcquired);
            }

            using (var blocked = limiter.AttemptAcquire(NewContext("POST", "/account/login", "203.0.113.1")))
            {
                Assert.False(blocked.IsAcquired);
            }

            using (var other = limiter.AttemptAcquire(NewContext("POST", "/account/login", "203.0.113.2")))
            {
                Assert.True(other.IsAcquired);
            }

            // Pages other than the sign-in post are never limited.
            for (var i = 0; i < 50; i++)
            {
                using var page = limiter.AttemptAcquire(NewContext("GET", "/account/login", "203.0.113.1"));
                Assert.True(page.IsAcquired);
            }
        }

        [Fact]
        public void SiteWideCeiling_StopsManyClients()
        {
            using var limiter = SignInRateLimit.CreateLimiter();
            var acquired = 0;

            for (var i = 0; i < SignInRateLimit.SiteWideLimit + 50; i++)
            {
                var ip = $"198.51.{i / 200}.{i % 200}";
                using var lease = limiter.AttemptAcquire(NewContext("POST", "/account/login", ip));
                if (lease.IsAcquired)
                {
                    acquired++;
                }
            }

            Assert.Equal(SignInRateLimit.SiteWideLimit, acquired);
        }

        private static DefaultHttpContext NewContext(string method, string path, string ip)
        {
            var context = new DefaultHttpContext();
            context.Request.Method = method;
            context.Request.Path = path;
            context.Connection.RemoteIpAddress = IPAddress.Parse(ip);
            return context;
        }
    }
}
