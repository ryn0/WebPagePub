using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using WebPagePub.Data.Models;

namespace WebPagePub.Data.DbContextInfo
{
    public class DbInitializer : IDbInitializer
    {
        private readonly ApplicationDbContext context;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly RoleManager<IdentityRole> roleManager;

        public DbInitializer(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            this.context = context;
            this.userManager = userManager;
            this.roleManager = roleManager;
        }

        public void Initialize()
        {
            this.context.Database.EnsureCreated();

            var roleName = "Administrator";
            if (!this.context.Roles.Any(r => r.Name == roleName))
            {
                Task.Run(() => this.roleManager.CreateAsync(new IdentityRole(roleName))).Wait();
            }

            string user = "admin@bootbaron.com";
            string password = "qUcI_8757osmt";

            if (!this.context.Users.Any(r => r.UserName == user))
            {
                var userResult = Task.Run(() => this.userManager.CreateAsync(
                    new ApplicationUser { UserName = user, Email = user, EmailConfirmed = true }, password)).Result;
                var addUserResult = Task.Run(() => this.userManager.AddToRoleAsync(
                    Task.Run(() => this.userManager.FindByNameAsync(user)).Result, roleName)).Result;
            }
        }
    }
}
