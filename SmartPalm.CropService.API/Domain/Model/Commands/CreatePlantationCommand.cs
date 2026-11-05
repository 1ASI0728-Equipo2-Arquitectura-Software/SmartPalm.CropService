namespace SmartPalm.CropService.Domain.Model.Commands;

public record CreatePlantationCommand(long PalmGrowerId, string Name, decimal Hectares, string Address, string? Coordinates);
