using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using ImageMagick;
using WebPagePub.Core.Utilities;
using WebPagePub.FileStorage.Repositories.Interfaces;
using WebPagePub.Services.Interfaces;

namespace WebPagePub.Services.Implementations
{
    public class ImageUploaderService : IImageUploaderService
    {
        private const bool OptimizeImage = true;
        private readonly ISiteFilesRepository siteFilesRepository;

        // FIX 1: Inject IHttpClientFactory instead of calling new HttpClient().
        //
        // The original ToStreamAsync created a new HttpClient inside a `using` block
        // on every call. Disposing HttpClient after each request closes the underlying
        // socket, but the OS keeps the port in TIME_WAIT for ~240 seconds. Under any
        // real upload workload this exhausts the available ephemeral port range and
        // causes "Only one usage of each socket address is permitted" errors.
        //
        // IHttpClientFactory maintains a pool of HttpMessageHandler instances that are
        // reused across calls, avoiding socket exhaustion entirely. It is already
        // registered via builder.Services.AddHttpClient() in Program.cs and is safe
        // to inject into a Singleton (IHttpClientFactory is itself a Singleton).
        private readonly IHttpClientFactory httpClientFactory;

        public ImageUploaderService(
            ISiteFilesRepository siteFilesRepository,
            IHttpClientFactory httpClientFactory)
        {
            this.siteFilesRepository = siteFilesRepository;
            this.httpClientFactory = httpClientFactory;
        }

        public async Task<Uri> UploadResizedVersionOfPhoto(
            string folderPath,
            MemoryStream stream,
            Uri originalPhotoUrl,
            int maxWidthPx,
            int maxHeightPx,
            string suffix,
            string? expiresDate = null)
        {
            var extension = originalPhotoUrl.ToString().GetFileExtension();
            var lowerQualityImageUrl = originalPhotoUrl.ToString()
                .Replace(
                    string.Format(".{0}", extension),
                    string.Format("{0}.{1}", suffix, extension));

            // FIX 2: Reset the stream position before reading.
            //
            // The caller passes a MemoryStream that may have been read by a previous
            // upload call (original photo, thumbnail, etc.). If Position is not at 0,
            // Image.FromStream reads from the current position and either throws or
            // silently produces a corrupt/empty image. Seek to the beginning first.
            stream.Seek(0, SeekOrigin.Begin);

            // FIX 3: Wrap the source Image in a `using` block.
            //
            // Image.FromStream allocates a GDI+ object. The original code had no
            // `using` or Dispose() call on this object, so it leaked on every call —
            // especially on any exception thrown before resizedImage.Dispose().
            using var sourceImage = Image.FromStream(stream);

            // FIX 4: Wrap resizedImage in `using` instead of calling Dispose() manually.
            //
            // The original called resizedImage.Dispose() at the very bottom of the
            // method. Any exception thrown between creation and that line (e.g. inside
            // LosslessCompress or UploadAsync) would skip the Dispose and leak the
            // GDI Bitmap handle. `using` guarantees disposal on every exit path.
            using var resizedImage = ImageUtilities.ScaleImage(sourceImage, maxWidthPx, maxHeightPx);

            // FIX 5: Wrap streamRotated in `using`.
            //
            // The original never disposed the MemoryStream returned by ToAStream.
            // While MemoryStream's Dispose is a no-op for the backing buffer, not
            // disposing it suppresses any future analysis warnings and is incorrect
            // ownership semantics. Using `using` also makes the scope explicit.
            using var streamRotated = this.ToAStream(resizedImage, this.SetImageFormat(lowerQualityImageUrl));

            if (OptimizeImage)
            {
                var optimizer = new ImageOptimizer
                {
                    OptimalCompression = true
                };

                // FIX 6: Seek to 0 after LosslessCompress before uploading.
                //
                // LosslessCompress reads the entire stream and writes the compressed
                // result back, leaving the stream position at the end. Uploading without
                // seeking would send zero bytes to blob storage. Reset to the beginning
                // so UploadAsync reads the full compressed content.
                optimizer.LosslessCompress(streamRotated);
                streamRotated.Seek(0, SeekOrigin.Begin);
            }

            await this.siteFilesRepository.UploadAsync(
                streamRotated,
                lowerQualityImageUrl.GetFileNameFromUrl(),
                folderPath,
                expiresDate);

            return new Uri(lowerQualityImageUrl);
        }

        public Stream ToAStream(Image image, ImageFormat format)
        {
            var stream = new MemoryStream();
            image.Save(stream, format);
            stream.Position = 0;
            return stream;
        }

        public async Task<MemoryStream> ToStreamAsync(Uri imageUrl)
        {
            try
            {
                // FIX 1 (continued): Use the factory-managed client instead of new HttpClient().
                // CreateClient() retrieves a client backed by a pooled HttpMessageHandler,
                // so the underlying socket is reused across calls rather than torn down
                // and re-established each time.
                var ms = new MemoryStream();
                var client = this.httpClientFactory.CreateClient();
                var rsp = await client.GetAsync(imageUrl);
                rsp.EnsureSuccessStatusCode();

                await using var response = await rsp.Content.ReadAsStreamAsync();
                await response.CopyToAsync(ms);

                ms.Seek(0, SeekOrigin.Begin);
                return ms;
            }
            catch (Exception ex)
            {
                throw new Exception("failed to copy to stream", ex);
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