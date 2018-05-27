using ImageMagick;
using WebPagePub.Core.Utilities;
using WebPagePub.Data.Repositories.Interfaces;
using WebPagePub.Services.Interfaces;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading.Tasks;

namespace WebPagePub.Services.Implementations
{
    public class ImageUploaderService : IImageUploaderService
    {
        const bool OptimizeImage = true;
        private readonly ISiteFilesRepository _siteFilesRepository;

        public ImageUploaderService(ISiteFilesRepository siteFilesRepository)
        {
            _siteFilesRepository = siteFilesRepository;
        }

        public async Task<Uri> UploadReducedQualityImage(string folderPath, Uri fullsizePhotoUrl, int maxWidthPx, int maxHeightPx, string suffix)
        {
            var stream = await ToStreamAsync(fullsizePhotoUrl.ToString());
            var imageHelper = new ImageUtilities();
            var extension = fullsizePhotoUrl.ToString().GetFileExtension();
            var resizedImage = imageHelper.ScaleImage(Image.FromStream(stream), maxWidthPx, maxHeightPx);
            var lowerQualityImageUrl = fullsizePhotoUrl.ToString().Replace(string.Format(".{0}", extension), string.Format("{0}.{1}", suffix, extension));
            var streamRotated = ToAStream(resizedImage, SetImageFormat(lowerQualityImageUrl));

            if (OptimizeImage)
            {
                var optimizer = new ImageOptimizer
                {
                    OptimalCompression = true
                };
                optimizer.LosslessCompress(streamRotated);
            }

            await _siteFilesRepository.UploadAsync(
                                        streamRotated,
                                        lowerQualityImageUrl.GetFileNameFromUrl(),
                                        folderPath);

            stream.Dispose();
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
            catch {
                throw new Exception("failed to save to stream");
            }
        }


        public async Task<MemoryStream> ToStreamAsync(string imageUrl)
        {
            try
            {
                var request = System.Net.WebRequest.Create(imageUrl);
                var response = await request.GetResponseAsync();
                var responseStream = response.GetResponseStream();
                var ms = new MemoryStream();

                await responseStream.CopyToAsync(ms);

                ms.Seek(0, SeekOrigin.Begin);

                return ms;
            }
            catch {
                throw new Exception("failed to copy to stream");
            }
        }


        public ImageFormat SetImageFormat(string photoUrl)
        {
            var extension = photoUrl.GetFileExtensionLower();

            switch (extension)
            {
                case "jpg":
                case "jpeg":
                    return ImageFormat.Jpeg;
                case "png":
                    return ImageFormat.Png;
                case "gif":
                    return ImageFormat.Gif;
                default:
                    return ImageFormat.Jpeg;
            }
        }

    }
}
