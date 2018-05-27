using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using WebPagePub.Data.Repositories.Interfaces;
using WebPagePub.Web.Models;
using WebPagePub.Data.Enums;

namespace WebPagePub.Web.Controllers
{
    [Authorize]
    public class AdminController : Controller
    {
        private readonly ISitePageCommentRepository _sitePageCommentRepository;

        public AdminController(ISitePageCommentRepository sitePageCommentRepository)
        {
            _sitePageCommentRepository = sitePageCommentRepository;
        }

        [Route("admin/index")]
        public IActionResult Index()
        {
            var totalRequiringModeration = _sitePageCommentRepository.GetCommentCountForStatus(CommentStatus.AwaitingModeration);

            var model = new AdminManagementModel()
            {
                CountOfCommentsToModerate = totalRequiringModeration
            };

            return View(model);
        }
    }
}
