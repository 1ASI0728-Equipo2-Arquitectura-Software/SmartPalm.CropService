using SmartPalm.CropService.Domain.Plantations;

namespace SmartPalm.CropService.Domain.Repositories;

public interface IPlantationRepository
{
    Task AddAsync(Plantation plantation, CancellationToken cancellationToken);
    Task<Plantation?> GetAsync(long id, CancellationToken cancellationToken);
    Task<IReadOnlyList<Plantation>> ListByOwnerAsync(long palmGrowerId, CancellationToken cancellationToken);
    Task<IReadOnlyList<Plantation>> ListAllAsync(CancellationToken cancellationToken);
    Task<IReadOnlyList<Plantation>> ListForSnapshotAsync(
        long? palmGrowerId,
        long? plantationId,
        long? afterPlantationId,
        int limit,
        CancellationToken cancellationToken);
}
