using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartPalm.CropService.Application.Contracts;
using SmartPalm.CropService.Application.Services;

namespace SmartPalm.CropService.Api.Controllers;

[ApiController]
[Authorize(Roles = "Administrator,AgronomyService")]
[Route("api/v1/internal/reconciliation")]
public class CropReconciliationController(CropQueryService queryService) : ControllerBase
{
    [HttpGet("snapshot")]
    public async Task<ActionResult<CropReconciliationSnapshotResponse>> GetSnapshot(
        [FromQuery] long? palmGrowerId,
        [FromQuery] long? userId,
        [FromQuery] long? plantationId,
        [FromQuery] long? sectorId,
        [FromQuery] long? afterPlantationId,
        [FromQuery] int limit = 100,
        CancellationToken cancellationToken = default)
    {
        if (limit is < 1 or > 500)
            return BadRequest(new { message = "Limit must be between 1 and 500." });
        if (palmGrowerId is <= 0 || plantationId is <= 0 || afterPlantationId is <= 0)
            return BadRequest(new { message = "Snapshot identifiers must be greater than zero." });
        if (userId is <= 0 || sectorId is <= 0)
            return BadRequest(new { message = "Snapshot identifiers must be greater than zero." });
        if (palmGrowerId.HasValue && userId.HasValue && palmGrowerId != userId)
            return BadRequest(new { message = "palmGrowerId and userId must match when both are provided." });

        return Ok(await queryService.GetReconciliationSnapshotAsync(
            palmGrowerId ?? userId, plantationId, sectorId, afterPlantationId, limit, cancellationToken));
    }
}
