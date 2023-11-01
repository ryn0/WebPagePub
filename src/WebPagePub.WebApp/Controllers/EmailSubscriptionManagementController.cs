using System.Text;
using Microsoft.AspNetCore.Mvc;
using WebPagePub.Data.Repositories.Interfaces;
using WebPagePub.Services.Interfaces;
using WebPagePub.Web.Models;

namespace WebPagePub.Web.Controllers
{
    public class EmailSubscriptionManagementController : Controller
    {
        private readonly IEmailSubscriptionRepository emailSubscriptionRepository;
        private readonly IEmailSender emailSender;

        public EmailSubscriptionManagementController(
            IEmailSubscriptionRepository emailSubscriptionRepository,
            IEmailSender emailSender)
        {
            this.emailSubscriptionRepository = emailSubscriptionRepository;
            this.emailSender = emailSender;
        }

        [Route("EmailSubscriptionManagement")]
        public IActionResult Index()
        {
            var allEmails = this.emailSubscriptionRepository.GetAll();
            var model = new EmailSubscribeEditListModel();

            foreach (var sub in allEmails)
            {
                model.Items.Add(new EmailSubscribeEditModel()
                {
                    Email = sub.Email,
                    IsSubscribed = sub.IsSubscribed,
                    EmailSubscriptionId = sub.EmailSubscriptionId
                });
            }

            if (allEmails != null && allEmails.Count > 0)
            {
                var sb = new StringBuilder();
                foreach (var sub in allEmails)
                {
                    if (!sub.IsSubscribed)
                    {
                        continue;
                    }

                    sb.AppendFormat("{0}, ", sub.Email);
                }

                model.Emails = sb.ToString();
                model.Emails = model.Emails.Trim().TrimEnd(',');
            }

            var link = string.Format(
                "{0}://{1}/EmailSubscription/Unsubscribe",
                this.HttpContext.Request.Scheme,
                this.HttpContext.Request.Host);
            model.UnsubscribeLink = link;

            return this.View(model);
        }

        [Route("EmailSubscriptionManagement/sendmail")]
        [HttpPost]
        public IActionResult SendMail(EmailSendModel model)
        {
            if (string.IsNullOrWhiteSpace(model.EmailMessage) ||
                string.IsNullOrWhiteSpace(model.EmailTitle) ||
                string.IsNullOrWhiteSpace(model.SendToEmails))
            {
                throw new InvalidOperationException();
            }

            var emails = model.SendToEmails.Split(',');

            foreach (var email in emails)
            {
                this.emailSender.SendEmailAsync(email.Trim(), model.EmailTitle, model.EmailMessage);
                System.Threading.Thread.Sleep(300); // todo: use queue
            }

            return this.RedirectToAction("index");
        }

        [Route("EmailSubscriptionManagement/edit")]
        [HttpPost]
        public IActionResult Edit(EmailSubscribeEditModel model)
        {
            if (!this.ModelState.IsValid)
            {
                return this.View(model);
            }

            var dbModel = this.emailSubscriptionRepository.Get(model.EmailSubscriptionId);

            dbModel.Email = model.Email;
            dbModel.IsSubscribed = model.IsSubscribed;

            this.emailSubscriptionRepository.Update(dbModel);

            return this.RedirectToAction("index");
        }

        [Route("EmailSubscriptionManagement/edit")]
        [HttpGet]
        public IActionResult Edit(int emailSubscriptionId)
        {
            var dbModel = this.emailSubscriptionRepository.Get(emailSubscriptionId);

            var model = new EmailSubscribeEditModel()
            {
                Email = dbModel.Email,
                IsSubscribed = dbModel.IsSubscribed,
                EmailSubscriptionId = dbModel.EmailSubscriptionId
            };

            return this.View(model);
        }
    }
}