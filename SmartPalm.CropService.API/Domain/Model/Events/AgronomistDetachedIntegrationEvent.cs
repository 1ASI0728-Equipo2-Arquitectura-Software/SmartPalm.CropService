namespace SmartPalm.CropService.Domain.Events;

public record AgronomistDetachedIntegrationEvent(
    long AgronomistId, long PlantationId, DateTime OccurredAtUtc);
