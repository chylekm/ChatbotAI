using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using ChatbotAI.Domain.Entities;

namespace ChatbotAI.Persistence.Seed;

public static class UserSeeder
{
    public static async Task SeedAdminUserAsync(UserManager<ApplicationUser> userManager, IConfiguration config)
    {
        var adminEmail = config["SeedAdminEmail"];
        var adminPassword = config["SeedAdminPassword"];

        if (string.IsNullOrWhiteSpace(adminEmail) || string.IsNullOrWhiteSpace(adminPassword))
            throw new Exception("Missing Admin seed credentials in configuration");

        var existingAdmin = await userManager.FindByEmailAsync(adminEmail);
        if (existingAdmin != null) return;

        var adminUser = new ApplicationUser
        {
            UserName = adminEmail,
            Email = adminEmail,
            EmailConfirmed = true
        };

        var result = await userManager.CreateAsync(adminUser, adminPassword);
        if (!result.Succeeded)
            throw new Exception("Failed to create admin user: " + string.Join(", ", result.Errors.Select(e => e.Description)));

        await userManager.AddToRoleAsync(adminUser, "Admin");
    }
}
