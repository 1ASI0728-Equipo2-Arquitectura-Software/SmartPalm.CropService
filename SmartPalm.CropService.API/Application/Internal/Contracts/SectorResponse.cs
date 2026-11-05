namespace SmartPalm.CropService.Application.Contracts;

public record SectorResponse(
    long Id,
    string Name,
    string? IotDeviceMacAddress,
    string Status,
    DateTime CreatedAt,
    DateTime? ActivatedAt);
