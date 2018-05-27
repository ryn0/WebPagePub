using Microsoft.AspNetCore.Mvc;
using WebPagePub.Core.Utilities;
using WebPagePub.Data.Enums;
using WebPagePub.Services.Interfaces;
using System.IO;
using System.Net;

namespace WebPagePub.Web.Controllers
{
    public class FaviconController : Controller
    {
        const string FavIconPath = "/sitecontent/icons/";
        const string FavIconAppleIcon57x57 = "apple-icon-57x57.png";
        const string FavIconAppleIcon60x60 = "apple-icon-60x60.png";
        const string FavIconAppleIcon72x72 = "apple-icon-72x72.png";
        const string FavIconAppleIcon76x76 = "apple-icon-76x76.png";
        const string FavIconAppleIcon114x114 = "apple-icon-114x114.png";
        const string FavIconAppleIcon120x120 = "apple-icon-120x120.png";
        const string FavIconAppleIcon144x144 = "apple-icon-144x144.png";
        const string FavIconAppleIcon152x152 = "apple-icon-152x152.png";
        const string FavIconAppleIcon180x180 = "apple-icon-180x180.png";
        const string FavIconAndroid192x192 = "android-icon-192x192.png";
        const string FavIcon32x32 = "favicon-32x32.png";
        const string FavIcon96x96 = "favicon-96x96.png";
        const string FavIcon16x16 = "favicon-16x16.png";
        const string FavIconMs144x144 = "ms-icon-144x144.png";
        const string FavIconMaifestJson = "manifest.json";
        const string FavIconIco = "favicon.ico";
        private readonly ICacheService _cacheService;

        public FaviconController(ICacheService cacheService)
        {
            _cacheService = cacheService;
        }
 
        [Route(FavIconAppleIcon57x57)]
        [Route(FavIconAppleIcon60x60)]
        [Route(FavIconAppleIcon72x72)]
        [Route(FavIconAppleIcon76x76)]
        [Route(FavIconAppleIcon114x114)]
        [Route(FavIconAppleIcon120x120)]
        [Route(FavIconAppleIcon144x144)]
        [Route(FavIconAppleIcon152x152)]
        [Route(FavIconAppleIcon180x180)]
        [Route(FavIconAndroid192x192)]
        [Route(FavIcon32x32)]
        [Route(FavIcon96x96)]
        [Route(FavIcon16x16)]
        [Route(FavIconMs144x144)]
        [Route(FavIconMs144x144)]
        [Route(FavIconMaifestJson)]
        [Route(FavIconIco)]
        [HttpGet]
        public IActionResult FavIcon()
        {
            return ReturnContent(Request.Path.Value);
        }

        private IActionResult ReturnContent(string fileName)
        {
            fileName = fileName.TrimStart('/');

            try
            {
                using (WebClient wc = new WebClient())
                {
                    var bytes = wc.DownloadData(BuildPath(fileName));
                    var ms = new MemoryStream(bytes);

                    switch (fileName.GetFileExtensionLower())
                    {
                        case "png":
                            Response.Headers.Add("Cache-Control", "public, max-age=604800");
                            return File(ms, "image/png");
                        case "json":
                            return File(ms, "application/json");
                        case "ico":
                            return File(ms, "image/x-icon");
                        default:
                            return File(ms, "text/plain");
                    }
                }
            }
            catch 
            {
                return StatusCode(404);
            }
        }
 
        public string BuildPath(string fileName)
        {
            var cdnPrefix = _cacheService.GetSnippet(SiteConfigSetting.CdnPrefixWithProtocol);
            return $"{cdnPrefix}{FavIconPath}{fileName}";
        }
    }
}
