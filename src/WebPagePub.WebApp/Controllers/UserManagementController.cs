using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WebPagePub.Data.Constants;
using WebPagePub.Data.DbContextInfo.Implementations;
using WebPagePub.Data.Models;

namespace WebPagePub.Web.Controllers
{
    /// <summary>
    /// Lists every account that can sign in to this site and lets an administrator delete one.
    /// Added after 2026-11-16, when accounts created through the old open sign-up page went
    /// unnoticed because no page showed them. Accounts cannot be created here: a new one is
    /// added by the site owner on the server.
    /// </summary>
    [Authorize(Roles = StringConstants.AdminRole)]
    public class UserManagementController : Controller
    {
        private readonly ApplicationDbContext context;
        private readonly UserManager<ApplicationUser> userManager;

        public UserManagementController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            this.context = context;
            this.userManager = userManager;
        }

        [Route("usermanagement")]
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var currentId = this.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var rows = new List<UserRow>();

            foreach (var user in this.context.Users.OrderBy(u => u.UserName).ToList())
            {
                var roles = await this.userManager.GetRolesAsync(user);
                rows.Add(new UserRow(
                    user.Id,
                    user.UserName ?? "(no name)",
                    user.Email ?? string.Empty,
                    string.Join(", ", roles),
                    user.LockoutEnd is { } end && end > DateTimeOffset.UtcNow,
                    user.AccessFailedCount,
                    user.Id == currentId));
            }

            this.ViewData["Message"] = this.TempData["Message"];
            return this.View(rows);
        }

        [Route("usermanagement/delete")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string id)
        {
            var currentId = this.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = string.IsNullOrEmpty(id) ? null : await this.userManager.FindByIdAsync(id);

            if (user == null)
            {
                this.TempData["Message"] = "That account no longer exists.";
            }
            else if (user.Id == currentId)
            {
                this.TempData["Message"] = "You can't delete the account you are signed in with.";
            }
            else
            {
                var result = await this.userManager.DeleteAsync(user);
                this.TempData["Message"] = result.Succeeded
                    ? $"Deleted {user.UserName}."
                    : "Could not delete: " + string.Join(", ", result.Errors.Select(e => e.Description));
            }

            return this.RedirectToAction(nameof(this.Index));
        }

        public sealed record UserRow(string Id, string UserName, string Email, string Roles, bool LockedOut, int FailedSignIns, bool IsYou);
    }
}
