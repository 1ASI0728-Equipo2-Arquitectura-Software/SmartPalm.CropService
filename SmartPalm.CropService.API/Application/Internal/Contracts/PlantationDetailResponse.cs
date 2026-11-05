namespace SmartPalm.CropService.Application.Contracts;

public record PlantationDetailResponse(
    long Id,
    long PalmGrowerId,
    string Name,
    string Address,
    string? Coordinates,
    decimal Hectares,
    string Status,
    int EstimatedSensors,
    string InstallationMessage,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    IReadOnlyList<SectorResponse> Sectors) : PlantationResponse(
        Id, PalmGrowerId, Name, Address, Coordinates, Hectares, Status, EstimatedSensors,
        InstallationMessage, CreatedAt, UpdatedAt);
