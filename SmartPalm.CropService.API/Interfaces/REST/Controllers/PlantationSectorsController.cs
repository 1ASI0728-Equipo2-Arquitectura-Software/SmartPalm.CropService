using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartPalm.CropService.Application.Contracts;
using SmartPalm.CropService.Application.Services;
using SmartPalm.CropService.Domain.Model.Commands;
using SmartPalm.CropService.Interfaces.REST.Resources;
using SmartPalm.CropService.Interfaces.REST.Transform;

namespace SmartPalm.CropService.Api.Controllers;

[ApiController]
[Authorize(Roles = "Administrator")]
[Route("api/v1/plantations/{plantationId:long}/sectors")]
public class PlantationSectorsController(
    CropCommandService commandService,
    CropQueryService queryService) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<SectorResponse>> AssignSector(
        long plantationId,
        AssignSectorResource resource,
        CancellationToken cancellationToken)
    {
        var result = await commandService.AssignSectorAsync(resource.ToCommand(plantationId), cancellationToken);
        return StatusCode(StatusCodes.Status201Created, result);
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<SectorResponse>>> GetPlantationSectors(
        long plantationId,
        CancellationToken cancellationToken) =>
        Ok(await queryService.ListSectorsAsync(plantationId, cancellationToken));

    [HttpDelete("{sectorId:long}")]
    public async Task<IActionResult> RemoveSector(
        long plantationId,
        long sectorId,
        CancellationToken cancellationToken)
    {
        await commandService.RemoveSectorAsync(new RemoveSectorCommand(plantationId, sectorId), cancellationToken);
        return Ok(new { message = "Sector removed." });
    }
}
