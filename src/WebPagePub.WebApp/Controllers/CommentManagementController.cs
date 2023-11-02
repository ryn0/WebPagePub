using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WebPagePub.Data.Constants;
using WebPagePub.Data.Enums;
using WebPagePub.Data.Models;
using WebPagePub.Data.Repositories.Interfaces;
using WebPagePub.Services.Interfaces;
using WebPagePub.WebApp.Models.SitePage;

namespace WebPagePub.Web.Controllers
{
    [Authorize(Roles = StringConstants.AdminRole)]
    public class CommentManagementController : Controller
    {
        private readonly ISitePageCommentRepository sitePageCommentRepository;
        private readonly ISitePageRepository sitePageRepository;
        private readonly ISpamFilterService spamFilterService;
        private readonly UserManager<ApplicationUser> userManager;

        public CommentManagementController(
            ISitePageCommentRepository sitePageCommentRepository,
            ISitePageRepository sitePageRepository,
            ISpamFilterService spamFilterService,
            UserManager<ApplicationUser> userManager)
        {
            this.sitePageCommentRepository = sitePageCommentRepository;
            this.sitePageRepository = sitePageRepository;
            this.spamFilterService = spamFilterService;
            this.userManager = userManager;
        }

        [Route("CommentManagement")]
        [HttpGet]
        public IActionResult Index(int pageNumber = 1)
        {
            var dbModel = this.sitePageCommentRepository.GetPage(pageNumber, WebApp.Constants.IntegerConstants.AmountPerPage, out int total);

            var model = new SitePageCommentListModel()
            {
                Total = total,
                CurrentPageNumber = pageNumber,
                QuantityPerPage = WebApp.Constants.IntegerConstants.AmountPerPage
            };

            var pageCount = (double)model.Total / model.QuantityPerPage;
            model.PageCount = (int)Math.Ceiling(pageCount);

            foreach (var item in dbModel)
            {
                model.Items.Add(new SitePageCommentItemModel()
                {
                    CreateDate = item.CreateDate,
                    Name = item.Name,
                    SitePageCommentId = item.SitePageCommentId,
                    CommentStatus = item.CommentStatus
                });
            }

            return this.View(model);
        }

        [Route("CommentManagement/Edit")]
        [HttpGet]
        public IActionResult Edit(int sitePageCommentId)
        {
            var dbModel = this.sitePageCommentRepository.Get(sitePageCommentId);

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

            return this.View(model);
        }

        [Route("CommentManagement/Edit")]
        [HttpPost]
        public IActionResult Edit(SitePageCommentModel model)
        {
            var dbModel = this.sitePageCommentRepository.Get(model.SitePageCommentId);

            dbModel.Name = model.Name.Trim();
            dbModel.Comment = model.Comment.Trim();
            dbModel.Email = model.Email.Trim();
            dbModel.WebSite = model.Website?.Trim();
            dbModel.CommentStatus = model.CommentStatus;

            if (model.CommentStatus == Data.Enums.CommentStatus.Spam &&
                !this.spamFilterService.IsBlocked(dbModel.IpAddress))
            {
                this.spamFilterService.Create(dbModel.IpAddress);
            }

            this.sitePageCommentRepository.Update(dbModel);

            return this.RedirectToAction(nameof(this.Index));
        }

        [Route("CommentManagement/deletespam")]
        [HttpGet]
        public IActionResult DeleteSpam()
        {
            var dbModel = this.sitePageCommentRepository.DeleteStaus(CommentStatus.Spam);

            return this.RedirectToAction(nameof(this.Index));
        }
    }
}