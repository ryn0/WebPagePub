using WebPagePub.Core;
using WebPagePub.Data.Enums;
using WebPagePub.Data.Models;
using WebPagePub.Services.Interfaces;
using WebPagePub.WebApp.Models.SitePage;

namespace WebPagePub.Web.Helpers
{
    public class ModelConverter
    {
        private readonly ICacheService cacheService;

        public ModelConverter(ICacheService cacheService)
        {
            this.cacheService = cacheService;

            this.BlobPrefix = this.cacheService.GetSnippet(SiteConfigSetting.BlobPrefix);
            this.CdnPrefix = this.cacheService.GetSnippet(SiteConfigSetting.CdnPrefixWithProtocol);
        }

        public string BlobPrefix { get; private set; }
        public string CdnPrefix { get; private set; }

        private List<SitePagePhotoModel> AddPhotos(List<SitePagePhoto> photos)
        {
            photos = photos.OrderBy(x => x.Rank).ToList();

            var photoList = new List<SitePagePhotoModel>();

            foreach (var photo in photos)
            {
                photoList.Add(new SitePagePhotoModel
                {
                    SitePagePhotoId = photo.SitePageId,
                    Description = photo.Description,
                    IsDefault = photo.IsDefault,
                    Title = photo.Title,
                    PhotoOriginalUrl = photo.PhotoOriginalUrl,
                    PhotoOriginalCdnUrl = UrlBuilder.ConvertBlobToCdnUrl(photo.PhotoOriginalUrl, this.BlobPrefix, this.CdnPrefix),
                    PhotoFullScreenCdnUrl = UrlBuilder.ConvertBlobToCdnUrl(photo.PhotoFullScreenUrl, this.BlobPrefix, this.CdnPrefix),

                    PhotoThumbUrl = photo.PhotoThumbUrl,
                    PhotoThumbCdnUrl = UrlBuilder.ConvertBlobToCdnUrl(photo.PhotoThumbUrl, this.BlobPrefix, this.CdnPrefix),

                    PhotoPreviewUrl = photo.PhotoPreviewUrl,
                    PhotoPreviewCdnUrl = UrlBuilder.ConvertBlobToCdnUrl(photo.PhotoPreviewUrl, this.BlobPrefix, this.CdnPrefix),
                });
            }

            return photoList;
        }
    }
}