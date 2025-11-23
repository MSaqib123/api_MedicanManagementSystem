using MedicineManagementSystem.Models;
using MedicineManagementSystem.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace api_MedicanManagementSystem.Controllers;


[ApiController]
[Route("api/[controller]")]
[ApiVersion("1.0")]
//[Authorize]
public class InventoryController : ControllerBase
{
    private readonly IInventoryService _inventoryService;

    public InventoryController(IInventoryService inventoryService)
    {
        _inventoryService = inventoryService;
    }

    [HttpPost]
    [Authorize(Policy = "Pharmacist")]
    public async Task<IActionResult> AddStock([FromBody] Inventory inventory)
    {
        var added = await _inventoryService.AddStockAsync(inventory);
        return Ok(added);
    }

    [HttpPut("{id}/update")]
    [Authorize(Policy = "Pharmacist")]
    public async Task<IActionResult> UpdateStock(Guid id, [FromQuery] int quantityChange, [FromQuery] string transactionType)
    {
        var updated = await _inventoryService.UpdateStockAsync(id, quantityChange, transactionType);
        return Ok(updated);
    }

    [HttpPost("transfer")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> TransferStock([FromBody] StockTransfer transfer)
    {
        await _inventoryService.TransferStockAsync(transfer);
        return Ok();
    }

    [HttpGet("lowstock/{branchId}")]
    public async Task<IActionResult> GetLowStockAlerts(Guid branchId)
    {
        var alerts = await _inventoryService.GetLowStockAlertsAsync(branchId);
        return Ok(alerts);
    }
    [HttpPost("import")]
    [Authorize(Policy = "AdminOnly")]
    [Consumes("multipart/form-data")] // Optional but helpful
    public async Task<IActionResult> ImportStock([FromForm] ImportStockRequest request)
    {
        if (request.CsvFile == null || request.CsvFile.Length == 0)
            return BadRequest("File is required.");

        var path = Path.GetTempFileName();
        using (var stream = System.IO.File.Create(path))
        {
            await request.CsvFile.CopyToAsync(stream);
        }

        await _inventoryService.ImportStockFromCsvAsync(path, request.BranchId);
        return Ok();
    }
    [HttpGet("export/{branchId}")]
    public async Task<IActionResult> ExportStock(Guid branchId)
    {
        var csvPath = await _inventoryService.ExportStockToCsvAsync(branchId);
        var bytes = System.IO.File.ReadAllBytes(csvPath);
        return File(bytes, "text/csv", "stock.csv");
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetInventory(Guid id)
    {
        var inventory = await _inventoryService.GetInventoryByIdAsync(id);
        return inventory != null ? Ok(inventory) : NotFound();
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> DeleteInventory(Guid id)
    {
        await _inventoryService.DeleteInventoryAsync(id);
        return Ok();
    }
}



public class ImportStockRequest
{
    [Required]
    public IFormFile CsvFile { get; set; } = null!;

    [Required]
    public Guid BranchId { get; set; }
}
