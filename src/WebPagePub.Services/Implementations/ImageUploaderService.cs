using System;
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

        // Inject IHttpClientFactory instead of calling new HttpClient().
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

            // ImageUtilities.ScaleImage seeks the input to 0 before reading, preserves
            // the source format (JPEG → JPEG, PNG → PNG, etc.), and returns a fresh
            // MemoryStream positioned at 0. The caller's `stream` is left intact so it
            // can be reused for additional resizes (preview, thumbnail, etc.) by the
            // SitePageManager.
            using var resizedStream = ImageUtilities.ScaleImage(stream, maxWidthPx, maxHeightPx);

            if (OptimizeImage)
            {
                var optimizer = new ImageOptimizer
                {
                    OptimalCompression = true,
                };

                // LosslessCompress reads the entire stream and writes the compressed
                // result back, leaving the position at the end. Uploading without
                // seeking would send zero bytes to blob storage. Reset to the beginning
                // so UploadAsync reads the full compressed content.
                optimizer.LosslessCompress(resizedStream);
                resizedStream.Seek(0, SeekOrigin.Begin);
            }

            await this.siteFilesRepository.UploadAsync(
                resizedStream,
                lowerQualityImageUrl.GetFileNameFromUrl(),
                folderPath,
                expiresDate);

            return new Uri(lowerQualityImageUrl);
        }

        public async Task<MemoryStream> ToStreamAsync(Uri imageUrl)
        {
            try
            {
                // Use the factory-managed client instead of new HttpClient().
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
    }
}