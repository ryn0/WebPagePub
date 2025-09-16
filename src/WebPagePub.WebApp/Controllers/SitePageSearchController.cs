using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using WebPagePub.Core;
using WebPagePub.Data.Models;
using WebPagePub.Data.Repositories.Interfaces;
using WebPagePub.WebApp.Models.SitePage;

namespace WebPagePub.Web.Controllers
{
    public class SitePageSearchController : Controller
    {
        private readonly ISitePageRepository sitePageRepository;
        private readonly ISitePageSectionRepository sitePageSectionRepository;
        private readonly ISiteSearchLogRepository siteSearchLogRepository;

        public SitePageSearchController(
            ISitePageRepository sitePageRepository,
            ISitePageSectionRepository sitePageSectionRepository,
            ISiteSearchLogRepository siteSearchLogRepository)
        {
            this.sitePageRepository = sitePageRepository;
            this.sitePageSectionRepository = sitePageSectionRepository;
            this.siteSearchLogRepository = siteSearchLogRepository;
        }

        [HttpGet("search")]
        public async Task<IActionResult> Index(string term = "", int page = 1, int pageSize = 10)
        {
            var result = await this.sitePageRepository.PagedSearchAsync(term, page, pageSize);

            // Best-effort log (don’t block the request if logging fails)
            try
            {
                var log = new SiteSearchLog
                {
                    Term = term ?? string.Empty,
                    PageNumber = page,
                    PageSize = pageSize,
                    ResultsCount = result.TotalCount,
                    CreateDate = DateTime.UtcNow,
                    ClientIp = GetClientIp(this.HttpContext),
                    UserAgent = this.Request?.Headers["User-Agent"].ToString(),
                    Referer = this.Request?.Headers["Referer"].ToString(),
                    Path = this.Request?.Path.ToString()
                };

                await this.siteSearchLogRepository.CreateAsync(log);
            }
            catch
            {
                // swallow logging exceptions
            }

            var model = new SitePageSearchResultsModel
            {
                SearchTerm = term ?? string.Empty,
                CurrentPageNumber = page,
                QuantityPerPage = pageSize,
                Total = result.TotalCount
            };

            foreach (var p in result.Items)
            {
                model.Items.Add(new SitePageItemModel
                {
                    SitePageId = p.SitePageId,
                    Title = p.Title,
                    Key = p.Key,
                    CreateDate = p.CreateDate,
                    PublishDateTimeUtc = p.PublishDateTimeUtc,
                    IsLive = p.IsLive,
                    IsIndex = p.IsSectionHomePage,
                    SitePageSectionId = p.SitePageSectionId,
                    LiveUrlPath = UrlBuilder.BlogUrlPath(p.SitePageSection.Key, p.Key),
                    PreviewUrlPath = UrlBuilder.BlogPreviewUrlPath(p.SitePageId)
                });
            }

            var pageCount = (double)model.Total / model.QuantityPerPage;
            model.PageCount = (int)Math.Ceiling(pageCount);

            return this.View("Results", model);
        }

        [Authorize(Roles = WebPagePub.Data.Constants.StringConstants.AdminRole)]
        [HttpGet("sitepages/advanced-search")]
        public IActionResult AdvancedSearch(
            string? term,
            string? tagsCsv,
            int? sitePageSectionId,
            bool? isLive,
            DateTime? publishedFromUtc,
            DateTime? publishedToUtc,
            int pageNumber = 1,
            int quantityPerPage = 10)
        {
            var model = new SitePageAdvancedSearchModel
            {
                Term = term,
                TagsCsv = tagsCsv,
                SitePageSectionId = sitePageSectionId,
                IsLive = isLive,
                PublishedFromUtc = publishedFromUtc,
                PublishedToUtc = publishedToUtc,
                PageNumber = Math.Max(1, pageNumber),
                QuantityPerPage = Math.Clamp(quantityPerPage, 1, 100)
            };

            // sections dropdown
            var sections = this.sitePageSectionRepository.GetAll()
                .OrderBy(s => s.Title)
                .Select(s => new SelectListItem { Text = s.Title, Value = s.SitePageSectionId.ToString() })
                .ToList();
            sections.Insert(0, new SelectListItem { Text = "(All sections)", Value = string.Empty });
            model.Sections = sections;

            // tags array
            var tags = (tagsCsv ?? string.Empty)
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(t => t.Trim())
                .Where(t => !string.IsNullOrWhiteSpace(t))
                .ToList();

            // execute search (paged)
            var results = this.sitePageRepository.SearchAdvanced(
                term: model.Term,
                tags: tags,
                sitePageSectionId: model.SitePageSectionId,
                isLive: model.IsLive,
                publishedFromUtc: model.PublishedFromUtc,
                publishedToUtc: model.PublishedToUtc,
                pageNumber: model.PageNumber,
                quantityPerPage: model.QuantityPerPage,
                out int total);

            model.Total = total;

            foreach (var p in results)
            {
                model.Items.Add(new SitePageAdvancedSearchModel.ResultItem
                {
                    SitePageId = p.SitePageId,
                    Title = p.Title,
                    Section = p.SitePageSection?.Title ?? string.Empty,
                    Key = p.Key,
                    IsLive = p.IsLive,
                    PublishDateTimeUtc = p.PublishDateTimeUtc,
                    LiveUrlPath = UrlBuilder.BlogUrlPath(p.SitePageSection?.Key ?? string.Empty, p.Key),
                    Tags = p.SitePageTags.Select(t => t.Tag.Name).OrderBy(n => n).ToList()
                });
            }

            return this.View("Advanced", model);
        }

        private static string? GetClientIp(HttpContext ctx)
        {
            // Friendly to proxies/CDNs
            var headers = ctx?.Request?.Headers;
            var cf = headers?["CF-Connecting-IP"].ToString();
            if (!string.IsNullOrWhiteSpace(cf))
            {
                return cf;
            }

            var xff = headers?["X-Forwarded-For"].ToString();
            if (!string.IsNullOrWhiteSpace(xff))
            {
                // first IP in the list is the original client
                var first = xff.Split(',', StringSplitOptions.RemoveEmptyEntries)[0].Trim();
                if (!string.IsNullOrWhiteSpace(first))
                {
                    return first;
                }
            }

            return ctx?.Connection?.RemoteIpAddress?.ToString();
        }
    }
}
