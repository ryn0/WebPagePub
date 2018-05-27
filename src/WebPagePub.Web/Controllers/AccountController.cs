using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using WebPagePub.Web.Models;
using WebPagePub.Data.Models;
using Microsoft.AspNetCore.Identity;

namespace WebPagePub.Web.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AccountController(
          UserManager<ApplicationUser> userManager,
          SignInManager<ApplicationUser> signInManager,
          RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
        }

        [Route("account/login")]
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login()
        {
            return View();
        }

        [Route("account/home")]
        [Authorize]
        [HttpGet]
        public IActionResult Home()
        {
            return View();
        }

        [Route("account/login")]
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public IActionResult Login(LoginViewModel model, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (ModelState.IsValid)
            {
                if (!IsValidPassword(model))
                {
                    ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                    return View(model);

                }
                // This doesn't count login failures towards account lockout
                // To enable password failures to trigger account lockout, set lockoutOnFailure: true

                // Require the user to have a confirmed email before they can log on.
                ApplicationUser user;
                if (!EmailIsConfirmed(model.Email, out user))
                {
                    ModelState.AddModelError(string.Empty, "You must have a confirmed email to log in.");
                    return View(model);
                }

                return RedirectToAction("Index", "Admin");
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        [Route("account/logoff")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LogOff()
        {
            await _signInManager.SignOutAsync();
           
            return RedirectToAction(nameof(HomeController.Index), "Home");
        }

       
        public async Task AddUserToRole(ApplicationUser user, string role)
        {
            var admin = await _roleManager.FindByNameAsync(role);

            if (admin == null)
            {
                admin = new IdentityRole(role.ToString());
                await _roleManager.CreateAsync(admin);
            }

            if (!await _userManager.IsInRoleAsync(user, admin.Name))
            {
                await _userManager.AddToRoleAsync(user, admin.Name);
            }
        }

        private bool EmailIsConfirmed(string email, out ApplicationUser applicationUser)
        {
            var user = Task.Run(() => _userManager.FindByNameAsync(email)).Result;

            applicationUser = user;

            applicationUser.EmailConfirmed = true;
            _userManager.UpdateAsync(applicationUser);

            if (user == null)
                return false;

            if (Task.Run(() => _userManager.IsEmailConfirmedAsync(user)).Result)
                return true;

            return false;
        }

        private bool IsValidPassword(LoginViewModel model)
        {
            var result = Task.Run(() => _signInManager.PasswordSignInAsync(
                    model.Email,
                    model.Password,
                    model.RememberMe,
                    lockoutOnFailure: false)).Result;

            return result.Succeeded && !result.IsLockedOut;

        }
 
    }
}
