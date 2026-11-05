using SmartPalm.CropService.Domain.Model.Commands;
using SmartPalm.CropService.Interfaces.REST.Resources;

namespace SmartPalm.CropService.Interfaces.REST.Transform;

public static class CropResourceAssemblers
{
    public static CreatePlantationCommand ToCommand(this CreatePlantationResource resource, long palmGrowerId) =>
        new(palmGrowerId, resource.Name, resource.Hectares, resource.Address, resource.Coordinates);
    public static UpdatePlantationCommand ToCommand(this UpdatePlantationResource resource, long plantationId) =>
        new(plantationId, resource.Name, resource.Hectares, resource.Address, resource.Coordinates);
    public static AssignSectorCommand ToCommand(this AssignSectorResource resource, long plantationId) =>
        new(plantationId, resource.IotDeviceMacAddress, resource.SectorName);
    public static AffiliateAgronomistCommand ToCommand(this CreateAgronomistAffiliationResource resource) =>
        new(resource.AgronomistId, resource.PlantationId);
}
