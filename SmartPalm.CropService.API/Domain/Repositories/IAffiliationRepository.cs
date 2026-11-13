using SmartPalm.CropService.Domain.Plantations;

namespace SmartPalm.CropService.Domain.Repositories;

public interface IAffiliationRepository
{
    Task AddAsync(AgronomistAffiliation affiliation, CancellationToken cancellationToken);
    Task<AgronomistAffiliation?> FindAsync(long agronomistId, long plantationId, CancellationToken cancellationToken);
    Task<IReadOnlyList<AgronomistAffiliation>> ListByAgronomistAsync(long agronomistId, CancellationToken cancellationToken);
    Task<IReadOnlyList<AgronomistAffiliation>> ListByPlantationAsync(long plantationId, CancellationToken cancellationToken);
    Task<IReadOnlyList<AgronomistAffiliation>> ListActiveByPlantationsAsync(
        IReadOnlyCollection<long> plantationIds,
        CancellationToken cancellationToken);
}
