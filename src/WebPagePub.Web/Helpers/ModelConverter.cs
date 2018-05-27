using WebPagePub.Data.Models;
using WebPagePub.Web.Models;
using System.Linq;
using System.Collections.Generic;
using WebPagePub.Services.Interfaces;
using WebPagePub.Data.Enums;

namespace WebPagePub.Web.Helpers
{
    public class ModelConverter
    {
        private readonly ICacheService _cacheService;

        public ModelConverter(ICacheService cacheService)
        {
            _cacheService = cacheService;

        }

        public  SitePageDisplayModel ConvertToBlogDisplayModel(SitePage current, SitePage previous, SitePage next)
        {
            var defaultPhotoUrl = current?.Photos.FirstOrDefault(x => x.IsDefault == true);
            var previousPhotoUrl = previous?.Photos.FirstOrDefault(x => x.IsDefault == true);
            var nextPhotoUrl = next?.Photos.FirstOrDefault(x => x.IsDefault == true);

            var model = new SitePageDisplayModel(_cacheService)
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
                    DefaultPreviousPhotoThumbCdnUrl = ConvertBlobToCdnUrl(previousPhotoUrl?.PhotoThumbUrl),

                    NextName = next?.Title,
                    NextUrlPath = (next != null) ? UrlBuilder.BlogUrlPath(next.SitePageSection.Key, next.Key) : null,
                    DefaultNextPhotoThumbCdnUrl = ConvertBlobToCdnUrl(nextPhotoUrl?.PhotoThumbUrl),

                    Photos = AddPhotos(current.Photos),

                    DefaultPhotoThumbUrl = defaultPhotoUrl?.PhotoThumbUrl,
                    DefaultPhotoThumbCdnUrl = ConvertBlobToCdnUrl(defaultPhotoUrl?.PhotoThumbUrl),

                    DefaultPhotoUrl = defaultPhotoUrl?.PhotoPreviewUrl,
                    DefaultPhotoCdnUrl = ConvertBlobToCdnUrl(defaultPhotoUrl?.PhotoPreviewUrl),

                    MetaDescription = current.MetaDescription
                }
            
            };

            if (current.SitePageTags != null)
            {
                foreach (var SitePageTag in current.SitePageTags)
                {
                    model.PageContent.Tags.Add(SitePageTag.Tag.Name);
                }

                model.PageContent.Tags = model.PageContent.Tags.OrderBy(x => x).ToList();
            }

            return model;
        }
 
        private List<SitePagePhotoModel> AddPhotos(List<SitePagePhoto> photos)
        {
            photos = photos.OrderBy(x => x.Rank).ToList();

            var photoList = new List<SitePagePhotoModel>();

            foreach(var photo in photos)
            {
                photoList.Add(new SitePagePhotoModel
                {
                    SitePagePhotoId = photo.SitePageId,
                    Description = photo.Description,
                    IsDefault = photo.IsDefault,
                    Title = photo.Title,

                    PhotoUrl = photo.PhotoUrl,
                    PhotoCdnUrl = ConvertBlobToCdnUrl(photo?.PhotoUrl),
                    PhotoFullScreenCdnUrl = ConvertBlobToCdnUrl(photo?.PhotoFullScreenUrl),
                    PhotoThumbUrl = photo.PhotoThumbUrl,
                    PhotoThumbCdnUrl = ConvertBlobToCdnUrl(photo?.PhotoThumbUrl),

                    PhotoPreviewUrl = photo.PhotoPreviewUrl,
                    PhotoPreviewCdnUrl = ConvertBlobToCdnUrl(photo?.PhotoPreviewUrl),
                });
            }

            return photoList;
        }


        public string ConvertBlobToCdnUrl(string blobUrl)
        {
            if (string.IsNullOrWhiteSpace(blobUrl))
                return null;

            var blobPrefix = _cacheService.GetSnippet(SiteConfigSetting.BlobPrefix);

            var cdnPrefix = _cacheService.GetSnippet(SiteConfigSetting.CdnPrefixWithProtocol);

            return blobUrl.Replace(blobPrefix, cdnPrefix);
        }
 
    }
}

