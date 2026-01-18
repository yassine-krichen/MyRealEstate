using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MyRealEstate.Domain.Entities;
using MyRealEstate.Domain.Enums;
using MyRealEstate.Domain.ValueObjects;

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

        // Seed sample properties
        await SeedPropertiesAsync(context, userManager);
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

    private static async Task SeedPropertiesAsync(ApplicationDbContext context, UserManager<User> userManager)
    {
        if (await context.Properties.AnyAsync())
        {
            return; // Properties already seeded
        }

        var adminUser = await userManager.FindByEmailAsync("admin@myrealestate.com");
        if (adminUser == null) return;

        var properties = new[]
        {
            new Property
            {
                Id = Guid.NewGuid(),
                Title = "Luxury Villa in La Marsa",
                Description = "Stunning 4-bedroom villa with sea view, private pool, and modern amenities. Located in the prestigious La Marsa neighborhood with easy access to beaches and shopping centers. The property features marble floors, high ceilings, and a spacious garden perfect for entertaining.",
                Price = new Money(850000, "TND"),
                PropertyType = "Villa",
                Status = PropertyStatus.Published,
                Bedrooms = 4,
                Bathrooms = 3,
                AreaSqM = 320,
                Address = new Address(
                    line1: "15 Avenue Habib Bourguiba",
                    city: "La Marsa",
                    country: "Tunisia",
                    postalCode: "2078",
                    latitude: 36.8774m,
                    longitude: 10.3247m
                ),
                AgentId = adminUser.Id,
                CreatedAt = DateTime.UtcNow.AddDays(-30),
                UpdatedAt = DateTime.UtcNow.AddDays(-5)
            },
            new Property
            {
                Id = Guid.NewGuid(),
                Title = "Modern Apartment in Lac 2",
                Description = "Spacious 3-bedroom apartment in the heart of Tunis business district. Features include contemporary design, central air conditioning, fitted kitchen, and secure underground parking. Walking distance to restaurants and shops.",
                Price = new Money(450000, "TND"),
                PropertyType = "Apartment",
                Status = PropertyStatus.Published,
                Bedrooms = 3,
                Bathrooms = 2,
                AreaSqM = 180,
                Address = new Address(
                    line1: "Résidence Les Pins, Lac 2",
                    city: "Tunis",
                    country: "Tunisia",
                    postalCode: "1053",
                    latitude: 36.8380m,
                    longitude: 10.2344m
                ),
                AgentId = adminUser.Id,
                CreatedAt = DateTime.UtcNow.AddDays(-20),
                UpdatedAt = DateTime.UtcNow.AddDays(-3)
            },
            new Property
            {
                Id = Guid.NewGuid(),
                Title = "Charming House in Sidi Bou Said",
                Description = "Traditional Tunisian house with stunning blue and white architecture, offering panoramic views of the Mediterranean Sea. This 3-bedroom property has been tastefully renovated while preserving authentic details like arched doorways and hand-painted tiles.",
                Price = new Money(620000, "TND"),
                PropertyType = "House",
                Status = PropertyStatus.Published,
                Bedrooms = 3,
                Bathrooms = 2,
                AreaSqM = 220,
                Address = new Address(
                    line1: "Rue Sidi Chabaane",
                    city: "Sidi Bou Said",
                    country: "Tunisia",
                    postalCode: "2026",
                    latitude: 36.8686m,
                    longitude: 10.3419m
                ),
                AgentId = adminUser.Id,
                CreatedAt = DateTime.UtcNow.AddDays(-15),
                UpdatedAt = DateTime.UtcNow.AddDays(-1)
            },
            new Property
            {
                Id = Guid.NewGuid(),
                Title = "Cozy Studio in Carthage",
                Description = "Perfect for young professionals or students. This well-maintained studio offers efficient use of space with a kitchenette, modern bathroom, and plenty of natural light. Close to universities and public transport.",
                Price = new Money(180000, "TND"),
                PropertyType = "Studio",
                Status = PropertyStatus.Published,
                Bedrooms = 0,
                Bathrooms = 1,
                AreaSqM = 45,
                Address = new Address(
                    line1: "Avenue de la République",
                    city: "Carthage",
                    country: "Tunisia",
                    postalCode: "2016",
                    latitude: 36.8531m,
                    longitude: 10.3231m
                ),
                AgentId = adminUser.Id,
                CreatedAt = DateTime.UtcNow.AddDays(-10),
                UpdatedAt = DateTime.UtcNow.AddDays(-2)
            },
            new Property
            {
                Id = Guid.NewGuid(),
                Title = "Elegant Duplex in Gammarth",
                Description = "Luxurious 5-bedroom duplex in exclusive Gammarth area. Features include a private rooftop terrace with sea views, designer kitchen with premium appliances, home office, and state-of-the-art security system. Gated community with 24/7 security.",
                Price = new Money(1200000, "TND"),
                PropertyType = "Duplex",
                Status = PropertyStatus.Draft,
                Bedrooms = 5,
                Bathrooms = 4,
                AreaSqM = 380,
                Address = new Address(
                    line1: "Résidence Gammarth Beach",
                    city: "Gammarth",
                    country: "Tunisia",
                    postalCode: "2070",
                    latitude: 36.9108m,
                    longitude: 10.2908m
                ),
                AgentId = adminUser.Id,
                CreatedAt = DateTime.UtcNow.AddDays(-5)
            }
        };

        await context.Properties.AddRangeAsync(properties);
        await context.SaveChangesAsync();
    }
}
