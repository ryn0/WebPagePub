using Microsoft.AspNetCore.Identity;
using WebPagePub.Data.DbContextInfo;
using WebPagePub.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebPagePub.Data.DbContextInfo
{
    public class DbInitializer : IDbInitializer
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public DbInitializer(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public void Initialize()
        {
            _context.Database.EnsureCreated();

            var roleName = "Administrator";
            if (!_context.Roles.Any(r => r.Name == roleName))
            {
                Task.Run(() => _roleManager.CreateAsync(new IdentityRole(roleName))).Wait();
            }

            string user = "admin@bootbaron.com";
            string password = "qUcI_8757osmt";

            if (!_context.Users.Any(r => r.UserName == user))
            {
                var userResult = Task.Run(() => _userManager.CreateAsync(new ApplicationUser { UserName = user, Email = user, EmailConfirmed = true }, password)).Result;
                var addUserResult = Task.Run(() => _userManager.AddToRoleAsync(Task.Run(() => _userManager.FindByNameAsync(user)).Result, roleName)).Result;
            }
        }
    }
}
