using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WebPagePub.Core.Utilities;
using WebPagePub.Data.Constants;
using WebPagePub.Data.Models;
using WebPagePub.Data.Repositories.Interfaces;
using WebPagePub.FileStorage.Repositories.Interfaces;
using WebPagePub.Services.Interfaces;
using WebPagePub.WebApp.Models.Author;

namespace WebPagePub.WebApp.Controllers
{
    [Authorize(Roles = StringConstants.AdminRole)]
    public class AuthorManagementController : Controller
    {
        private readonly IAuthorRepository authorRepository;
        private readonly IImageUploaderService imageUploaderService;
        private readonly ISiteFilesRepository siteFilesRepository;
        private readonly UserManager<ApplicationUser> userManager;

        public AuthorManagementController(
            IAuthorRepository authorRepository,
            IImageUploaderService imageUploaderService,
            ISiteFilesRepository siteFilesRepository,
            UserManager<ApplicationUser> userManager)
        {
            this.authorRepository = authorRepository;
            this.imageUploaderService = imageUploaderService;
            this.siteFilesRepository = siteFilesRepository;
            this.userManager = userManager;
        }

        [Route("AuthorManagement")]
        [HttpGet]
        public IActionResult Index()
        {
            var dbModel = this.authorRepository.GetAll();

            var model = new AuthorListModel()
            {
            };

            foreach (var item in dbModel)
            {
                model.Items.Add(new AuthorItem(
                    item.AuthorId,
                    item.FirstName,
                    item.LastName));
            }

            return this.View(model);
        }

        [Route("authormanagement/edit/{authorId}")]
        [HttpGet]
        public IActionResult Edit(int authorId)
        {
            var dbModel = this.authorRepository.Get(authorId);

            var model = new AuthorEditModel()
            {
                AuthorBio = dbModel.AuthorBio,
                AuthorId = dbModel.AuthorId,
                FirstName = dbModel.FirstName,
                LastName = dbModel.LastName,
                PhotoFullScreenUrl = dbModel.PhotoFullScreenUrl,
                PhotoOriginalUrl = dbModel.PhotoOriginalUrl,
                PhotoPreviewUrl = dbModel.PhotoPreviewUrl,
                PhotoThumbUrl = dbModel.PhotoThumbUrl,
            };

            return this.View(model);
        }

        [Route("authormanagement/edit/{authorId}")]
        [HttpPost]
        public IActionResult Edit(AuthorEditModel model)
        {
            var dbModel = this.authorRepository.Get(model.AuthorId);

            dbModel.AuthorBio = model.AuthorBio?.Trim();
            dbModel.FirstName = model.FirstName?.Trim();
            dbModel.LastName = model.LastName?.Trim();

            this.authorRepository.Update(dbModel);

            return this.RedirectToAction(nameof(this.Index));
        }

        [Route("authormanagement/create")]
        [HttpGet]
        public IActionResult Create()
        {
            var model = new AuthorCreateModel()
            {
            };

            return this.View(nameof(this.Create));
        }

        [Route("authormanagement/create")]
        [HttpPost]
        public IActionResult Create(AuthorCreateModel model)
        {
            if (!this.ModelState.IsValid)
            {
                throw new Exception();
            }

            this.authorRepository.Create(new Data.Models.Db.Author()
            {
                FirstName = model.FirstName.Trim(),
                LastName = model.LastName.Trim(),
                AuthorBio = model?.AuthorBio?.Trim()
            });

            return this.RedirectToAction(nameof(this.Index));
        }

        [Route("authormanagement/uploadphoto/{AuthorId}")]
        [HttpGet]
        public IActionResult UploadPhoto(int authorId)
        {
            var model = new AuthorPhotoUploadModel()
            {
                AuthorId = authorId
            };

            return this.View(nameof(this.UploadPhoto), model);
        }

