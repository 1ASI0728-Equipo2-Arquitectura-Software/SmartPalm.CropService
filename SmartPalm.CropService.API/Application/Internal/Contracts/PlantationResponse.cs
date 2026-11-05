namespace SmartPalm.CropService.Application.Contracts;

public record PlantationResponse(
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
    DateTime? UpdatedAt);
