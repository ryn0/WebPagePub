using System.Threading.Tasks;
using IPinfo;
using WebPagePub.Data.Repositories.Interfaces;
using WebPagePub.Services.Interfaces;

namespace WebPagePub.Services.Implementations
{
    public class SpamFilterService : ISpamFilterService
    {
        private readonly IBlockedIPRepository blockedIPRepository;
        private readonly string ipinfoToken;
        private readonly IPinfoClient client;

        public SpamFilterService(
            IBlockedIPRepository blockedIPRepository,
            string ipinfoToken)
        {
            this.blockedIPRepository = blockedIPRepository;
            this.ipinfoToken = ipinfoToken;
            this.client = new IPinfoClient.Builder()
                .AccessToken(ipinfoToken)
                .Build();
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

            try
            {
                var ipResponse = this.client.IPApi.GetDetails(ipAddress);

                if (ipResponse.Country == "US")
                {
                    return false; // Accept only US traffic
                }

                return true; // Block traffic outside of US
            }
            catch
            {
                return false; // In case of an error, do not block
            }
        }

        private bool HasConfigs()
        {
            return !string.IsNullOrWhiteSpace(this.ipinfoToken);
        }
    }
}