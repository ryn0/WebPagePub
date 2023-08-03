using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebPagePub.Data.Enums;
using WebPagePub.Data.Repositories.Interfaces;
using WebPagePub.Web.Helpers;
using WebPagePub.Web.Models;

namespace WebPagePub.Web.Controllers
{
    [Authorize]
    public class AdminController : Controller
    {
        private readonly ISitePageCommentRepository sitePageCommentRepository;

        public AdminController(ISitePageCommentRepository sitePageCommentRepository)
        {
            this.sitePageCommentRepository = sitePageCommentRepository;
        }

        [Route("admin/index")]
        public IActionResult Index()
        {
            var totalRequiringModeration = this.sitePageCommentRepository
                                               .GetCommentCountForStatus(CommentStatus.AwaitingModeration);

            var location = new Uri($"{Request.Scheme}://{Request.Host}{Request.Path}{Request.QueryString}");
            var url = location.AbsoluteUri;

            var model = new AdminManagementModel()
            {
                CountOfCommentsToModerate = totalRequiringModeration,
                DebugText = new Uri(url).ToString()
            };

            return this.View(model);
        }
    }
}