        [Route("authormanagement/uploadphoto/{authorId}")]
        [HttpPost]
        public async Task<ActionResult> UploadPhotoAsync(IFormFile file, int authorId)
        {
            if (file == null)
            {
                return this.RedirectToAction(nameof(this.Edit), new { AuthorId = authorId });
            }

            var author = this.authorRepository.Get(authorId);

            try
            {
                await this.UploadPhotoAsync(file, author);

                return this.RedirectToAction(nameof(this.Edit), new { AuthorId = authorId });
            }
            catch (Exception ex)
            {
                throw new Exception("Upload failed", ex.InnerException);
            }
        }

        [Route("authormanagement/deleteauthorphoto/{AuthorId}")]
        [HttpGet]
        public async Task<IActionResult> DeleteAuthorPhotoAsync(int authorId)
        {
            var author = this.authorRepository.Get(authorId);

            await this.DeleteExistingPhotos(author);

            return this.RedirectToAction(nameof(this.Index));
        }

        private async Task UploadPhotoAsync(IFormFile file, Data.Models.Db.Author author)
        {
            var fileExtension = file.FileName.GetFileExtension().ToLower();
            var folderPath = this.GetAuthorPhotoFolder(author.AuthorId);
            var authorNameForUrl = string.Format("{0} {1}", author.FirstName, author.LastName).UrlKey();
            var authorFileName = string.Format("{0}.{1}", authorNameForUrl, fileExtension);
            var memoryStream = new MemoryStream();
            await file.CopyToAsync(memoryStream);
            memoryStream.Seek(0, SeekOrigin.Begin);

            await this.UploadSizesOfPhotos(author, folderPath, memoryStream, authorFileName);
        }

        private string GetAuthorPhotoFolder(int authorId)
        {
            return $"/{StringConstants.AuthorPhotoBlobPhotoName}/{authorId}/";
        }

        private async Task UploadSizesOfPhotos(
                Data.Models.Db.Author author,
                string folderPath,
                MemoryStream memoryStream,
                string fileName)
        {
            await this.DeleteExistingPhotos(author);

            var originalPhotoUrl = await this.siteFilesRepository.UploadAsync(memoryStream, fileName, folderPath);
            var thumbnailPhotoUrl = await this.imageUploaderService.UploadResizedVersionOfPhoto(folderPath, memoryStream, originalPhotoUrl, 300, 200, StringConstants.SuffixThumb);
            var fullScreenPhotoUrl = await this.imageUploaderService.UploadResizedVersionOfPhoto(folderPath, memoryStream, originalPhotoUrl, 1600, 1200, StringConstants.SuffixFullscreen);
            var previewPhotoUrl = await this.imageUploaderService.UploadResizedVersionOfPhoto(folderPath, memoryStream, originalPhotoUrl, 800, 600, StringConstants.SuffixPrevew);
            memoryStream.Dispose();

            this.UpdateAuthorPhoto(
                author,
                originalPhotoUrl,
                thumbnailPhotoUrl,
                fullScreenPhotoUrl,
                previewPhotoUrl);
        }

        private void UpdateAuthorPhoto(
            Data.Models.Db.Author author,
            Uri originalPhotoUrl,
            Uri thumbnailPhotoUrl,
            Uri fullScreenPhotoUrl,
            Uri previewPhotoUrl)
        {
            author.PhotoOriginalUrl = originalPhotoUrl.ToString();
            author.PhotoThumbUrl = thumbnailPhotoUrl.ToString();
            author.PhotoFullScreenUrl = fullScreenPhotoUrl.ToString();
            author.PhotoPreviewUrl = previewPhotoUrl.ToString();

            this.authorRepository.Update(author);
        }

        private async Task DeleteExistingPhotos(Data.Models.Db.Author author)
        {
            await this.siteFilesRepository.DeleteFileAsync(author.PhotoOriginalUrl);
            author.PhotoOriginalUrl = string.Empty;
            await this.siteFilesRepository.DeleteFileAsync(author.PhotoThumbUrl);
            author.PhotoThumbUrl = string.Empty;
            await this.siteFilesRepository.DeleteFileAsync(author.PhotoFullScreenUrl);
            author.PhotoFullScreenUrl = string.Empty;
            await this.siteFilesRepository.DeleteFileAsync(author.PhotoPreviewUrl);
            author.PhotoPreviewUrl = string.Empty;

            this.authorRepository.Update(author);
        }
    }
}