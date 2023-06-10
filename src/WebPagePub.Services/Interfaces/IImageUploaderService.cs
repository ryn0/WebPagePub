using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading.Tasks;

namespace WebPagePub.Services.Interfaces
{
    public interface IImageUploaderService
    {
        Task<Uri> UploadResizedVersionOfPhoto(string folderPath, MemoryStream stream, Uri originalPhotoUrl, int maxWidthPx, int maxHeightPx, string suffix);

        Stream ToAStream(Image image, ImageFormat formaw);

        Task<MemoryStream> ToStreamAsync(Uri imageUrl);

        ImageFormat SetImageFormat(string photoUrl);
    }
}
