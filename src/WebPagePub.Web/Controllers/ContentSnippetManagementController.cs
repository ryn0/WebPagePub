using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using WebPagePub.Data.Repositories.Interfaces;
using WebPagePub.Web.Models;
using WebPagePub.Data.Models.Db;
using System.Linq;
using WebPagePub.Services.Interfaces;

namespace WebPagePub.Web.Controllers
{
    [Authorize]
    public class ContentSnippetManagementController : Controller
    {
        private readonly IContentSnippetRepository _contentSnippetRepository;
        private readonly ICacheService _contentSnippetHelper;

        public ContentSnippetManagementController(
            IContentSnippetRepository contentSnippetRepository, 
            ICacheService contentSnippetHelper)
        {
            _contentSnippetRepository = contentSnippetRepository;
            _contentSnippetHelper = contentSnippetHelper;
        }

        [Route("ContentSnippetManagement")]
        public IActionResult Index()
        {
            var allSnippets = _contentSnippetRepository.GetAll().OrderBy(x => x.SnippetType.ToString());
            var model = new ContentSnippetEditListModel();

            foreach (var snippet in allSnippets)
            {
                model.Items.Add(new ContentSnippetEditModel()
                {
                    Content = snippet.Content,
                    ContentSnippetId = snippet.ContentSnippetId,
                    SnippetType = snippet.SnippetType
                });
            }

            return View(model);
        }

        [Route("ContentSnippetManagement/create")]
        [HttpPost]
        public IActionResult Create(ContentSnippetEditModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var dbModel = _contentSnippetRepository.Get(model.ContentSnippetId);

            if (dbModel != null)
                throw new System.Exception("type already exists");

            _contentSnippetRepository.Create(new ContentSnippet()
            {
                Content = model.Content.Trim(),
                ContentSnippetId = model.ContentSnippetId,
                SnippetType = model.SnippetType
            });

            return RedirectToAction("index");
        }

        [Route("ContentSnippetManagement/create")]
        [HttpGet]
        public IActionResult Create()
        {
            var model = new ContentSnippetEditModel()
            {
                
            };
                
            return View();
        }


        [Route("ContentSnippetManagement/edit")]
        [HttpPost]
        public IActionResult Edit(ContentSnippetEditModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var dbModel = _contentSnippetRepository.Get(model.ContentSnippetId);

            dbModel.Content = model.Content.Trim();
            dbModel.SnippetType = model.SnippetType;

            _contentSnippetRepository.Update(dbModel);

            _contentSnippetHelper.ClearSnippetCache(model.SnippetType);

            return RedirectToAction("index");
        }

        [Route("ContentSnippetManagement/edit")]
        [HttpGet]
        public IActionResult Edit(int contentSnippetId)
        {
            var dbModel = _contentSnippetRepository.Get(contentSnippetId);

            var model = new ContentSnippetEditModel()
            {
                Content = dbModel.Content,
                ContentSnippetId = dbModel.ContentSnippetId,
                SnippetType = dbModel.SnippetType,
                
            };

            return View(model);
        }

        [Route("ContentSnippetManagement/delete")]
        [HttpPost]
        public IActionResult Delete(int contentSnippetId)
        {
            _contentSnippetRepository.Delete(contentSnippetId);

            return RedirectToAction("index");
        }
    }
}
