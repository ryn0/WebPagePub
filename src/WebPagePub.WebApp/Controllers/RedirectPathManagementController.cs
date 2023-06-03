using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebPagePub.Data.Repositories.Interfaces;
using WebPagePub.Services.Interfaces;
using WebPagePub.Web.Models;

namespace WebPagePub.Web.Controllers
{
    [Authorize]
    public class RedirectPathManagementController : Controller
    {
        private readonly IRedirectPathRepository redirectPathRepository;
        private readonly ICacheService contentSnippetHelper;

        public RedirectPathManagementController(
            IRedirectPathRepository redirectPathRepository,
            ICacheService contentSnippetHelper)
        {
            this.redirectPathRepository = redirectPathRepository;
            this.contentSnippetHelper = contentSnippetHelper;
        }

        [Route("RedirectPathManagement")]
        public IActionResult Index()
        {
            var all = this.redirectPathRepository.GetAll();
            var model = new RedirectPathListModel();

            foreach (var item in all)
            {
                model.Items.Add(new RedirectPathListModel.RedirectPathItemModel()
                {
                    Path = item.Path,
                    PathDestination = item.PathDestination,
                    RedirectPathId = item.RedirectPathId
                });
            }

            return this.View(model);
        }

        [Route("RedirectPathManagement/create")]
        [HttpPost]
        public IActionResult Create(RedirectPathCreateModel model)
        {
            if (!this.ModelState.IsValid)
            {
                return this.View(model);
            }

            var dbModel = this.redirectPathRepository.Get(model.Path);

            if (dbModel != null)
            {
                throw new System.Exception("already exists");
            }

            this.redirectPathRepository.Create(new Data.Models.Db.RedirectPath()
            {
                Path = model.Path.Trim(),
                PathDestination = model.PathDestination.Trim()
            });

            return this.RedirectToAction(nameof(this.Index));
        }

        [Route("RedirectPathManagement/create")]
        [HttpGet]
        public IActionResult Create()
        {
            var model = new RedirectPathCreateModel()
            {
            };

            return this.View(model);
        }

        [Route("RedirectPathManagement/delete")]
        [HttpPost]
        public IActionResult Delete(int redirectPathId)
        {
            this.redirectPathRepository.Delete(redirectPathId);

            return this.RedirectToAction(nameof(this.Index));
        }
    }
}
