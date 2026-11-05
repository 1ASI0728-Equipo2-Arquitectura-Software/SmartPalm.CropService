using Microsoft.EntityFrameworkCore;
using SmartPalm.CropService.Domain.Repositories;
using SmartPalm.CropService.Domain.Plantations;

namespace SmartPalm.CropService.Infrastructure.Persistence;

internal class AffiliationRepository(CropDbContext context) : IAffiliationRepository
{
    public async Task AddAsync(AgronomistAffiliation affiliation, CancellationToken cancellationToken) =>
        await context.Affiliations.AddAsync(affiliation, cancellationToken);

    public Task<AgronomistAffiliation?> FindAsync(
        long agronomistId,
        long plantationId,
        CancellationToken cancellationToken) =>
        context.Affiliations.SingleOrDefaultAsync(
            value => value.AgronomistId == agronomistId && value.PlantationId == plantationId,
            cancellationToken);

    public async Task<IReadOnlyList<AgronomistAffiliation>> ListByAgronomistAsync(
        long agronomistId,
        CancellationToken cancellationToken) =>
        await context.Affiliations.AsNoTracking()
            .Where(value => value.AgronomistId == agronomistId && value.DetachedAt == null)
            .OrderByDescending(value => value.AffiliatedAt)
            .ToArrayAsync(cancellationToken);

    public async Task<IReadOnlyList<AgronomistAffiliation>> ListByPlantationAsync(
        long plantationId,
        CancellationToken cancellationToken) =>
        await context.Affiliations.AsNoTracking()
            .Where(value => value.PlantationId == plantationId && value.DetachedAt == null)
            .OrderByDescending(value => value.AffiliatedAt)
            .ToArrayAsync(cancellationToken);
}
