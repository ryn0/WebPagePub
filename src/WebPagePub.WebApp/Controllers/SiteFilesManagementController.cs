﻿using System.Reflection;
using log4net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebPagePub.Core;
using WebPagePub.Data.Enums;
using WebPagePub.FileStorage.Repositories.Interfaces;
using WebPagePub.Services.Interfaces;
using WebPagePub.Web.Models;
using WebPagePub.WebApp.Constants;

namespace WebPagePub.Web.Controllers
{
    [Authorize(Roles = WebPagePub.Data.Constants.StringConstants.AdminRole)]
    public class SiteFilesManagementController : Controller
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod()?.DeclaringType ?? typeof(SiteFilesManagementController));

        private readonly ISiteFilesRepository siteFilesRepository;
        private readonly ICacheService cacheService;

        public SiteFilesManagementController(
            ISiteFilesRepository siteFilesRepository,
            ICacheService cacheService)
        {
            this.siteFilesRepository = siteFilesRepository;
            this.cacheService = cacheService;
        }

        [Route("sitefilesmanagement/upload")]
        [HttpGet]
        public ActionResult Upload(string? folderPath = null)
        {
            this.ViewBag.UploadFolderPath = folderPath;

            return this.View();
        }

        [Route("sitefilesmanagement/uploadasync")]
        [HttpPost]
        public async Task<ActionResult> UploadAsync(IEnumerable<IFormFile> files, string? folderPath = null)
        {
            try
            {
                foreach (var file in files)
                {
                    if (file != null && file.Length > 0)
                    {
                        using var stream = file.OpenReadStream();
                        await this.siteFilesRepository.UploadAsync(stream, file.FileName, folderPath);
                    }
                }

                return this.RedirectToSiteMagementIndex();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex);

                return this.RedirectToSiteMagementIndex();
            }
        }

        [Route("sitefilesmanagement/CreateFolderAsync")]
        [HttpPost]
        public async Task<ActionResult> CreateFolderAsync(string folderName, string? currentDirectory = null)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(folderName))
                {
                    folderName = folderName.Trim();

                    if (folderName == Data.Constants.StringConstants.ContainerName)
                    {
                        throw new Exception($"do not create a folder with this name {Data.Constants.StringConstants.ContainerName}");
                    }

                    await this.siteFilesRepository.CreateFolderAsync(folderName, currentDirectory);
                }

                return this.RedirectToSiteMagementIndex();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex);
                return this.RedirectToSiteMagementIndex();
            }
        }

        [Route("sitefilesmanagement")]
        [HttpGet]
        public async Task<ActionResult> IndexAsync(string? folderPath = null)
        {
            var directory = await this.siteFilesRepository.ListFilesAsync(folderPath);

            var model = new SiteFileListModel();

            foreach (var file in directory.FileItems)
            {
                var item = new SiteFileItem
                {
                    FilePath = file.FilePath ?? string.Empty,
                    FolderName = file.FolderName ?? string.Empty,
                    FolderPathFromRoot = file.FolderPathFromRoot ?? string.Empty,
                    IsFolder = file.IsFolder
                };

                if (!file.IsFolder)
                {
                    if (string.IsNullOrWhiteSpace(file.FilePath))
                    {
                        throw new ArgumentNullException($"{nameof(file.FilePath)}");
                    }

                    item.CdnLink = this.ConvertBlobToCdnUrl(file.FilePath);
                }

                model.FileItems.Add(item);
            }

            var folders = model.FileItems.Where(x => x.IsFolder == true).OrderBy(x => x.FolderName).ToList();
            var files = model.FileItems.Where(x => x.IsFolder == false).OrderBy(x => x.FilePath).ToList();

            model.FileItems = new List<SiteFileItem>();
            model.FileItems.AddRange(folders);
            model.FileItems.AddRange(files);

            if (folderPath != null)
            {
                model.CurrentDirectory = folderPath;
                var lastPath = folderPath.Split('/')[^2];

                if (string.IsNullOrWhiteSpace(lastPath))
                {
                    model.ParentDirectory = string.Empty;
                }
                else
                {
                    var lastPart = lastPath + "/";
                    var startIndex = folderPath.IndexOf(lastPart);
                    model.ParentDirectory = folderPath.Remove(startIndex, lastPart.Length);
                }
            }

            return this.View(model);
        }

        [Route("sitefilesmanagement/DeleteFileAsync")]
        [HttpGet]
        public async Task<ActionResult> DeleteFileAsync(string fileUrl)
        {
            await this.siteFilesRepository.DeleteFileAsync(fileUrl);

            return this.RedirectToSiteMagementIndex();
        }

        [Route("sitefilesmanagement/DeleteFolderAsync")]
        [HttpGet]
        public async Task<ActionResult> DeleteFolderAsync(string folderUrl)
        {
            await this.siteFilesRepository.DeleteFolderAsync(folderUrl);

            return this.RedirectToSiteMagementIndex();
        }

        private string ConvertBlobToCdnUrl(string filePath)
        {
            var blobPrefix = this.cacheService.GetSnippet(SiteConfigSetting.BlobPrefix);
            var cdnPrefix = this.cacheService.GetSnippet(SiteConfigSetting.CdnPrefixWithProtocol);

            return UrlBuilder.ConvertBlobToCdnUrl(filePath, blobPrefix, cdnPrefix);
        }

        private ActionResult RedirectToSiteMagementIndex()
        {
            return this.RedirectToAction("Index", "SiteFilesManagement");
        }
    }
}