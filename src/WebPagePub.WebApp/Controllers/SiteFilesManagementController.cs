using System.Reflection;
using log4net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebPagePub.Data.Repositories.Interfaces;
using WebPagePub.Web.Models;

namespace WebPagePub.Web.Controllers
{
    [Authorize(Roles = WebPagePub.Data.Constants.StringConstants.AdminRole)]
    public class SiteFilesManagementController : Controller
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly ISiteFilesRepository siteFilesRepository;

        public SiteFilesManagementController(ISiteFilesRepository siteFilesRepository)
        {
            this.siteFilesRepository = siteFilesRepository;
        }

        [Route("sitefilesmanagement/upload")]
        [HttpGet]
        public ActionResult Upload(string folderPath = null)
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
                        await this.siteFilesRepository.UploadAsync(file, folderPath);
                    }
                }

                return this.RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                Log.Fatal(ex);
                return this.RedirectToAction(nameof(Index));
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

                    await this.siteFilesRepository.CreateFolderAsync(folderName, currentDirectory);
                }

                return this.RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                Log.Fatal(ex);
                return this.RedirectToAction(nameof(Index));
            }
        }

        [Route("sitefilesmanagement")]
        [HttpGet]
        public ActionResult Index(string? folderPath = null)
        {
            var directory = this.siteFilesRepository.ListFiles(folderPath);

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

            return this.View(model);
        }

        [Route("sitefilesmanagement/DeleteFileAsync")]
        [HttpGet]
        public async Task<ActionResult> DeleteFileAsync(string fileUrl)
        {
            await this.siteFilesRepository.DeleteFileAsync(fileUrl);

            return this.RedirectToAction(nameof(Index));
        }

        [Route("sitefilesmanagement/DeleteFolderAsync")]
        [HttpGet]
        public async Task<ActionResult> DeleteFolderAsync(string folderUrl)
        {
            await this.siteFilesRepository.DeleteFolderAsync(folderUrl);

            return this.RedirectToAction(nameof(Index));
        }
    }
}
