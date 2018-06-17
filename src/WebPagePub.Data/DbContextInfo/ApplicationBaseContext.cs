using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using WebPagePub.Data.Models;

namespace WebPagePub.Data.DbContextInfo
{
    public class ApplicationBaseContext<TContext> : IdentityDbContext<ApplicationUser>
        where TContext : DbContext
    {
        public ApplicationBaseContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
    }
}
