using MedicineManagementSystem.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace api_MedicanManagementSystem.Controllers;



[ApiController]
[Route("api/[controller]")]
[ApiVersion("1.0")]
[Authorize(Policy = "AdminOnly")]
public class SubscriptionController : ControllerBase
{
    private readonly ISubscriptionService _subscriptionService;

    public SubscriptionController(ISubscriptionService subscriptionService)
    {
        _subscriptionService = subscriptionService;
    }

    [HttpPost]
    public async Task<IActionResult> CreateSubscription([FromBody] Subscription sub)
    {
        var created = await _subscriptionService.CreateSubscriptionAsync(sub);
        return Ok(created);
    }

    [HttpGet("check/{tenantId}")]
    public async Task<IActionResult> CheckStatus(Guid tenantId)
    {
        await _subscriptionService.CheckSubscriptionStatusAsync(tenantId);
        return Ok();
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetSubscription(Guid id)
    {
        var sub = await _subscriptionService.GetSubscriptionByIdAsync(id);
        return sub != null ? Ok(sub) : NotFound();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> CancelSubscription(Guid id)
    {
        await _subscriptionService.CancelSubscriptionAsync(id);
        return Ok();
    }
}
