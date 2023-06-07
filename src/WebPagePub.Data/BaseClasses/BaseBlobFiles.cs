using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Shared.Protocol;

namespace WebPagePub.Data.BaseClasses
{
    public class BaseBlobFiles
    {
        protected async Task SetPropertiesAsync(CloudBlockBlob blockBlob, string extension)
        {
            switch (extension.ToLower())
            {
                // images
                case "png":
                    blockBlob.Properties.ContentType = "image/png";
                    break;
                case "jpeg":
                case "jpg":
                    blockBlob.Properties.ContentType = "image/jpeg";
                    break;
                case "gif":
                    blockBlob.Properties.ContentType = "image/gif";
                    break;
                case "svg":
                    blockBlob.Properties.ContentType = "image/svg+xml";
                    break;
                case "ico":
                    blockBlob.Properties.ContentType = "image/x-icon";
                    break;
                case "webp":
                    blockBlob.Properties.ContentType = "image/webp";
                    break;

                // style
                case "css":
                    blockBlob.Properties.ContentType = "text/css";
                    break;

                // script
                case "js":
                    blockBlob.Properties.ContentType = "text/javascript";
                    break;

                // video
                case "mpeg":
                    blockBlob.Properties.ContentType = "audio/mpeg";
                    break;

                case "webm":
                    blockBlob.Properties.ContentType = "video/webm";
                    break;

                // text
                case "txt":
                    blockBlob.Properties.ContentType = "text/plain";
                    break;

                // documents
                case "pdf":
                    blockBlob.Properties.ContentType = "application/pdf";
                    break;
                case "doc":
                case "dot":
                    blockBlob.Properties.ContentType = "application/msword";
                    break;
                case "docx":
                    blockBlob.Properties.ContentType = "application/vnd.openxmlformats-officedocument.wordprocessingml.document";
                    break;
                case "dotx":
                    blockBlob.Properties.ContentType = "application/vnd.openxmlformats-officedocument.wordprocessingml.template";
                    break;
                case "docm":
                    blockBlob.Properties.ContentType = "application/vnd.ms-word.document.macroEnabled.12";
                    break;
                case "dotm":
                    blockBlob.Properties.ContentType = "application/vnd.ms-word.template.macroEnabled.12";
                    break;
                case "xls":
                case "xlt":
                case "xla":
                    blockBlob.Properties.ContentType = "application/vnd.ms-excel";
                    break;
                case "xlsx":
                    blockBlob.Properties.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                    break;
                case "xltx":
                    blockBlob.Properties.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.template";
                    break;
                case "xlsm":
                    blockBlob.Properties.ContentType = "application/vnd.ms-excel.sheet.macroEnabled.12";
                    break;
                case "xltm":
                    blockBlob.Properties.ContentType = "application/vnd.ms-excel.template.macroEnabled.12";
                    break;
                case "xlam":
                    blockBlob.Properties.ContentType = "application/vnd.ms-excel.addin.macroEnabled.12";
                    break;
                case "xlsb":
                    blockBlob.Properties.ContentType = "application/vnd.ms-excel.sheet.binary.macroEnabled.12";
                    break;
                case "ppt":
                case "pot":
                case "pps":
                case "ppa":
                    blockBlob.Properties.ContentType = "application/vnd.ms-powerpoint";
                    break;
                case "pptx":
                    blockBlob.Properties.ContentType = "application/vnd.openxmlformats-officedocument.presentationml.presentation";
                    break;
                case "potx":
                    blockBlob.Properties.ContentType = "application/vnd.openxmlformats-officedocument.presentationml.template";
                    break;
                case "ppsx":
                    blockBlob.Properties.ContentType = "application/vnd.openxmlformats-officedocument.presentationml.slideshow";
                    break;

                // other
                case "ttf":
                    blockBlob.Properties.ContentType = "application/font-sfnt";
                    break;
                case "eot":
                    blockBlob.Properties.ContentType = "application/vnd.ms-fontobject";
                    break;
                case "woff":
                case "woff2":
                    blockBlob.Properties.ContentType = "application/x-font-woff";
                    break;
            }

            blockBlob.Properties.CacheControl = "public, max-age=604800"; // 1 week

            try
            {
                await blockBlob.SetPropertiesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("async", ex.InnerException);
            }
        }

        protected async Task SetCorsAsync(CloudBlobClient blobClient)
        {
            ServiceProperties blobServiceProperties = await blobClient.GetServicePropertiesAsync();

            blobServiceProperties.Cors = new CorsProperties();

            blobServiceProperties.Cors.CorsRules.Add(new CorsRule()
            {
                AllowedHeaders = new List<string>() { "*" },
                AllowedMethods = CorsHttpMethods.Put | CorsHttpMethods.Get | CorsHttpMethods.Head | CorsHttpMethods.Post,
                AllowedOrigins = new List<string>() { "*" },
                ExposedHeaders = new List<string>() { "*" },
                MaxAgeInSeconds = 1800 // 30 minutes
            });

            await blobClient.SetServicePropertiesAsync(blobServiceProperties);
        }

        protected string CleanFileName(string fileName)
        {
            return fileName.Replace(" ", string.Empty);
        }

        protected async Task SetPublicContainerPermissionsAsync(CloudBlobContainer container)
        {
            var permissions = await container.GetPermissionsAsync();
            permissions.PublicAccess = BlobContainerPublicAccessType.Container;
            await container.SetPermissionsAsync(permissions);
        }
    }
}
