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
[Route("api/v1/admin/plantations")]
public class AdminPlantationsController(
    CropCommandService commandService,
    CropQueryService queryService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<PlantationResponse>>> ListPlantations(CancellationToken cancellationToken) =>
        Ok(await queryService.ListAllAsync(cancellationToken));

    [HttpGet("{plantationId:long}")]
    public async Task<ActionResult<PlantationDetailResponse>> GetPlantationById(long plantationId, CancellationToken cancellationToken)
    {
        var result = await queryService.GetAsync(plantationId, cancellationToken);
        return result is null ? NotFound(new { message = "Plantation not found." }) : Ok(result);
    }

    [HttpPost("{plantationId:long}/sectors")]
    public async Task<ActionResult<SectorResponse>> AssignSector(
        long plantationId,
        AssignSectorResource resource,
        CancellationToken cancellationToken)
    {
        var result = await commandService.AssignSectorAsync(resource.ToCommand(plantationId), cancellationToken);
        return StatusCode(StatusCodes.Status201Created, result);
    }

    [HttpDelete("{plantationId:long}/sectors/{sectorId:long}")]
    public async Task<IActionResult> RemoveSector(long plantationId, long sectorId, CancellationToken cancellationToken)
    {
        await commandService.RemoveSectorAsync(new RemoveSectorCommand(plantationId, sectorId), cancellationToken);
        return Ok(new { message = "Sector removed." });
    }
}
