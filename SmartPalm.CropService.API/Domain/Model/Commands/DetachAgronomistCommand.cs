namespace SmartPalm.CropService.Domain.Model.Commands;

public record DetachAgronomistCommand(long AgronomistId, long PlantationId);
