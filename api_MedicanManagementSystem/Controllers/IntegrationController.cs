using MedicineManagementSystem.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace api_MedicanManagementSystem.Controllers;




[ApiController]
[Route("api/[controller]")]
[ApiVersion("1.0")]
//[Authorize]
public class IntegrationController : ControllerBase
{
    private readonly IIntegrationService _integrationService;

    public IntegrationController(IIntegrationService integrationService)
    {
        _integrationService = integrationService;
    }

    [HttpGet("track/{trackingId}")]
    public async Task<IActionResult> TrackDelivery(string trackingId)
    {
        var status = await _integrationService.TrackDeliveryAsync(trackingId);
        return Ok(status);
    }

    [HttpPost("sync-regulatory")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> SyncRegulatory()
    {
        await _integrationService.SyncWithRegulatoryDbAsync();
        return Ok();
    }

    [HttpPost("sync-offline")]
    public async Task<IActionResult> SyncOffline([FromBody] string data)
    {
        await _integrationService.SyncOfflineDataAsync(data);
        return Ok();
    }
}