using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading.Tasks;

namespace WebPagePub.Services.Interfaces
{
    public interface IImageUploaderService
    {
        Task<Uri> UploadReducedQualityImage(string folderPath, Uri fullsizePhotoUrl, int maxWidthPx, int maxHeightPx, string suffix);

        Stream ToAStream(Image image, ImageFormat formaw);

        Task<MemoryStream> ToStreamAsync(string imageUrl);

        ImageFormat SetImageFormat(string photoUrl);
    }
}
