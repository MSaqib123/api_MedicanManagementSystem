using MedicineManagementSystem.Models;
using MedicineManagementSystem.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace api_MedicanManagementSystem.Controllers;
[ApiController]
[Route("api/[controller]")]
[ApiVersion("1.0")]
//[Authorize]
public class SalesController : ControllerBase
{
    private readonly ISalesService _salesService;

    public SalesController(ISalesService salesService)
    {
        _salesService = salesService;
    }

    [HttpPost]
    [Authorize(Policy = "Pharmacist")]
    public async Task<IActionResult> CreateSale([FromBody] Sale sale)
    {
        var created = await _salesService.CreateSaleAsync(sale);
        return Ok(created);
    }

    [HttpPost("{id}/payment")]
    [Authorize(Policy = "Pharmacist")]
    public async Task<IActionResult> ProcessPayment(Guid id, [FromBody] string paymentToken)
    {
        await _salesService.ProcessPaymentAsync(id, paymentToken);
        return Ok();
    }

    [HttpPost("{id}/prescription")]
    [Authorize(Policy = "Pharmacist")]
    public async Task<IActionResult> UploadPrescription(Guid id, [FromForm] IFormFile file)
    {
        var path = Path.GetTempFileName();
        using (var stream = System.IO.File.Create(path))
        {
            await file.CopyToAsync(stream);
        }
        await _salesService.UploadPrescriptionAsync(id, path);
        return Ok();
    }

    [HttpPost("{id}/return")]
    [Authorize(Policy = "Pharmacist")]
    public async Task<IActionResult> ReturnSale(Guid id, [FromQuery] decimal refundAmount)
    {
        var returned = await _salesService.ReturnSaleAsync(id, refundAmount);
        return Ok(returned);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetSale(Guid id)
    {
        var sale = await _salesService.GetSaleByIdAsync(id);
        return sale != null ? Ok(sale) : NotFound();
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> DeleteSale(Guid id)
    {
        await _salesService.DeleteSaleAsync(id);
        return Ok();
    }
}