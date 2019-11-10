using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Inergy.Api.Web.Models
{
    /// <summary>
    /// Usuario Identity
    /// </summary>
    public class ApplicationUser : IdentityUser
    {

    }

    /// <summary>
    /// Contexto de tablas de Identity.
    /// El Framework ORM que se utiliza es Entity Framework Microsoft
    /// </summary>
    public class AppIdentityDbContext : IdentityDbContext<ApplicationUser>
    {
        public AppIdentityDbContext(DbContextOptions<AppIdentityDbContext> options) : base(options) { }
    }
}