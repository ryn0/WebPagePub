﻿using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using WebPagePub.Core.Utilities;
using WebPagePub.Data.Models.Db;
using WebPagePub.Data.Repositories.Interfaces;
using WebPagePub.Web.Helpers;
using WebPagePub.Web.Models;

namespace WebPagePub.Web.Controllers
{
    [Authorize]
    public class LinkManagementController : Controller
    {
        private readonly ILinkRedirectionRepository linkRedirectionRepository;
        private IMemoryCache memoryCache;

        public LinkManagementController(
            ILinkRedirectionRepository linkRedirectionRepository,
            IMemoryCache memoryCache)
        {
            this.memoryCache = memoryCache;
            this.linkRedirectionRepository = linkRedirectionRepository;
        }

        [Route("linkmanagement")]
        public IActionResult Index()
        {
            var allLinks = this.linkRedirectionRepository.GetAll();
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

            return this.View(model);
        }

        [Route("linkmanagement/create")]
        [HttpPost]
        public IActionResult Create(LinkEditModel model)
        {
            if (!this.ModelState.IsValid)
            {
                return this.View(model);
            }

            this.linkRedirectionRepository.Create(new LinkRedirection()
            {
                LinkKey = model.LinkKey.UrlKey(),
                UrlDestination = model.UrlDestination
            });

            return this.RedirectToAction("index");
        }

        [Route("linkmanagement/create")]
        [HttpGet]
        public IActionResult Create()
        {
            return this.View(new LinkEditModel());
        }

        [Route("linkmanagement/edit")]
        [HttpPost]
        public IActionResult Edit(LinkEditModel model)
        {
            if (!this.ModelState.IsValid)
            {
                return this.View(model);
            }

            var linkDbModel = this.linkRedirectionRepository.Get(model.LinkRedirectionId);

            linkDbModel.LinkKey = model.LinkKey.UrlKey();
            linkDbModel.UrlDestination = model.UrlDestination.Trim();

            this.linkRedirectionRepository.Update(linkDbModel);

            var cacheKey = CacheHelper.GetLinkCacheKey(linkDbModel.LinkKey);
            this.memoryCache.Remove(cacheKey);

            return this.RedirectToAction("index");
        }

        [Route("linkmanagement/edit")]
        [HttpGet]
        public IActionResult Edit(int linkRedirectionId)
        {
            var linkDbModel = this.linkRedirectionRepository.Get(linkRedirectionId);

            var model = new LinkEditModel()
            {
                LinkKey = linkDbModel.LinkKey,
                LinkRedirectionId = linkDbModel.LinkRedirectionId,
                UrlDestination = linkDbModel.UrlDestination
            };

            return this.View(model);
        }

        [Route("linkmanagement/delete")]
        [HttpPost]
        public IActionResult Delete(int linkRedirectionId)
        {
            this.linkRedirectionRepository.Delete(linkRedirectionId);

            return this.RedirectToAction("index");
        }
    }
}
