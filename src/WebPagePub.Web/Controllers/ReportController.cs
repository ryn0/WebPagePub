using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using WebPagePub.Data.Repositories.Interfaces;
using WebPagePub.Web.Models;
using System.Linq;
using System;
using MoreLinq;

namespace WebPagePub.Web.Controllers
{
    [Authorize]
    public class ReportsController : Controller
    {
        private readonly IClickLogRepository _clickLogRepository;

        public ReportsController(IClickLogRepository clickLogRepository)
        {
            _clickLogRepository = clickLogRepository;
        }

        [Route("reports")]
        public IActionResult Index()
        {
            return View();
        }

        [Route("reports/clicks")]
        public IActionResult ClickReport(DateTime? startDate, DateTime? endDate)
        {
            var now = DateTime.UtcNow;

            if (startDate == null)
                startDate = now.AddDays(-30);

            if (endDate == null)
                endDate = now;

            var model = new ClickReportModel();
            model.StartDate = Convert.ToDateTime(startDate);
            model.EndDate = new DateTime(endDate.Value.Year, endDate.Value.Month, endDate.Value.Day, 23, 59, 59);
          
            var clicksInRange = _clickLogRepository.GetClicksInRange(model.StartDate, model.EndDate);

            model.TotalClicks = clicksInRange.Count();
            model.UniqueIps = clicksInRange.DistinctBy(x => x.IpAddress).Count();
             
            foreach (var item in clicksInRange)
            {  
                var urlClickItem = model.UrlClicks.FirstOrDefault(x => x.Url == item.Url);

                if (urlClickItem == null)
                {
                    model.UrlClicks.Add(new UrlClickReportModel()
                    {
                        Url = item.Url,
                        TotalClicks = 1,
                        IpsForClick = new System.Collections.Generic.List<string>()
                        {
                            item.IpAddress
                        }
                    });
                }
                else
                {
                    urlClickItem.TotalClicks = urlClickItem.TotalClicks + 1;
                    urlClickItem.IpsForClick.Add(item.IpAddress);
                }
            }

            model.UrlClicks = model.UrlClicks.OrderByDescending(x => x.TotalClicks)
                                             .ThenByDescending(x => x.UniqueIps)
                                             .ToList();

            return View(model);
        }
    }
}
