using System;
using System.IO;
using System.Threading.Tasks;

namespace WebPagePub.Services.Interfaces
{
    public interface IImageUploaderService
    {
        Task<Uri> UploadResizedVersionOfPhoto(
            string folderPath,
            MemoryStream stream,
            Uri originalPhotoUrl,
            int maxWidthPx,
            int maxHeightPx,
            string suffix,
            string? expiresDate = null);

        Task<MemoryStream> ToStreamAsync(Uri imageUrl);
    }
}