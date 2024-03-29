﻿using Microsoft.AspNetCore.Identity;
using WebPagePub.Data.DbContextInfo.Interfaces;
using WebPagePub.Data.Models;

namespace WebPagePub.Data.DbContextInfo.Implementations
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
        }
    }
}