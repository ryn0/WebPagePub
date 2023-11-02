using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WebPagePub.Data.Constants;
using WebPagePub.Data.DbContextInfo;
using WebPagePub.Data.Models;
using WebPagePub.Web.Models;

namespace WebPagePub.Web.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext context;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly SignInManager<ApplicationUser> signInManager;
        private readonly RoleManager<IdentityRole> roleManager;

        public AccountController(
          ApplicationDbContext context,
          UserManager<ApplicationUser> userManager,
          SignInManager<ApplicationUser> signInManager,
          RoleManager<IdentityRole> roleManager)
        {
            this.context = context;
            this.userManager = userManager;
            this.signInManager = signInManager;
            this.roleManager = roleManager;
        }

        [Route("account/login")]
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login()
        {
            if (!this.context.Users.Any())
            {
                return this.RedirectToAction(nameof(this.CreateAccount));
            }

            return this.View();
        }

        [Route("account/createaccount")]
        [HttpGet]
        public IActionResult CreateAccount()
        {
            return this.View();
        }

        [Route("account/createaccount")]
        [HttpPost]
        public async Task<IActionResult> CreateAccount(RegistrationModel model)
        {
            if (!this.ModelState.IsValid)
            {
                return this.View();
            }

            if (!this.context.Roles.Any(r => r.Name == StringConstants.AdminRole))
            {
                await this.roleManager.CreateAsync(new IdentityRole(StringConstants.AdminRole));
            }

            var userResult = await this.userManager.CreateAsync(
                new ApplicationUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                    EmailConfirmed = true
                }, model.Password);

            if (!userResult.Succeeded)
            {
                this.ModelState.AddModelError(string.Empty, string.Join(",", userResult.Errors.Select(x => x.Description)));
                return this.View(model);
            }

            var newUser = await this.userManager.FindByNameAsync(model.Email);
            if (newUser == null)
            {
                this.ModelState.AddModelError(string.Empty, "User not found after creation.");
                return this.View(model);
            }

            var addUserResult = await this.userManager.AddToRoleAsync(newUser, StringConstants.AdminRole);

            if (this.IsValidPassword(new LoginViewModel()
            {
                Email = model.Email,
                Password = model.Password,
                RememberMe = false
            }) &&
            this.EmailIsConfirmed(model.Email, out ApplicationUser user))
            {
                return this.RedirectToAction("Index", "Admin");
            }

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
        public IActionResult Login(LoginViewModel model, string? returnUrl = null)
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
                if (!this.EmailIsConfirmed(model.Email, out _))
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

        [Route("account/changepassword")]
        [Authorize]
        [HttpGet]
        public IActionResult ChangePassword()
        {
            return this.View(nameof(this.ChangePassword), new ChangePasswordModel());
        }

        [Route("account/changepassword")]
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> ChangePasswordAsync(ChangePasswordModel model)
        {
            if (!this.ModelState.IsValid)
            {
                return this.View(model);
            }

            var currentUser = await this.userManager.GetUserAsync(this.HttpContext.User);
            if (currentUser == null)
            {
                this.ModelState.AddModelError(string.Empty, "Unable to retrieve current user.");
                return this.View(model);
            }

            var result = await this.userManager.ChangePasswordAsync(currentUser, model.CurrentPassword, model.NewPassword);

            if (!result.Succeeded)
            {
                this.ModelState.AddModelError(string.Empty, string.Join(",", result.Errors.Select(x => x.Description)));
                return this.View(model);
            }

            return this.RedirectToAction(nameof(this.Home));
        }

        public async Task AddUserToRole(ApplicationUser user, string role)
        {
            var admin = await this.roleManager.FindByNameAsync(role);

            if (admin == null)
            {
                admin = new IdentityRole(role);
                await this.roleManager.CreateAsync(admin);
            }

            var roleName = admin.Name ?? role;

            if (!await this.userManager.IsInRoleAsync(user, roleName))
            {
                await this.userManager.AddToRoleAsync(user, roleName);
            }
        }

        private bool EmailIsConfirmed(string email, out ApplicationUser applicationUser)
        {
            var localUser = Task.Run(() => this.userManager.FindByNameAsync(email)).Result;

            if (localUser == null)
            {
                applicationUser = new ApplicationUser(); // Default instance
                return false;
            }

            localUser.EmailConfirmed = true;
            Task.Run(() => this.userManager.UpdateAsync(localUser)).Wait();

            applicationUser = localUser;

            return Task.Run(() => this.userManager.IsEmailConfirmedAsync(localUser)).Result;
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
