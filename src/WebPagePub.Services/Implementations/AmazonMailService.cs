using System;
using System.Reflection;
using System.Threading.Tasks;
using Amazon;
using Amazon.SimpleEmail;
using Amazon.SimpleEmail.Model;
using log4net;
using WebPagePub.Services.Interfaces;

namespace WebPagePub.Services.Implementations
{
    public class AmazonMailService : IEmailSender
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly string amazonAccessKey;
        private readonly string amazonSecretKey;
        private readonly string amazonEmailFrom;

        public AmazonMailService(string amazonAccessKey, string amazonSecretKey, string amazonEmailFrom)
        {
            this.amazonEmailFrom = amazonEmailFrom?.Trim();
            this.amazonAccessKey = amazonAccessKey?.Trim();
            this.amazonSecretKey = amazonSecretKey?.Trim();
        }

        public Task SendEmailAsync(string email, string subject, string message)
        {
            if (!this.HasConfigs())
            {
                return Task.FromResult(0);
            }

            return this.SendTextMailAsync(email, subject, message);
        }

        public async Task<bool> SendHtmlMail(string toEmail, string subject, string htmlBody)
        {
            if (!this.HasConfigs())
            {
                return await Task.FromResult(false);
            }

            if (string.IsNullOrWhiteSpace(htmlBody))
            {
                return false;
            }

            var bdy = new Body { Html = new Content(htmlBody) };

            return await this.SendMailAsync(toEmail, subject, bdy);
        }

        public async Task<bool> SendTextMailAsync(string toEmail, string subject, string body)
        {
            if (!this.HasConfigs())
            {
                return await Task.FromResult(false);
            }

            if (string.IsNullOrWhiteSpace(body))
            {
                return false;
            }

            var bdy = new Body { Text = new Content(body) };

            return await this.SendMailAsync(toEmail, subject, bdy);
        }

        private async Task<bool> SendMailAsync(string toEmail, string subject, Body body)
        {
            if (string.IsNullOrEmpty(toEmail) ||
                string.IsNullOrEmpty(this.amazonEmailFrom) ||
                string.IsNullOrEmpty(subject))
            {
                return false;
            }

            try
            {
                toEmail = toEmail.Trim();

                var amzClient = new AmazonSimpleEmailServiceClient(
                   this.amazonAccessKey,
                   this.amazonSecretKey,
                   RegionEndpoint.USEast1);

                var dest = new Destination();

                dest.ToAddresses.Add(toEmail);

                var title = new Content(subject);
                var message = new Message(title, body);
                var ser = new SendEmailRequest(this.amazonEmailFrom, dest, message);

                await amzClient.SendEmailAsync(ser);

                return true;
            }
            catch (Exception ex)
            {
                Log.Fatal(ex);
                return false;
            }
        }

        private bool HasConfigs()
        {
            if (string.IsNullOrEmpty(this.amazonAccessKey) ||
                string.IsNullOrEmpty(this.amazonEmailFrom) ||
                string.IsNullOrEmpty(this.amazonSecretKey))
            {
                return false;
            }

            return true;
        }
    }
}