using SmartPalm.CropService.Application.Contracts;
using SmartPalm.CropService.Domain.Plantations;
using SmartPalm.CropService.Domain.Repositories;

namespace SmartPalm.CropService.Application.Services;

public class CropQueryService(
    IPlantationRepository plantations,
    ISectorRepository sectors,
    IAffiliationRepository affiliations)
{
    public async Task<IReadOnlyList<PlantationResponse>> ListMineAsync(
        long palmGrowerId,
        CancellationToken cancellationToken) =>
        (await plantations.ListByOwnerAsync(palmGrowerId, cancellationToken)).Select(Map).ToArray();

    public async Task<IReadOnlyList<PlantationResponse>> ListAllAsync(CancellationToken cancellationToken) =>
        (await plantations.ListAllAsync(cancellationToken)).Select(Map).ToArray();

    public async Task<PlantationDetailResponse?> GetAsync(long id, CancellationToken cancellationToken)
    {
        var plantation = await plantations.GetAsync(id, cancellationToken);
        return plantation is null ? null : MapDetail(plantation);
    }

    public async Task<IReadOnlyList<SectorResponse>> ListSectorsAsync(
        long plantationId,
        CancellationToken cancellationToken) =>
        (await sectors.ListByPlantationAsync(plantationId, cancellationToken)).Select(Map).ToArray();

    public async Task<IReadOnlyList<AgronomistAffiliationResponse>> ListByAgronomistAsync(
        long agronomistId,
        CancellationToken cancellationToken) =>
        (await affiliations.ListByAgronomistAsync(agronomistId, cancellationToken)).Select(Map).ToArray();

    public async Task<IReadOnlyList<AgronomistAffiliationResponse>> ListByPlantationAsync(
        long plantationId,
        CancellationToken cancellationToken) =>
        (await affiliations.ListByPlantationAsync(plantationId, cancellationToken)).Select(Map).ToArray();

    public async Task<bool> CanAccessAsync(
        long plantationId,
        long userId,
        string role,
        CancellationToken cancellationToken)
    {
        if (string.Equals(role, "Administrator", StringComparison.OrdinalIgnoreCase)) return true;
        var plantation = await plantations.GetAsync(plantationId, cancellationToken);
        if (plantation is null) return false;
        if (string.Equals(role, "PalmGrower", StringComparison.OrdinalIgnoreCase))
            return plantation.PalmGrowerId == userId;
        if (!string.Equals(role, "Agronomist", StringComparison.OrdinalIgnoreCase)) return false;
        var affiliation = await affiliations.FindAsync(userId, plantationId, cancellationToken);
        return affiliation?.IsActive == true;
    }

    private static PlantationResponse Map(Plantation value) => new(
        value.Id, value.PalmGrowerId, value.Name, value.Address, value.Coordinates, value.Hectares,
        value.Status.ToString(), value.EstimatedSensors, value.InstallationMessage,
        value.CreatedAt, value.UpdatedAt);

    private static PlantationDetailResponse MapDetail(Plantation value) => new(
        value.Id, value.PalmGrowerId, value.Name, value.Address, value.Coordinates, value.Hectares,
        value.Status.ToString(), value.EstimatedSensors, value.InstallationMessage,
        value.CreatedAt, value.UpdatedAt, value.Sectors.Select(Map).ToArray());

    private static SectorResponse Map(Sector value) => new(
        value.Id, value.Name, value.IotDeviceMacAddress, value.Status.ToString(),
        value.CreatedAt, value.ActivatedAt);

    private static AgronomistAffiliationResponse Map(AgronomistAffiliation value) => new(
        value.Id, value.AgronomistId, value.PlantationId, value.AffiliatedAt, value.DetachedAt);
}
