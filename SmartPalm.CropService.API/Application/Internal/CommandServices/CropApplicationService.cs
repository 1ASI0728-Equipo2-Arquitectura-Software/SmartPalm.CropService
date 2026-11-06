using SmartPalm.CropService.Application.Abstractions;
using SmartPalm.CropService.Application.Contracts;
using SmartPalm.CropService.Domain.Model.Commands;
using SmartPalm.CropService.Domain.Events;
using SmartPalm.CropService.Domain.Repositories;
using SmartPalm.CropService.Domain.Plantations;

namespace SmartPalm.CropService.Application.Services;

public class CropCommandService(
    IPlantationRepository plantations,
    ISectorRepository sectors,
    IAffiliationRepository affiliations,
    IUnitOfWork unitOfWork,
    IIntegrationEventWriter events)
{
    public async Task<PlantationResponse> CreateAsync(
        CreatePlantationCommand command,
        CancellationToken cancellationToken)
    {
        return await unitOfWork.ExecuteAsync(async transactionToken =>
        {
            var plantation = new Plantation(
                command.PalmGrowerId, command.Name, command.Address, command.Coordinates, command.Hectares);
            await plantations.AddAsync(plantation, transactionToken);
            await unitOfWork.SaveChangesAsync(transactionToken);
            events.Enqueue("crop.plantation-created.v1", new PlantationCreatedIntegrationEvent(
                plantation.Id, plantation.PalmGrowerId, plantation.Name, plantation.Hectares, DateTime.UtcNow));
            await unitOfWork.SaveChangesAsync(transactionToken);
            return Map(plantation);
        }, cancellationToken);
    }

    public async Task<PlantationResponse> UpdateAsync(
        UpdatePlantationCommand command,
        CancellationToken cancellationToken)
    {
        var plantation = await GetRequiredPlantationAsync(command.PlantationId, cancellationToken);
        plantation.Update(command.Name, command.Address, command.Coordinates, command.Hectares);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Map(plantation);
    }

    public async Task<SectorResponse> AssignSectorAsync(
        AssignSectorCommand command,
        CancellationToken cancellationToken)
    {
        var normalizedMac = Sector.NormalizeMac(command.IotDeviceMacAddress);
        if (normalizedMac is not null && await sectors.FindByDeviceMacAsync(normalizedMac, cancellationToken) is not null)
            throw new InvalidOperationException($"IoT device '{normalizedMac}' is already assigned to a sector.");

        return await unitOfWork.ExecuteAsync(async transactionToken =>
        {
            var plantation = await GetRequiredPlantationAsync(command.PlantationId, transactionToken);
            var sector = plantation.AssignSector(command.SectorName, normalizedMac);
            await unitOfWork.SaveChangesAsync(transactionToken);
            events.Enqueue("crop.sector-assigned.v1", new SectorAssignedIntegrationEvent(
                sector.Id, command.PlantationId, normalizedMac, sector.Name, DateTime.UtcNow));
            await unitOfWork.SaveChangesAsync(transactionToken);
            return Map(sector);
        }, cancellationToken);
    }

    public async Task RemoveSectorAsync(
        RemoveSectorCommand command,
        CancellationToken cancellationToken)
    {
        var sector = await sectors.GetAsync(command.SectorId, cancellationToken)
            ?? throw new KeyNotFoundException($"Sector '{command.SectorId}' not found.");
        if (sector.PlantationId != command.PlantationId)
            throw new InvalidOperationException("Sector does not belong to this plantation.");

        sectors.Remove(sector);
        events.Enqueue("crop.sector-removed.v1", new SectorRemovedIntegrationEvent(
            command.SectorId, command.PlantationId, sector.IotDeviceMacAddress, DateTime.UtcNow));
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task<AgronomistAffiliationResponse> AffiliateAsync(
        AffiliateAgronomistCommand command,
        CancellationToken cancellationToken)
    {
        _ = await GetRequiredPlantationAsync(command.PlantationId, cancellationToken);
        var existing = await affiliations.FindAsync(command.AgronomistId, command.PlantationId, cancellationToken);
        if (existing is not null && existing.IsActive) return Map(existing);
        if (existing is not null)
            throw new InvalidOperationException("A detached affiliation must be recreated by an explicit invitation workflow.");

        return await unitOfWork.ExecuteAsync(async transactionToken =>
        {
            var affiliation = new AgronomistAffiliation(command.AgronomistId, command.PlantationId);
            await affiliations.AddAsync(affiliation, transactionToken);
            await unitOfWork.SaveChangesAsync(transactionToken);
            events.Enqueue("crop.agronomist-affiliated.v1", new AgronomistAffiliatedIntegrationEvent(
                affiliation.Id, affiliation.AgronomistId, affiliation.PlantationId, DateTime.UtcNow));
            await unitOfWork.SaveChangesAsync(transactionToken);
            return Map(affiliation);
        }, cancellationToken);
    }

    public async Task DetachAsync(DetachAgronomistCommand command, CancellationToken cancellationToken)
    {
        var affiliation = await affiliations.FindAsync(command.AgronomistId, command.PlantationId, cancellationToken)
            ?? throw new KeyNotFoundException("Active agronomist affiliation not found.");
        affiliation.Detach();
        events.Enqueue("crop.agronomist-detached.v1", new AgronomistDetachedIntegrationEvent(
            command.AgronomistId, command.PlantationId, DateTime.UtcNow));
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private async Task<Plantation> GetRequiredPlantationAsync(long id, CancellationToken cancellationToken) =>
        await plantations.GetAsync(id, cancellationToken)
        ?? throw new KeyNotFoundException($"Plantation '{id}' not found.");

    private static PlantationResponse Map(Plantation value) => new(
        value.Id, value.PalmGrowerId, value.Name, value.Address, value.Coordinates, value.Hectares,
        value.Status.ToString(), value.EstimatedSensors, value.InstallationMessage,
        value.CreatedAt, value.UpdatedAt);

    private static SectorResponse Map(Sector value) => new(
        value.Id, value.Name, value.IotDeviceMacAddress, value.Status.ToString(),
        value.CreatedAt, value.ActivatedAt);

    private static AgronomistAffiliationResponse Map(AgronomistAffiliation value) => new(
        value.Id, value.AgronomistId, value.PlantationId, value.AffiliatedAt, value.DetachedAt);
}
