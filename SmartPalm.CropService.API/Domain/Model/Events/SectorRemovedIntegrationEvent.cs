namespace SmartPalm.CropService.Domain.Events;

public record SectorRemovedIntegrationEvent(
    long SectorId, long PlantationId, string? DeviceMacAddress, DateTime OccurredAtUtc);
