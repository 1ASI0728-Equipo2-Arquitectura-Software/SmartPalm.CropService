namespace SmartPalm.CropService.Application.Contracts;

public record AgronomistAffiliationResponse(
    long Id,
    long AgronomistId,
    long PlantationId,
    DateTime AffiliatedAt,
    DateTime? DetachedAt);
