using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartPalm.CropService.Application.Contracts;
using SmartPalm.CropService.Application.Services;
using SmartPalm.CropService.Interfaces.REST.Resources;
using SmartPalm.CropService.Interfaces.REST.Transform;

namespace SmartPalm.CropService.Api.Controllers;

[ApiController]
[Authorize(Roles = "Administrator,PalmGrower,Agronomist")]
[Route("api/v1/plantations")]
public class PlantationsController(
    CropCommandService commandService,
    CropQueryService queryService) : ControllerBase
{
    [HttpPost]
    [Authorize(Roles = "Administrator,PalmGrower")]
    public async Task<ActionResult<PlantationResponse>> CreatePlantation(
        CreatePlantationResource resource,
        CancellationToken cancellationToken)
    {
        var palmGrowerId = User.IsInRole("Administrator")
            ? resource.PalmGrowerId ?? throw new ArgumentException("PalmGrowerId is required when an administrator creates a plantation.")
            : User.RequiredUserId();
        var result = await commandService.CreateAsync(resource.ToCommand(palmGrowerId), cancellationToken);
        return CreatedAtAction(nameof(GetPlantationById), new { plantationId = result.Id }, result);
    }

    [HttpGet]
    [Authorize(Roles = "PalmGrower")]
    public async Task<ActionResult<IReadOnlyList<PlantationResponse>>> GetMyPlantations(CancellationToken cancellationToken) =>
        Ok(await queryService.ListMineAsync(User.RequiredUserId(), cancellationToken));

    [HttpGet("{plantationId:long}")]
    public async Task<ActionResult<PlantationDetailResponse>> GetPlantationById(long plantationId, CancellationToken cancellationToken)
    {
        if (!await queryService.CanAccessAsync(plantationId, User.RequiredUserId(), User.RequiredRole(), cancellationToken))
            return Forbid();
        var result = await queryService.GetAsync(plantationId, cancellationToken);
        return result is null ? NotFound(new { message = "Plantation not found." }) : Ok(result);
    }

    [HttpPatch("{plantationId:long}")]
    [Authorize(Roles = "Administrator,PalmGrower")]
    public async Task<ActionResult<PlantationResponse>> UpdatePlantation(
        long plantationId,
        UpdatePlantationResource resource,
        CancellationToken cancellationToken)
    {
        if (!await queryService.CanAccessAsync(plantationId, User.RequiredUserId(), User.RequiredRole(), cancellationToken))
            return Forbid();
        return Ok(await commandService.UpdateAsync(resource.ToCommand(plantationId), cancellationToken));
    }

    [HttpGet("{plantationId:long}/agronomist-affiliations")]
    public async Task<ActionResult<IReadOnlyList<AgronomistAffiliationResponse>>> GetPlantationAgronomistAffiliations(
        long plantationId,
        CancellationToken cancellationToken)
    {
        if (!await queryService.CanAccessAsync(plantationId, User.RequiredUserId(), User.RequiredRole(), cancellationToken))
            return Forbid();
        return Ok(await queryService.ListByPlantationAsync(plantationId, cancellationToken));
    }
}
