namespace SmartPalm.CropService.Domain.Model.Commands;

public record AssignSectorCommand(long PlantationId, string? IotDeviceMacAddress, string SectorName);
