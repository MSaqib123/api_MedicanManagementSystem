using MedicineManagementSystem.Models;
using MedicineManagementSystem.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace api_MedicanManagementSystem.Controllers;



[ApiController]
[Route("api/[controller]")]
[ApiVersion("1.0")]
//[Authorize]
public class PurchaseController : ControllerBase
{
    private readonly IPurchaseService _purchaseService;

    public PurchaseController(IPurchaseService purchaseService)
    {
        _purchaseService = purchaseService;
    }

    [HttpPost]
    [Authorize(Policy = "Accountant")]
    public async Task<IActionResult> CreatePurchase([FromBody] Purchase purchase)
    {
        var created = await _purchaseService.CreatePurchaseAsync(purchase);
        return Ok(created);
    }

    [HttpPost("alerts/due")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> SendDueAlerts()
    {
        await _purchaseService.SendDueAlertsAsync();
        return Ok();
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetPurchase(Guid id)
    {
        var purchase = await _purchaseService.GetPurchaseByIdAsync(id);
        return purchase != null ? Ok(purchase) : NotFound();
    }

    [HttpPut("{id}")]
    [Authorize(Policy = "Accountant")]
    public async Task<IActionResult> UpdatePurchase(Guid id, [FromBody] Purchase updated)
    {
        await _purchaseService.UpdatePurchaseAsync(id, updated);
        return Ok();
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> DeletePurchase(Guid id)
    {
        await _purchaseService.DeletePurchaseAsync(id);
        return Ok();
    }
}