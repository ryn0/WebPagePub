using System;
using Microsoft.AspNetCore.Mvc;
using WebPagePub.Web.Models;
using WebPagePub.Data.Repositories.Interfaces;

namespace WebPagePub.Web.Controllers
{
    public class EmailSubscriptionController : Controller
    {
        private readonly IEmailSubscriptionRepository _emailSubscriptionRepository;

        public EmailSubscriptionController(IEmailSubscriptionRepository emailSubscriptionRepository)
        {
            _emailSubscriptionRepository = emailSubscriptionRepository;
        }

        [Route("EmailSubscription/subscribe")]
        [HttpPost]
        public IActionResult Subscribe(EmailSubscribeModel model)
        {
            if (!ModelState.IsValid)
                throw new Exception("invalid email submission");

            var emailDbModel = _emailSubscriptionRepository.Get(model.Email);

            if (emailDbModel == null || emailDbModel.EmailSubscriptionId == 0)
            {
                _emailSubscriptionRepository.Create(new Data.Models.Db.EmailSubscription()
                {
                    Email = model.Email,
                    IsSubscribed = true
                });
            }

            return View();
        }

        [Route("EmailSubscription/unsubscribe")]
        [HttpGet]
        public IActionResult UnSubscribe( )
        {
           
            return View();
        }

        [Route("EmailSubscription/unsubscribe")]
        [HttpPost]
        public IActionResult UnSubscribe(EmailSubscribeModel model)
        {
            if (!ModelState.IsValid)
                throw new Exception("invalid email submission");

            var emailDbModel = _emailSubscriptionRepository.Get(model.Email);

            if (emailDbModel != null)
            {
                emailDbModel.IsSubscribed = false;

                _emailSubscriptionRepository.Update(emailDbModel);

                ViewBag.Success = true;
            }

            return View();
        }
    }
}
