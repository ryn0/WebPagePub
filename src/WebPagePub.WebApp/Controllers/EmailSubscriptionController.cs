using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using WebPagePub.Data.Repositories.Interfaces;
using WebPagePub.Services.Interfaces;
using WebPagePub.Web.Models;

namespace WebPagePub.Web.Controllers
{
    public class EmailSubscriptionController : Controller
    {
        private readonly IEmailSubscriptionRepository emailSubscriptionRepository;
        private readonly ISpamFilterService spamFilterService;
        private readonly IHttpContextAccessor accessor;

        public EmailSubscriptionController(
                IEmailSubscriptionRepository emailSubscriptionRepository,
                ISpamFilterService spamFilterService,
                IHttpContextAccessor accessor)
        {
            this.emailSubscriptionRepository = emailSubscriptionRepository;
            this.spamFilterService = spamFilterService;
            this.accessor = accessor;
        }

        [Route("EmailSubscription/subscribe")]
        [HttpPost]
        public IActionResult Subscribe(EmailSubscribeModel model)
        {
            if (!this.ModelState.IsValid)
            {
                throw new Exception("Invalid email submission.");
            }

            // Validate and sanitize the email input
            if (!this.IsValidEmail(model.Email))
            {
                throw new Exception("Invalid email format.");
            }

            var context = this.accessor.HttpContext ??
                throw new InvalidOperationException("HttpContext is not available.");
            var ipAddress = context.Connection.RemoteIpAddress?.ToString();
            if (string.IsNullOrEmpty(ipAddress))
            {
                throw new InvalidOperationException("IP address is not available.");
            }

            if (this.spamFilterService.IsBlocked(ipAddress))
            {
                throw new Exception("Invalid IP");
            }

            var emailDbModel = this.emailSubscriptionRepository.Get(model.Email);

            if (emailDbModel == null || emailDbModel.EmailSubscriptionId == 0)
            {
                this.emailSubscriptionRepository.Create(new Data.Models.Db.EmailSubscription()
                {
                    Email = model.Email,
                    IsSubscribed = true
                });
            }

            return this.View();
        }

        [Route("EmailSubscription/unsubscribe")]
        [HttpGet]
        public IActionResult UnSubscribe()
        {
            return this.View();
        }

        [Route("EmailSubscription/unsubscribe")]
        [HttpPost]
        public IActionResult UnSubscribe(EmailSubscribeModel model)
        {
            if (!this.ModelState.IsValid)
            {
                throw new Exception("invalid email submission");
            }

            var emailDbModel = this.emailSubscriptionRepository.Get(model.Email);

            if (emailDbModel != null)
            {
                emailDbModel.IsSubscribed = false;

                this.emailSubscriptionRepository.Update(emailDbModel);

                this.ViewBag.Success = true;
            }

            return this.View();
        }

        private bool IsValidEmail(string email)
        {
            // Basic email validation using regular expressions
            if (string.IsNullOrWhiteSpace(email))
            {
                return false;
            }

            // This regex pattern can be adjusted to be more or less strict as needed
            var emailRegex = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
            return Regex.IsMatch(email, emailRegex);
        }
    }
}
