using System;
using System.Linq;
using System.Threading.Tasks;
using NeutrinoAPI;
using WebPagePub.Data.Repositories.Interfaces;
using WebPagePub.Services.Interfaces;

namespace WebPagePub.Services.Implementations
{
    public class SpamFilterService : ISpamFilterService
    {
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

            var neutrinoApiClient = new NeutrinoAPIClient(this.userId, this.apiKey);

            try
            {
                var response = neutrinoApiClient.SecurityAndNetworking.IPBlocklist(ipAddress);

                if (response.Blocklists.Any() ||
                            response.IsBot || response.IsExploitBot || response.IsHijacked || response.IsMalware ||
                            response.IsProxy || response.IsSpamBot || response.IsSpyware)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                return false;
            }
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
