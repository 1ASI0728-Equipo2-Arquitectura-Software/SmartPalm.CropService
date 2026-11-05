namespace SmartPalm.CropService.Interfaces.REST.Resources;

public record CreatePlantationResource(
    string Name,
    decimal Hectares,
    string Address,
    string? Coordinates,
    long? PalmGrowerId = null);
