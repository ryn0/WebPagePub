using Newtonsoft.Json;
using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using WebPagePub.Data.Constants;
using WebPagePub.Data.Repositories.Interfaces;
using WebPagePub.Services.Interfaces;
using WebPagePub.Services.Models;

namespace WebPagePub.Services.Implementations
{
    public class SpamFilterService : ISpamFilterService
    {
        private const string ApiUrlFormat = "https://neutrinoapi.com/ip-blocklist?ip={0}&user-id={1}&api-key={2}";
        private readonly IBlockedIPRepository _blockedIPRepository;
        private readonly string _userId;
        private readonly string _apiKey;

        public SpamFilterService (
            IBlockedIPRepository blockedIPRepository,
            string userId,
            string apiKey)
        {
            _blockedIPRepository = blockedIPRepository;
            _userId = userId;
            _apiKey = apiKey;
        }

        public void Create(string ipAddress)
        {
            Task.Run(() => _blockedIPRepository.CreateAsync(new Data.Models.Db.BlockedIP()
            {
                IpAddress = ipAddress
            })).Wait();
        }

        public bool IsBlocked(string ipAddress)
        {
            if (_blockedIPRepository.IsBlockedIp(ipAddress))
            {
                return true;
            }

            if (!HasConfigs())
                return false;

            var client = new HttpClient
            {
                BaseAddress = new Uri(ApiUrlFormat)
            };
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue(StringConstants.ApplicationJson));

            var response = Task.Run(() => client.GetStringAsync(FormattedUrl(ipAddress))).Result;
            var model = JsonConvert.DeserializeObject<IpBlocklistModel>(response);
            if (model.blocklists.Any() ||
                model.isbot || model.isexploitbot || model.ishijacked || model.ismalware || 
                model.isproxy || model.isspambot || model.isspyware)
            {
                Task.Run(() => _blockedIPRepository.CreateAsync(new Data.Models.Db.BlockedIP()
                {
                    IpAddress = ipAddress
                }).Result);

                return true;
            }

            return false;
        }

        private string FormattedUrl(string ipAddress)
        {
            return string.Format(ApiUrlFormat, ipAddress, _userId, _apiKey);
        }

        private bool HasConfigs()
        {
            if (string.IsNullOrWhiteSpace(_apiKey) ||
                string.IsNullOrWhiteSpace(_userId))
                return false;

            return true;
        }
    }
}
