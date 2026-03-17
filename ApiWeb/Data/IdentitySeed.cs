using ApiWeb.Constants;
using ApiWeb.Models.Auth;
using Microsoft.AspNetCore.Identity;

namespace ApiWeb.Data;

public static class IdentitySeed
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();

        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        string[] roles = [Roles.Admin, Roles.User];

        foreach (var role in roles)
        {
            var exists = await roleManager.RoleExistsAsync(role);
            if (!exists)
                await roleManager.CreateAsync(new IdentityRole(role));
        }

        var adminEmail = "admin@apiweb.com";
        var adminPassword = "Admin@12345";

        var admin = await userManager.FindByEmailAsync(adminEmail);

        if (admin is null)
        {
            admin = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                FullName = "Administrador API"
            };

            var result = await userManager.CreateAsync(admin, adminPassword);

            if (!result.Succeeded)
            {
                var errors = string.Join(" | ", result.Errors.Select(e => e.Description));
                throw new Exception($"Erro ao criar admin inicial: {errors}");
            }
        }

        var isAdmin = await userManager.IsInRoleAsync(admin, Roles.Admin);
        if (!isAdmin)
            await userManager.AddToRoleAsync(admin, Roles.Admin);
    }
}