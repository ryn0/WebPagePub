using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using ImageMagick;
using WebPagePub.Core.Utilities;
using WebPagePub.Data.Repositories.Interfaces;
using WebPagePub.Services.Interfaces;

namespace WebPagePub.Services.Implementations
{
    public class ImageUploaderService : IImageUploaderService
    {
        private const bool OptimizeImage = true;
        private readonly ISiteFilesRepository siteFilesRepository;

        public ImageUploaderService(ISiteFilesRepository siteFilesRepository)
        {
            this.siteFilesRepository = siteFilesRepository;
        }

        public async Task<Uri> UploadResizedVersionOfPhoto(string folderPath, MemoryStream stream, Uri originalPhotoUrl, int maxWidthPx, int maxHeightPx, string suffix)
        {
            var extension = originalPhotoUrl.ToString().GetFileExtension();
            var resizedImage = ImageUtilities.ScaleImage(Image.FromStream(stream), maxWidthPx, maxHeightPx);
            var lowerQualityImageUrl = originalPhotoUrl.ToString()
                                                       .Replace(string.Format(".{0}", extension), string.Format("{0}.{1}", suffix, extension));
            var streamRotated = this.ToAStream(resizedImage, this.SetImageFormat(lowerQualityImageUrl));

            if (OptimizeImage)
            {
                var optimizer = new ImageOptimizer
                {
                    OptimalCompression = true
                };
                optimizer.LosslessCompress(streamRotated);
            }

            await this.siteFilesRepository.UploadAsync(
                                        streamRotated,
                                        lowerQualityImageUrl.GetFileNameFromUrl(),
                                        folderPath);

            resizedImage.Dispose();

            return new Uri(lowerQualityImageUrl);
        }

        public Stream ToAStream(Image image, ImageFormat formaw)
        {
            try
            {
                var stream = new MemoryStream();
                image.Save(stream, formaw);
                stream.Position = 0;

                return stream;
            }
            catch
            {
                throw new Exception("failed to save to stream");
            }
        }

        public async Task<MemoryStream> ToStreamAsync(Uri imageUrl)
        {
            try
            {
                var ms = new MemoryStream();

                using (var client = new HttpClient())
                {
                    var rsp = await client.GetAsync(imageUrl);
                    var response = await rsp.Content.ReadAsStreamAsync();
                    await response.CopyToAsync(ms);
                }

                ms.Seek(0, SeekOrigin.Begin);

                return ms;
            }
            catch
            {
                throw new Exception("failed to copy to stream");
            }
        }

        public ImageFormat SetImageFormat(string photoUrl)
        {
            var extension = photoUrl.GetFileExtensionLower();

            return extension switch
            {
                "jpg" or "jpeg" => ImageFormat.Jpeg,
                "png" => ImageFormat.Png,
                "gif" => ImageFormat.Gif,
                _ => ImageFormat.Jpeg,
            };
        }
    }
}
