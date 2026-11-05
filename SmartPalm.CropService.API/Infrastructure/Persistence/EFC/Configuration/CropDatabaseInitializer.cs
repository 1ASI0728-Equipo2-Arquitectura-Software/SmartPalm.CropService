using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace SmartPalm.CropService.Infrastructure.Persistence;

public static class CropDatabaseInitializer
{
    public static async Task InitializeAsync(IServiceProvider services, CancellationToken cancellationToken = default)
    {
        await using var scope = services.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<CropDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("CropDatabaseInitializer");
        for (var attempt = 1; attempt <= 10; attempt++)
        {
            try
            {
                await context.Database.MigrateAsync(cancellationToken);
                return;
            }
            catch (Exception exception) when (attempt < 10)
            {
                logger.LogWarning(exception, "Database migration attempt {Attempt} failed; retrying.", attempt);
                await Task.Delay(TimeSpan.FromSeconds(2), cancellationToken);
            }
        }
    }
}
