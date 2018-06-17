using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Newtonsoft.Json;
using WebPagePub.Data.Constants;
using WebPagePub.Data.Repositories.Interfaces;
using WebPagePub.Services.Interfaces;
using WebPagePub.Services.Models;

namespace WebPagePub.Services.Implementations
{
    public class SpamFilterService : ISpamFilterService
    {
        private const string ApiUrlFormat = "https://neutrinoapi.com/ip-blocklist?ip={0}&user-id={1}&api-key={2}";
        private readonly IBlockedIPRepository blockedIPRepository;
        private readonly string userId;
        private readonly string apiKey;

        public SpamFilterService (
            IBlockedIPRepository blockedIPRepository,
            string userId,
            string apiKey)
        {
            this.blockedIPRepository = blockedIPRepository;
            this.userId = userId;
            this.apiKey = apiKey;
        }

        public void Create(string ipAddress)
        {
            Task.Run(() => this.blockedIPRepository.CreateAsync(new Data.Models.Db.BlockedIP()
            {
                IpAddress = ipAddress
            })).Wait();
        }

        public bool IsBlocked(string ipAddress)
        {
            if (this.blockedIPRepository.IsBlockedIp(ipAddress))
            {
                return true;
            }

            if (!this.HasConfigs())
            {
                return false;
            }

            var client = new HttpClient
            {
                BaseAddress = new Uri(ApiUrlFormat)
            };
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue(StringConstants.ApplicationJson));

            var response = Task.Run(() => client.GetStringAsync(this.FormattedUrl(ipAddress))).Result;
            var model = JsonConvert.DeserializeObject<IpBlocklistModel>(response);
            if (model.Blocklists.Any() ||
                model.Isbot || model.Isexploitbot || model.Ishijacked || model.Ismalware ||
                model.Isproxy || model.Isspambot || model.Isspyware)
            {
                Task.Run(() => this.blockedIPRepository.CreateAsync(new Data.Models.Db.BlockedIP()
                {
                    IpAddress = ipAddress
                }).Result);

                return true;
            }

            return false;
        }

        private string FormattedUrl(string ipAddress)
        {
            return string.Format(ApiUrlFormat, ipAddress, this.userId, this.apiKey);
        }

        private bool HasConfigs()
        {
            if (string.IsNullOrWhiteSpace(this.apiKey) ||
                string.IsNullOrWhiteSpace(this.userId))
            {
                return false;
            }

            return true;
        }
    }
}
