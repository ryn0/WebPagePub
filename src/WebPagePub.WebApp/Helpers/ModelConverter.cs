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
        public string BlobPrefix { get; private set; }
        public string CdnPrefix { get; private set; }

        public ModelConverter(ICacheService cacheService)
        {
            this.cacheService = cacheService;

            BlobPrefix = this.cacheService.GetSnippet(SiteConfigSetting.BlobPrefix);
            CdnPrefix = this.cacheService.GetSnippet(SiteConfigSetting.CdnPrefixWithProtocol);
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
                    DefaultPreviousPhotoThumbCdnUrl = UrlBuilder.ConvertBlobToCdnUrl(previousPhotoUrl?.PhotoThumbUrl, BlobPrefix, CdnPrefix),

                    NextName = next?.Title,
                    NextUrlPath = (next != null) ? UrlBuilder.BlogUrlPath(next.SitePageSection.Key, next.Key) : null,
                    DefaultNextPhotoThumbCdnUrl = UrlBuilder.ConvertBlobToCdnUrl(nextPhotoUrl?.PhotoThumbUrl, BlobPrefix, CdnPrefix),
                    Photos = this.AddPhotos(current.Photos),

                    DefaultPhotoThumbUrl = defaultPhotoOriginalUrl?.PhotoThumbUrl,
                    DefaultPhotoThumbCdnUrl = UrlBuilder.ConvertBlobToCdnUrl(defaultPhotoOriginalUrl?.PhotoThumbUrl, BlobPrefix, CdnPrefix),

                    DefaultPhotoOriginalUrl = defaultPhotoOriginalUrl?.PhotoPreviewUrl,
                    DefaultPhotoOriginalCdnUrl = UrlBuilder.ConvertBlobToCdnUrl(defaultPhotoOriginalUrl?.PhotoPreviewUrl, BlobPrefix, CdnPrefix),

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
                    PhotoOriginalCdnUrl = UrlBuilder.ConvertBlobToCdnUrl(photo.PhotoOriginalUrl, BlobPrefix, CdnPrefix),
                    PhotoFullScreenCdnUrl = UrlBuilder.ConvertBlobToCdnUrl(photo.PhotoFullScreenUrl, BlobPrefix, CdnPrefix),

                    PhotoThumbUrl = photo.PhotoThumbUrl,
                    PhotoThumbCdnUrl = UrlBuilder.ConvertBlobToCdnUrl(photo.PhotoThumbUrl, BlobPrefix, CdnPrefix),

                    PhotoPreviewUrl = photo.PhotoPreviewUrl,
                    PhotoPreviewCdnUrl = UrlBuilder.ConvertBlobToCdnUrl(photo.PhotoPreviewUrl, BlobPrefix, CdnPrefix),
                });
            }

            return photoList;
        }
    }
}