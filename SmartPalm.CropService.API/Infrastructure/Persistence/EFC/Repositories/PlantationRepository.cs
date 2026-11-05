using Microsoft.EntityFrameworkCore;
using SmartPalm.CropService.Domain.Repositories;
using SmartPalm.CropService.Domain.Plantations;

namespace SmartPalm.CropService.Infrastructure.Persistence;

internal class PlantationRepository(CropDbContext context) : IPlantationRepository
{
    public async Task AddAsync(Plantation plantation, CancellationToken cancellationToken) =>
        await context.Plantations.AddAsync(plantation, cancellationToken);

    public Task<Plantation?> GetAsync(long id, CancellationToken cancellationToken) =>
        context.Plantations.Include(value => value.Sectors)
            .SingleOrDefaultAsync(value => value.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Plantation>> ListByOwnerAsync(long palmGrowerId, CancellationToken cancellationToken) =>
        await context.Plantations.AsNoTracking()
            .Where(value => value.PalmGrowerId == palmGrowerId)
            .OrderBy(value => value.Name)
            .ToArrayAsync(cancellationToken);

    public async Task<IReadOnlyList<Plantation>> ListAllAsync(CancellationToken cancellationToken) =>
        await context.Plantations.AsNoTracking().OrderBy(value => value.Name).ToArrayAsync(cancellationToken);
}
