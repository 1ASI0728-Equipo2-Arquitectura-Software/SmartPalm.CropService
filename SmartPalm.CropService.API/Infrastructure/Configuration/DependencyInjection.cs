using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SmartPalm.CropService.Application.Abstractions;
using SmartPalm.CropService.Application.Services;
using SmartPalm.CropService.Domain.Repositories;
using SmartPalm.CropService.Infrastructure.Configuration;
using SmartPalm.CropService.Infrastructure.Messaging;
using SmartPalm.CropService.Infrastructure.Persistence;

namespace SmartPalm.CropService.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddCropInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("CropDatabase")
            ?? throw new InvalidOperationException("ConnectionStrings:CropDatabase is required.");
        services.AddDbContext<CropDbContext>(options => options.UseNpgsql(connectionString).UseSnakeCaseNamingConvention());
        services.Configure<RabbitMqOptions>(configuration.GetSection(RabbitMqOptions.SectionName));
        services.AddScoped<IPlantationRepository, PlantationRepository>();
        services.AddScoped<ISectorRepository, SectorRepository>();
        services.AddScoped<IAffiliationRepository, AffiliationRepository>();
        services.AddScoped<IUnitOfWork>(provider => provider.GetRequiredService<CropDbContext>());
        services.AddScoped<IIntegrationEventWriter, IntegrationEventWriter>();
        services.AddScoped<CropCommandService>();
        services.AddScoped<CropQueryService>();
        services.AddHostedService<OutboxPublisher>();
        return services;
    }
}
