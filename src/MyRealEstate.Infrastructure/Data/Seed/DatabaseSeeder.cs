using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MyRealEstate.Domain.Entities;

namespace MyRealEstate.Infrastructure.Data.Seed;

public static class DatabaseSeeder
{
    public const string AdminRole = "Admin";
    public const string AgentRole = "Agent";

    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();

        // Apply migrations
        await context.Database.MigrateAsync();

        // Seed roles
        await SeedRolesAsync(roleManager);

        // Seed admin user
        await SeedAdminUserAsync(userManager);

        // Seed sample content entries
        await SeedContentEntriesAsync(context);
    }

    private static async Task SeedRolesAsync(RoleManager<IdentityRole<Guid>> roleManager)
    {
        string[] roles = { AdminRole, AgentRole };

        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole<Guid>(role));
            }
        }
    }

    private static async Task SeedAdminUserAsync(UserManager<User> userManager)
    {
        const string adminEmail = "admin@myrealestate.com";
        const string adminPassword = "Admin@123456";

        var adminUser = await userManager.FindByEmailAsync(adminEmail);
        if (adminUser == null)
        {
            adminUser = new User
            {
                UserName = adminEmail,
                Email = adminEmail,
                FullName = "System Administrator",
                EmailConfirmed = true,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            var result = await userManager.CreateAsync(adminUser, adminPassword);
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, AdminRole);
            }
        }
    }

    private static async Task SeedContentEntriesAsync(ApplicationDbContext context)
    {
        var contentKeys = new Dictionary<string, string>
        {
            { "HomeHero", "<h1>Welcome to MyRealEstate</h1><p>Your trusted partner in finding the perfect property.</p>" },
            { "AboutHtml", "<h2>About Us</h2><p>We are a leading real estate company dedicated to helping you find your dream home.</p>" },
            { "FooterText", "<p>&copy; 2026 MyRealEstate. All rights reserved.</p>" },
            { "ContactInfo", "<p>Email: contact@myrealestate.com<br/>Phone: +216 XX XXX XXX</p>" }
        };

        foreach (var (key, value) in contentKeys)
        {
            if (!await context.ContentEntries.AnyAsync(c => c.Key == key))
            {
                context.ContentEntries.Add(new ContentEntry
                {
                    Id = Guid.NewGuid(),
                    Key = key,
                    HtmlValue = value,
                    CreatedAt = DateTime.UtcNow
                });
            }
        }

        await context.SaveChangesAsync();
    }
}
