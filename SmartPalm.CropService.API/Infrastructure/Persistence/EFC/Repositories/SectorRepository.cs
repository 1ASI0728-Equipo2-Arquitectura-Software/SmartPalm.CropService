using Microsoft.EntityFrameworkCore;
using SmartPalm.CropService.Domain.Repositories;
using SmartPalm.CropService.Domain.Plantations;

namespace SmartPalm.CropService.Infrastructure.Persistence;

internal class SectorRepository(CropDbContext context) : ISectorRepository
{
    public Task<Sector?> GetAsync(long id, CancellationToken cancellationToken) =>
        context.Sectors.SingleOrDefaultAsync(value => value.Id == id, cancellationToken);

    public Task<Sector?> FindByDeviceMacAsync(string deviceMacAddress, CancellationToken cancellationToken) =>
        context.Sectors.SingleOrDefaultAsync(
            value => value.IotDeviceMacAddress == Sector.NormalizeMac(deviceMacAddress), cancellationToken);

    public async Task<IReadOnlyList<Sector>> ListByPlantationAsync(long plantationId, CancellationToken cancellationToken) =>
        await context.Sectors.AsNoTracking()
            .Where(value => value.PlantationId == plantationId)
            .OrderBy(value => value.Name)
            .ToArrayAsync(cancellationToken);

    public async Task<IReadOnlyList<Sector>> ListByPlantationsAsync(
        IReadOnlyCollection<long> plantationIds,
        long? sectorId,
        CancellationToken cancellationToken) =>
        await context.Sectors.AsNoTracking()
            .Where(value => plantationIds.Contains(value.PlantationId))
            .Where(value => sectorId == null || value.Id == sectorId.Value)
            .OrderBy(value => value.Id)
            .ToArrayAsync(cancellationToken);

    public void Remove(Sector sector) => context.Sectors.Remove(sector);
}
