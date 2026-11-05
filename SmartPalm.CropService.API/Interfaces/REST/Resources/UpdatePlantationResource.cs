namespace SmartPalm.CropService.Interfaces.REST.Resources;

public record UpdatePlantationResource(string Name, decimal Hectares, string Address, string? Coordinates);
