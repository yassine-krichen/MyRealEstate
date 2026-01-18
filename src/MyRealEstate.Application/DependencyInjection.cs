using System.Reflection;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using MyRealEstate.Application.Common.Behaviors;

namespace MyRealEstate.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = Assembly.GetExecutingAssembly();
        
        // Register MediatR
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(assembly);
            cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
            cfg.AddOpenBehavior(typeof(LoggingBehavior<,>));
        });
        
        // Register FluentValidation validators
        services.AddValidatorsFromAssembly(assembly);
        
        // Register AutoMapper
        services.AddAutoMapper(assembly);
        
        return services;
    }
}
