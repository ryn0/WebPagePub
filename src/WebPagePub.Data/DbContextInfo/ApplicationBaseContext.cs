using WebPagePub.Data.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace WebPagePub.Data.DbContextInfo
{
    public class ApplicationBaseContext<TContext> : IdentityDbContext<ApplicationUser> where TContext : DbContext
    {
        public ApplicationBaseContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {

        }
    }
}
