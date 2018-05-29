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
        private readonly IRedirectPathRepository _redirectPathRepository;
        private readonly ICacheService _contentSnippetHelper;

        public RedirectPathManagementController(
            IRedirectPathRepository redirectPathRepository,
            ICacheService contentSnippetHelper)
        {
            _redirectPathRepository = redirectPathRepository;
            _contentSnippetHelper = contentSnippetHelper;
        }

        [Route("RedirectPathManagement")]
        public IActionResult Index()
        {
            var all = _redirectPathRepository.GetAll();
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

            return View(model);
        }

        [Route("RedirectPathManagement/create")]
        [HttpPost]
        public IActionResult Create(RedirectPathCreateModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var dbModel = _redirectPathRepository.Get(model.Path);

            if (dbModel != null)
                throw new System.Exception("already exists");

            _redirectPathRepository.Create(new Data.Models.Db.RedirectPath()
            {
                Path = model.Path.Trim(),
                PathDestination = model.PathDestination.Trim()
            });

            return RedirectToAction(nameof(Index));
        }

        [Route("RedirectPathManagement/create")]
        [HttpGet]
        public IActionResult Create()
        {
            var model = new RedirectPathCreateModel()
            {

            };

            return View(model);
        }
  
        [Route("RedirectPathManagement/delete")]
        [HttpPost]
        public IActionResult Delete(int redirectPathId)
        {
            _redirectPathRepository.Delete(redirectPathId);

            return RedirectToAction(nameof(Index));
        }
    }
}
