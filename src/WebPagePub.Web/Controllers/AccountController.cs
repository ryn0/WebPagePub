using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WebPagePub.Data.Models;
using WebPagePub.Web.Models;

namespace WebPagePub.Web.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly SignInManager<ApplicationUser> signInManager;
        private readonly RoleManager<IdentityRole> roleManager;

        public AccountController(
          UserManager<ApplicationUser> userManager,
          SignInManager<ApplicationUser> signInManager,
          RoleManager<IdentityRole> roleManager)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
            this.roleManager = roleManager;
        }

        [Route("account/login")]
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login()
        {
            return this.View();
        }

        [Route("account/home")]
        [Authorize]
        [HttpGet]
        public IActionResult Home()
        {
            return this.View();
        }

        [Route("account/login")]
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public IActionResult Login(LoginViewModel model, string returnUrl = null)
        {
            this.ViewData["ReturnUrl"] = returnUrl;

            if (this.ModelState.IsValid)
            {
                if (!this.IsValidPassword(model))
                {
                    this.ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                    return this.View(model);
                }

                // This doesn't count login failures towards account lockout
                // To enable password failures to trigger account lockout, set lockoutOnFailure: true

                // Require the user to have a confirmed email before they can log on.
                if (!this.EmailIsConfirmed(model.Email, out ApplicationUser user))
                {
                    this.ModelState.AddModelError(string.Empty, "You must have a confirmed email to log in.");
                    return this.View(model);
                }

                return this.RedirectToAction("Index", "Admin");
            }

            // If we got this far, something failed, redisplay form
            return this.View(model);
        }

        [Route("account/logoff")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LogOff()
        {
            await this.signInManager.SignOutAsync();

            return this.RedirectToAction(nameof(HomeController.Index), "Home");
        }

        public async Task AddUserToRole(ApplicationUser user, string role)
        {
            var admin = await this.roleManager.FindByNameAsync(role);

            if (admin == null)
            {
                admin = new IdentityRole(role.ToString());
                await this.roleManager.CreateAsync(admin);
            }

            if (!await this.userManager.IsInRoleAsync(user, admin.Name))
            {
                await this.userManager.AddToRoleAsync(user, admin.Name);
            }
        }

        private bool EmailIsConfirmed(string email, out ApplicationUser applicationUser)
        {
            var user = Task.Run(() => this.userManager.FindByNameAsync(email)).Result;

            applicationUser = user;

            applicationUser.EmailConfirmed = true;
            this.userManager.UpdateAsync(applicationUser);

            if (user == null)
            {
                return false;
            }

            if (Task.Run(() => this.userManager.IsEmailConfirmedAsync(user)).Result)
            {
                return true;
            }

            return false;
        }

        private bool IsValidPassword(LoginViewModel model)
        {
            var result = Task.Run(() => this.signInManager.PasswordSignInAsync(
                    model.Email,
                    model.Password,
                    model.RememberMe,
                    lockoutOnFailure: false)).Result;

            return result.Succeeded && !result.IsLockedOut;
        }
    }
}
