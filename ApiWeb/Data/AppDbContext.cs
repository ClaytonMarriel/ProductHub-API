using ApiWeb.Models;
using ApiWeb.Models.Auth;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ApiWeb.Data
{
    public class AppDbContent : IdentityDbContext<ApplicationUser>
    {
        public AppDbContent(DbContextOptions<AppDbContent> options) : base(options) { }

        public DbSet<ProductModel> Products { get; set; }
    }
}