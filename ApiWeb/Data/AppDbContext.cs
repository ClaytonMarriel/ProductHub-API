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
        public DbSet<RefreshToken> RefreshTokens { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<ProductModel>(entity =>
            {
                entity.HasKey(p => p.Id);

                entity.Property(p => p.Name)
                    .HasMaxLength(100)
                    .IsRequired();

                entity.Property(p => p.Description)
                    .HasMaxLength(300)
                    .IsRequired();

                entity.Property(p => p.BarCode)
                    .HasMaxLength(50)
                    .IsRequired();

                entity.Property(p => p.Mark)
                    .HasMaxLength(80)
                    .IsRequired();

                entity.HasIndex(p => p.BarCode)
                    .IsUnique();

                entity.HasQueryFilter(p => !p.IsDeleted);
            });

            builder.Entity<RefreshToken>(entity =>
            {
                entity.HasKey(r => r.Id);

                entity.Property(r => r.Token)
                    .HasMaxLength(200)
                    .IsRequired();

                entity.HasIndex(r => r.Token)
                    .IsUnique();

                entity.HasOne(r => r.User)
                    .WithMany()
                    .HasForeignKey(r => r.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}