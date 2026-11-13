namespace SmartPalm.CropService.Application.Contracts;

public record CropReconciliationSnapshotResponse(
    IReadOnlyList<ReconciliationPlantationResponse> Plantations,
    IReadOnlyList<ReconciliationSectorResponse> Sectors,
    IReadOnlyList<ReconciliationAffiliationResponse> ActiveAgronomistAffiliations,
    long? NextPlantationId);

public record ReconciliationPlantationResponse(
    long Id,
    long PalmGrowerId,
    string Name);

public record ReconciliationSectorResponse(
    long Id,
    long PlantationId,
    string Name);

public record ReconciliationAffiliationResponse(
    long Id,
    long AgronomistId,
    long PlantationId);
