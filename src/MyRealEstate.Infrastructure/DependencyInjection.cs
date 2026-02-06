using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MyRealEstate.Application.Interfaces;
using MyRealEstate.Domain.Entities;
using MyRealEstate.Infrastructure.Data;
using MyRealEstate.Infrastructure.Repositories;
using MyRealEstate.Infrastructure.Services;

namespace MyRealEstate.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        
        // Add DbContext - Use SQLite for easier development
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlite(connectionString,
                b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)));
        
        // Register IApplicationDbContext
        services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<ApplicationDbContext>());

        // Add Identity
        services.AddIdentity<User, IdentityRole<Guid>>(options =>
        {
            // Password settings
            options.Password.RequireDigit = true;
            options.Password.RequireLowercase = true;
            options.Password.RequireUppercase = true;
            options.Password.RequireNonAlphanumeric = true;
            options.Password.RequiredLength = 8;

            // Lockout settings
            options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
            options.Lockout.MaxFailedAccessAttempts = 5;
            options.Lockout.AllowedForNewUsers = true;

            // User settings
            options.User.RequireUniqueEmail = true;
        })
        .AddEntityFrameworkStores<ApplicationDbContext>()
        .AddDefaultTokenProviders();

        // Configure cookie settings
        services.ConfigureApplicationCookie(options =>
        {
            options.LoginPath = "/Account/Login";
            options.LogoutPath = "/Account/Logout";
            options.AccessDeniedPath = "/Account/AccessDenied";
            options.ExpireTimeSpan = TimeSpan.FromHours(24);
            options.SlidingExpiration = true;
        });
        
        // Register Infrastructure services
        services.AddScoped<IFileStorage, LocalFileStorage>();
        services.AddScoped<IEmailSender, FakeEmailSender>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<IPropertyViewRepository, PropertyViewRepository>();
        services.AddHttpContextAccessor();

        return services;
    }
}