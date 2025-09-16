// WebPagePub.Web/Controllers/Admin/SearchLogAdminController.cs
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebPagePub.Data.Constants;
using WebPagePub.Data.Repositories.Interfaces;
using WebPagePub.WebApp.Models.Admin.SearchLogs;

namespace WebPagePub.Web.Controllers.Admin
{
    [Authorize(Roles = StringConstants.AdminRole)]
    public class SearchLogAdminController : Controller
    {
        private readonly ISiteSearchLogRepository searchLogRepository;

        public SearchLogAdminController(ISiteSearchLogRepository searchLogRepository)
        {
            this.searchLogRepository = searchLogRepository;
        }

        // GET /admin/search-logs
        [HttpGet("admin/search-logs")]
        public async Task<IActionResult> Index(DateTime? fromUtc, DateTime? toUtc, string? term, int page = 1, int pageSize = 50)
        {
            // Default range: last 30 days
            var now = DateTime.UtcNow;
            var from = fromUtc ?? now.AddDays(-30);
            var to = toUtc ?? now;

            // normalize bad input
            if (from > to)
            {
                (from, to) = (to, from);
            }

            var result = await this.searchLogRepository.GetPagedAsync(from, to, page, pageSize, term);

            var model = new SiteSearchLogListModel
            {
                FromUtc = from,
                ToUtc = to,
                Term = term,
                PageNumber = page,
                PageSize = pageSize,
                TotalCount = result.TotalCount,
                Items = (List<Data.Models.SiteSearchLog>)result.Items
            };

            return this.View("Index", model);
        }
    }
}
