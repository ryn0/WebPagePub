using Microsoft.AspNetCore.Mvc;
using WebPagePub.Data.Repositories.Interfaces;
using WebPagePub.Web.Models;

namespace WebPagePub.Web.Controllers
{
    public class EmailSubscriptionController : Controller
    {
        private readonly IEmailSubscriptionRepository emailSubscriptionRepository;

        public EmailSubscriptionController(IEmailSubscriptionRepository emailSubscriptionRepository)
        {
            this.emailSubscriptionRepository = emailSubscriptionRepository;
        }

        [Route("EmailSubscription/subscribe")]
        [HttpPost]
        public IActionResult Subscribe(EmailSubscribeModel model)
        {
            if (!this.ModelState.IsValid)
            {
                throw new Exception("invalid email submission");
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
    }
}
