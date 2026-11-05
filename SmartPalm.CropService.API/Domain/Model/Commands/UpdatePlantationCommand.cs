namespace SmartPalm.CropService.Domain.Model.Commands;

public record UpdatePlantationCommand(long PlantationId, string Name, decimal Hectares, string Address, string? Coordinates);
