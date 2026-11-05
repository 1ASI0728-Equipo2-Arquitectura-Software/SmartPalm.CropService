namespace SmartPalm.CropService.Domain.Model.Queries;

public record CanAccessPlantationQuery(long PlantationId, long UserId, string Role);
