using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using WebPagePub.Data.Repositories.Interfaces;
using WebPagePub.Web.Models;
using WebPagePub.Core.Utilities;
using WebPagePub.Data.Models.Db;
using System.Linq;
using Microsoft.Extensions.Caching.Memory;
using WebPagePub.Web.Helpers;

namespace WebPagePub.Web.Controllers
{
    [Authorize]
    public class LinkManagementController : Controller
    {
        private IMemoryCache _memoryCache;
        private readonly ILinkRedirectionRepository _linkRedirectionRepository;

        public LinkManagementController(
            ILinkRedirectionRepository linkRedirectionRepository,
            IMemoryCache memoryCache)
        {
            _memoryCache = memoryCache;
            _linkRedirectionRepository = linkRedirectionRepository;
        }

        [Route("linkmanagement")]
        public IActionResult Index()
        {
            var allLinks = _linkRedirectionRepository.GetAll();
            var model = new LinkListModel();

            allLinks = allLinks.OrderByDescending(x => x.CreateDate).ToList();

            foreach (var link in allLinks)
            {
                model.Items.Add(new LinkEditModel()
                {
                    LinkKey = link.LinkKey,
                    LinkRedirectionId = link.LinkRedirectionId,
                    UrlDestination = link.UrlDestination
                });
            }

            return View(model);
        }

        [Route("linkmanagement/create")]
        [HttpPost]
        public IActionResult Create(LinkEditModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            _linkRedirectionRepository.Create(new LinkRedirection()
            {
                LinkKey = model.LinkKey.UrlKey(),
                UrlDestination = model.UrlDestination
            });

            return RedirectToAction("index");
        }

        [Route("linkmanagement/create")]
        [HttpGet]
        public IActionResult Create()
        {
            return View(new LinkEditModel());
        }

        [Route("linkmanagement/edit")]
        [HttpPost]
        public IActionResult Edit(LinkEditModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var linkDbModel = _linkRedirectionRepository.Get(model.LinkRedirectionId);

            linkDbModel.LinkKey = model.LinkKey.UrlKey();
            linkDbModel.UrlDestination = model.UrlDestination.Trim();

            _linkRedirectionRepository.Update(linkDbModel);

            var cacheKey = CacheHelper.GetLinkCacheKey(linkDbModel.LinkKey);
            _memoryCache.Remove(cacheKey);

            return RedirectToAction("index");
        }

        [Route("linkmanagement/edit")]
        [HttpGet]
        public IActionResult Edit(int linkRedirectionId)
        {
            var linkDbModel = _linkRedirectionRepository.Get(linkRedirectionId);

            var model = new LinkEditModel()
            {
                LinkKey = linkDbModel.LinkKey,
                LinkRedirectionId = linkDbModel.LinkRedirectionId,
                UrlDestination = linkDbModel.UrlDestination
            };

            return View(model);
        }

        [Route("linkmanagement/delete")]
        [HttpPost]
        public IActionResult Delete(int linkRedirectionId)
        {
            _linkRedirectionRepository.Delete(linkRedirectionId);

            return RedirectToAction("index");
        }
    }
}
