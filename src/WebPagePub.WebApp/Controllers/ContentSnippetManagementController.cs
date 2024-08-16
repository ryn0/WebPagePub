using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using WebPagePub.Data.Models.Db;
using WebPagePub.Data.Repositories.Interfaces;
using WebPagePub.Services.Interfaces;
using WebPagePub.WebApp.Models.ContentSnippet;

namespace WebPagePub.Web.Controllers
{
    [Authorize]
    public class ContentSnippetManagementController : Controller
    {
        private readonly IContentSnippetRepository contentSnippetRepository;
        private readonly ICacheService contentSnippetHelper;

        public ContentSnippetManagementController(
            IContentSnippetRepository contentSnippetRepository,
            ICacheService contentSnippetHelper)
        {
            this.contentSnippetRepository = contentSnippetRepository;
            this.contentSnippetHelper = contentSnippetHelper;
        }

        [Route("ContentSnippetManagement")]
        public IActionResult Index()
        {
            var allSnippets = this.contentSnippetRepository.GetAll().OrderBy(x => x.SnippetType.ToString());
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

            return this.View(model);
        }

        [Route("ContentSnippetManagement/create")]
        [HttpPost]
        public IActionResult Create(ContentSnippetEditModel model)
        {
            if (!this.ModelState.IsValid)
            {
                return this.View(model);
            }

            var dbModel = this.contentSnippetRepository.Get(model.ContentSnippetId);

            if (dbModel != null)
            {
                throw new Exception("type already exists");
            }

            this.contentSnippetRepository.Create(new ContentSnippet()
            {
                Content = model.Content?.Trim(),
                ContentSnippetId = model.ContentSnippetId,
                SnippetType = model.SnippetType
            });

            return this.RedirectToAction("index");
        }

        [Route("ContentSnippetManagement/create")]
        [HttpGet]
        public IActionResult Create()
        {
            // Get all existing snippets and order them alphabetically by their SnippetType as a string
            var existingSnippets = this.contentSnippetRepository.GetAll().OrderBy(x => x.SnippetType.ToString());

            // Initialize the model with a default SnippetType
            var model = new ContentSnippetEditModel()
            {
                SnippetType = Data.Enums.SiteConfigSetting.Unknown
            };

            // Get all enum names, sort them alphabetically, and iterate over them
            var sortedSnippetTypes = Enum.GetNames(typeof(Data.Enums.SiteConfigSetting)).OrderBy(snippetType => snippetType);

            foreach (string snippetType in sortedSnippetTypes)
            {
                // Check if the snippet type is already in use
                if (existingSnippets.Any(x => x.SnippetType.ToString() == snippetType))
                {
                    continue; // Skip the snippet type if it's already used
                }

                // Add the unused snippet type to the model's UnusedSnippetTypes list
                model.UnusedSnippetTypes.Add(new SelectListItem()
                {
                    Text = snippetType,
                    Value = snippetType,
                });
            }

            // Return the view with the model
            return this.View(model);
        }

        [Route("ContentSnippetManagement/edit")]
        [HttpPost]
        public IActionResult Edit(ContentSnippetEditModel model)
        {
            if (!this.ModelState.IsValid)
            {
                return this.View(model);
            }

            model.Content ??= string.Empty;

            var dbModel = this.contentSnippetRepository.Get(model.ContentSnippetId);

            dbModel.Content = model.Content.Trim();
            dbModel.SnippetType = model.SnippetType;

            this.contentSnippetRepository.Update(dbModel);

            this.contentSnippetHelper.ClearSnippetCache(model.SnippetType);

            return this.RedirectToAction("index");
        }

        [Route("ContentSnippetManagement/edit")]
        [HttpGet]
        public IActionResult Edit(int contentSnippetId)
        {
            var dbModel = this.contentSnippetRepository.Get(contentSnippetId);

            var model = new ContentSnippetEditModel()
            {
                Content = dbModel.Content,
                ContentSnippetId = dbModel.ContentSnippetId,
                SnippetType = dbModel.SnippetType,
            };

            return this.View(model);
        }

        [Route("ContentSnippetManagement/delete")]
        [HttpPost]
        public IActionResult Delete(int contentSnippetId)
        {
            this.contentSnippetRepository.Delete(contentSnippetId);

            return this.RedirectToAction("index");
        }
    }
}
