using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartPalm.CropService.Application.Contracts;
using SmartPalm.CropService.Application.Services;
using SmartPalm.CropService.Domain.Model.Commands;
using SmartPalm.CropService.Interfaces.REST.Resources;
using SmartPalm.CropService.Interfaces.REST.Transform;

namespace SmartPalm.CropService.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/agronomists")]
public class AgronomistsController(
    CropCommandService commandService,
    CropQueryService queryService) : ControllerBase
{
    [HttpGet("{agronomistId:long}/plantation-affiliations")]
    public async Task<ActionResult<IReadOnlyList<AgronomistAffiliationResponse>>> GetAgronomistAffiliations(
        long agronomistId,
        CancellationToken cancellationToken)
    {
        if (!User.IsInRole("Administrator") && User.RequiredUserId() != agronomistId) return Forbid();
        return Ok(await queryService.ListByAgronomistAsync(agronomistId, cancellationToken));
    }

    [HttpPost("plantation-affiliations")]
    [Authorize(Roles = "Administrator")]
    public async Task<ActionResult<AgronomistAffiliationResponse>> CreateAgronomistAffiliation(
        CreateAgronomistAffiliationResource resource,
        CancellationToken cancellationToken)
    {
        var result = await commandService.AffiliateAsync(resource.ToCommand(), cancellationToken);
        return StatusCode(StatusCodes.Status201Created, result);
    }

    [HttpDelete("{agronomistId:long}/plantation-affiliations/{plantationId:long}")]
    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> DetachAgronomistAffiliation(
        long agronomistId,
        long plantationId,
        CancellationToken cancellationToken)
    {
        await commandService.DetachAsync(new DetachAgronomistCommand(agronomistId, plantationId), cancellationToken);
        return NoContent();
    }
}
