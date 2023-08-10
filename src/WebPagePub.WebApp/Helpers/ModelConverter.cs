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
        }

        public SitePageDisplayModel ConvertToBlogDisplayModel(SitePage current, SitePage previous, SitePage next)
        {
            var defaultPhotoOriginalUrl = current?.Photos.FirstOrDefault(x => x.IsDefault == true);
            var previousPhotoUrl = previous?.Photos.FirstOrDefault(x => x.IsDefault == true);
            var nextPhotoUrl = next?.Photos.FirstOrDefault(x => x.IsDefault == true);

            var model = new SitePageDisplayModel(this.cacheService)
            {
                PageContent = new SitePageContentModel()
                {
                    LastUpdatedDateTimeUtc = current.PublishDateTimeUtc,
                    Content = current.Content,
                    Key = current.Key,
                    Title = current.Title,
                    BreadcrumbName = current.BreadcrumbName,
                    UrlPath = UrlBuilder.BlogUrlPath(current.SitePageSection.Key, current.Key),

                    PreviousName = previous?.Title,
                    PreviousUrlPath = (previous != null) ? UrlBuilder.BlogUrlPath(previous.SitePageSection.Key, previous.Key) : null,
                    DefaultPreviousPhotoThumbCdnUrl = this.ConvertBlobToCdnUrl(previousPhotoUrl?.PhotoThumbUrl),

                    NextName = next?.Title,
                    NextUrlPath = (next != null) ? UrlBuilder.BlogUrlPath(next.SitePageSection.Key, next.Key) : null,
                    DefaultNextPhotoThumbCdnUrl = this.ConvertBlobToCdnUrl(nextPhotoUrl?.PhotoThumbUrl),

                    Photos = this.AddPhotos(current.Photos),

                    DefaultPhotoThumbUrl = defaultPhotoOriginalUrl?.PhotoThumbUrl,
                    DefaultPhotoThumbCdnUrl = this.ConvertBlobToCdnUrl(defaultPhotoOriginalUrl?.PhotoThumbUrl),

                    DefaultPhotoOriginalUrl = defaultPhotoOriginalUrl?.PhotoPreviewUrl,
                    DefaultPhotoOriginalCdnUrl = this.ConvertBlobToCdnUrl(defaultPhotoOriginalUrl?.PhotoPreviewUrl),

                    MetaDescription = current.MetaDescription
                }
            };

            if (current.SitePageTags != null)
            {
                foreach (var sitePageTag in current.SitePageTags)
                {
                    model.PageContent.Tags.Add(sitePageTag.Tag.Name);
                }

                model.PageContent.Tags = model.PageContent.Tags.OrderBy(x => x).ToList();
            }

            return model;
        }

        public string ConvertBlobToCdnUrl(string blobUrl)
        {
            if (string.IsNullOrWhiteSpace(blobUrl))
            {
                return null;
            }

            var blobPrefix = this.cacheService.GetSnippet(SiteConfigSetting.BlobPrefix);

            var cdnPrefix = this.cacheService.GetSnippet(SiteConfigSetting.CdnPrefixWithProtocol);

            if (string.IsNullOrWhiteSpace(cdnPrefix))
            {
                return null;
            }

            return blobUrl.Replace(blobPrefix, cdnPrefix);
        }

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
                    PhotoOriginalCdnUrl = this.ConvertBlobToCdnUrl(photo.PhotoOriginalUrl),
                    PhotoFullScreenCdnUrl = this.ConvertBlobToCdnUrl(photo.PhotoFullScreenUrl),

                    PhotoThumbUrl = photo.PhotoThumbUrl,
                    PhotoThumbCdnUrl = this.ConvertBlobToCdnUrl(photo.PhotoThumbUrl),

                    PhotoPreviewUrl = photo.PhotoPreviewUrl,
                    PhotoPreviewCdnUrl = this.ConvertBlobToCdnUrl(photo.PhotoPreviewUrl),
                });
            }

            return photoList;
        }
    }
}