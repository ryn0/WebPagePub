using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using WebPagePub.Data.Repositories.Interfaces;
using WebPagePub.Data.Models;
using WebPagePub.Data.Constants;
using Microsoft.AspNetCore.Identity;
using WebPagePub.Web.Models;
using System;
using WebPagePub.Services.Interfaces;

namespace WebPagePub.Web.Controllers
{
    [Authorize(Roles = StringConstants.AdminRole)]
    public class CommentManagementController : Controller
    {
        const int AmountPerPage = 10;

        private readonly ISitePageCommentRepository _sitePageCommentRepository;
        private readonly ISitePageRepository _sitePageRepository;
        private readonly ISpamFilterService _spamFilterService;
        private readonly UserManager<ApplicationUser> _userManager;

        public CommentManagementController(
            ISitePageCommentRepository sitePageCommentRepository,
            ISitePageRepository sitePageRepository,
            ISpamFilterService spamFilterService,
            UserManager<ApplicationUser> userManager)
        {
            _sitePageCommentRepository = sitePageCommentRepository;
            _sitePageRepository = sitePageRepository;
            _spamFilterService = spamFilterService;
            _userManager = userManager;
        }

        [Route("CommentManagement")]
        [HttpGet]
        public IActionResult Index(int pageNumber = 1)
        {
            var dbModel = _sitePageCommentRepository.GetPage(pageNumber, AmountPerPage, out int total);

            var model = new SitePageCommentListModel()
            {
                Total = total,
                CurrentPageNumber = pageNumber,
                QuantityPerPage = AmountPerPage
            };

            var pageCount = (double)model.Total / model.QuantityPerPage;
            model.PageCount = (int)Math.Ceiling(pageCount);

            foreach(var item in dbModel)
            {
                model.Items.Add(new SitePageCommentItemModel()
                {
                    CreateDate = item.CreateDate,
                    Name = item.Name,
                    SitePageCommentId = item.SitePageCommentId,
                    CommentStatus = item.CommentStatus
                });
            }

            return View(model);
        }

        [Route("CommentManagement/Edit")]
        [HttpGet]
        public IActionResult Edit(int sitePageCommentId)
        {
            var dbModel = _sitePageCommentRepository.Get(sitePageCommentId);

            var model = new SitePageCommentModel()
            {
                Comment = dbModel.Comment,
                CommentStatus = dbModel.CommentStatus,
                CreateDate = dbModel.CreateDate,
                Email = dbModel.Email,
                Name = dbModel.Name,
                RequestId = dbModel.RequestId,
                SitePageCommentId = dbModel.SitePageCommentId,
                SitePageId = dbModel.SitePageId,
                Website = dbModel.WebSite
            };

            return View(model);
        }

        [Route("CommentManagement/Edit")]
        [HttpPost]
        public IActionResult Edit(SitePageCommentModel model)
        {
            var dbModel = _sitePageCommentRepository.Get(model.SitePageCommentId);

            dbModel.Name = model.Name.Trim();
            dbModel.Comment = model.Comment.Trim();
            dbModel.Email = model.Email.Trim();
            dbModel.WebSite = model.Website?.Trim();
            dbModel.CommentStatus = model.CommentStatus;

            if (model.CommentStatus == Data.Enums.CommentStatus.Spam)
            {
                if (!_spamFilterService.IsBlocked(dbModel.IpAddress))
                {
                    _spamFilterService.Create(dbModel.IpAddress);
                }
            }

            _sitePageCommentRepository.Update(dbModel);

            return RedirectToAction("Index");
        }
    }
}