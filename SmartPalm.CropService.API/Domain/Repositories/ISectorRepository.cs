using SmartPalm.CropService.Domain.Plantations;

namespace SmartPalm.CropService.Domain.Repositories;

public interface ISectorRepository
{
    Task<Sector?> GetAsync(long id, CancellationToken cancellationToken);
    Task<Sector?> FindByDeviceMacAsync(string deviceMacAddress, CancellationToken cancellationToken);
    Task<IReadOnlyList<Sector>> ListByPlantationAsync(long plantationId, CancellationToken cancellationToken);
    void Remove(Sector sector);
}
