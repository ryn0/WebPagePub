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
        public IActionResult Index(int pageNumber = 1, int pageSize = 10)
        {
            int totalItems;
            var pagedEmails = this.emailSubscriptionRepository.GetPaged(pageNumber, pageSize, out totalItems);

            var model = new EmailSubscribeEditListModel
            {
                Items = pagedEmails.Select(sub => new EmailSubscribeEditModel
                {
                    Email = sub.Email,
                    IsSubscribed = sub.IsSubscribed,
                    EmailSubscriptionId = sub.EmailSubscriptionId
                }).ToList(),
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalItems = totalItems
            };

            model.TotalPages = (int)Math.Ceiling((double)totalItems / pageSize);

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