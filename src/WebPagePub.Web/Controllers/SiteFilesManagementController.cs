using WebPagePub.Data.Repositories.Interfaces;
using WebPagePub.Web.Models;
using log4net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace WebPagePub.Web.Controllers
{
    [Authorize(Roles = WebPagePub.Data.Constants.StringConstants.AdminRole)]
    public class SiteFilesManagementController : Controller
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly ISiteFilesRepository _siteFilesRepository;

        public SiteFilesManagementController(ISiteFilesRepository siteFilesRepository)
        {
            _siteFilesRepository = siteFilesRepository;
        }

        [Route("sitefilesmanagement/upload")]
        [HttpGet]
        public ActionResult Upload(string folderPath = null)
        {
            ViewBag.UploadFolderPath = folderPath;

            return View();
        }

        [Route("sitefilesmanagement/uploadasync")]
        [HttpPost]
        public async Task<ActionResult> UploadAsync(IEnumerable<IFormFile> files, string folderPath = null)
        {
            try
            {
                foreach (var file in files)
                {
                    if (file != null && file.Length > 0)
                    {
                        await _siteFilesRepository.UploadAsync(file, folderPath);
                    }
                }

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                log.Fatal(ex);
                return RedirectToAction("Index");
            }
        }

        [Route("sitefilesmanagement/CreateFolderAsync")]
        [HttpPost]
        public async Task<ActionResult> CreateFolderAsync(string folderName, string currentDirectory = null)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(folderName))
                {
                    folderName = folderName.Trim();

                    await _siteFilesRepository.CreateFolderAsync(folderName, currentDirectory);
                }

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                log.Fatal(ex);
                return RedirectToAction("Index");
            }
        }

        [Route("sitefilesmanagement")]
        [HttpGet]
        public ActionResult Index(string folderPath = null)
        {
            var directory = _siteFilesRepository.ListFiles(folderPath);

            var model = new SiteFileListModel();

            foreach (var file in directory.FileItems)
            {
                model.FileItems.Add(new SiteFileItem
                {
                    FilePath = file.FilePath,
                    FolderName = file.FolderName,
                    FolderPathFromRoot = file.FolderPathFromRoot,
                    IsFolder = file.IsFolder
                });
            }

            var folders = model.FileItems.Where(x => x.IsFolder == true).OrderBy(x => x.FolderName).ToList();
            var files = model.FileItems.Where(x => x.IsFolder == false).OrderBy(x => x.FilePath).ToList();

            model.FileItems = new List<SiteFileItem>();
            model.FileItems.AddRange(folders);
            model.FileItems.AddRange(files);

            if (folderPath != null)
            {
                model.CurrentDirectory = folderPath;
                var lastPath = folderPath.Split('/')[folderPath.Split('/').Length - 2];

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

            return View(model);
        }

        [Route("sitefilesmanagement/DeleteFileAsync")]
        [HttpGet]
        public async Task<ActionResult> DeleteFileAsync(string fileUrl)
        {
            await _siteFilesRepository.DeleteFileAsync(fileUrl);

            return RedirectToAction("Index");
        }

        [Route("sitefilesmanagement/DeleteFolderAsync")]
        [HttpGet]
        public async  Task<ActionResult> DeleteFolderAsync(string folderUrl)
        {
            await _siteFilesRepository.DeleteFolderAsync(folderUrl);

            return RedirectToAction("Index");
        }
    }
}
