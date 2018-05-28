using Amazon;
using Amazon.SimpleEmail;
using Amazon.SimpleEmail.Model;
using WebPagePub.Services.Interfaces;
using log4net;
using System;
using System.Reflection;
using System.Threading.Tasks;

namespace WebPagePub.Services.Implementations
{
    public class AmazonMailService : IEmailSender
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        readonly string _amazonAccessKey;
        readonly string _amazonSecretKey;
        readonly string _amazonEmailFrom;

        public AmazonMailService(string amazonAccessKey, string amazonSecretKey, string amazonEmailFrom)
        {
            _amazonEmailFrom = amazonEmailFrom?.Trim();
            _amazonAccessKey = amazonAccessKey?.Trim();
            _amazonSecretKey = amazonSecretKey?.Trim();
        }

        public Task SendEmailAsync(string email, string subject, string message)
        {
            if (!HasConfigs())
                return Task.FromResult(0);

            return SendTextMailAsync(email, subject, message);
        }

        public async Task<bool> SendHtmlMail(string toEmail, string subject, string htmlBody)
        {
            if (!HasConfigs())
                return await Task.FromResult(false);

            if (string.IsNullOrWhiteSpace(htmlBody))
                return false;

            var bdy = new Body { Html = new Content(htmlBody) };

            return await SendMailAsync(toEmail, subject, bdy);
        }

        public async Task<bool> SendTextMailAsync(string toEmail, string subject, string body)
        {
            if (!HasConfigs())
                return await Task.FromResult(false);

            if (string.IsNullOrWhiteSpace(body))
                return false;

            var bdy = new Body { Text = new Content(body) };

            return await SendMailAsync(toEmail, subject, bdy);
        }

        private async Task<bool> SendMailAsync(string toEmail, string subject, Body body)
        {
            if (string.IsNullOrEmpty(toEmail) ||
                string.IsNullOrEmpty(_amazonEmailFrom) ||
                string.IsNullOrEmpty(subject))
                return false;

            try
            {
                toEmail = toEmail.Trim();

                var amzClient = new AmazonSimpleEmailServiceClient(
                   _amazonAccessKey,
                   _amazonSecretKey,
                    RegionEndpoint.USEast1);

                var dest = new Destination();

                dest.ToAddresses.Add(toEmail);

                var title = new Content(subject);
                var message = new Message(title, body);
                var ser = new SendEmailRequest(_amazonEmailFrom, dest, message);

                await amzClient.SendEmailAsync(ser);

                return true;
            }
            catch (Exception ex)
            {
                log.Fatal(ex);
                return false;
            }
        }

        private bool HasConfigs()
        {
            if (string.IsNullOrEmpty(_amazonAccessKey) ||
                string.IsNullOrEmpty(_amazonEmailFrom) ||
                string.IsNullOrEmpty(_amazonSecretKey))
                return false;

            return true;
        }
    }
}
