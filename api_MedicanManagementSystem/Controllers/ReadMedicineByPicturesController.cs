
// Controllers/ReadMedicineByPicturesController.cs - New controller
using MedicineManagementSystem.Data;
using MedicineManagementSystem.Models;
using MedicineManagementSystem.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

[ApiController]
[Route("api/[controller]")]
[ApiVersion("1.0")]
//[Authorize(Policy = "Pharmacist")] // Assume role
public class ReadMedicineByPicturesController : ControllerBase
{
    private readonly IMedicineOcrService _ocrService;
    private readonly IMedicineService _medicineService;
    private readonly IInventoryService _inventoryService;
    private readonly ApplicationDbContext _context; // For checks

    public ReadMedicineByPicturesController(IMedicineOcrService ocrService, IMedicineService medicineService, IInventoryService inventoryService, ApplicationDbContext context)
    {
        _ocrService = ocrService;
        _medicineService = medicineService;
        _inventoryService = inventoryService;
        _context = context;
    }

    [HttpPost]
    public async Task<IActionResult> ProcessMedicinePictures([FromForm] List<IFormFile> images, [FromForm] string? quantitiesJson = null)
    {
        if (images == null || images.Count == 0)
        {
            return BadRequest("No images uploaded.");
        }

        Dictionary<string, int> quantities = null;
        if (!string.IsNullOrEmpty(quantitiesJson))
        {
            try
            {
                quantities = JsonSerializer.Deserialize<Dictionary<string, int>>(quantitiesJson);
            }
            catch
            {
                return BadRequest("Invalid quantities JSON.");
            }
        }

        var results = new List<object>(); // For responses
        foreach (var image in images)
        {
            if (image.Length == 0 || (!image.ContentType.Equals("image/png", StringComparison.OrdinalIgnoreCase) && !image.ContentType.Equals("image/jpeg", StringComparison.OrdinalIgnoreCase)))
            {
                results.Add(new { FileName = image.FileName, Error = "Invalid image file." });
                continue;
            }

            try
            {
                using var ms = new MemoryStream();
                await image.CopyToAsync(ms);
                ms.Position = 0;

                var extractedMedicine = await _ocrService.ExtractMedicineFromImageAsync(ms);

                // Check if medicine exists
                var existingMedicine = await _context.Medicines.FirstOrDefaultAsync(m => m.Name == extractedMedicine.Name && m.Brand.Name == extractedMedicine.Brand.Name);

                Medicine medicine;
                if (existingMedicine == null)
                {
                    // Create new
                    // First, find or create Type and Brand
                    var type = await _context.MedicineTypes.FirstOrDefaultAsync(mt => mt.Name == extractedMedicine.MedicineType.Name);
                    if (type == null)
                    {
                        type = extractedMedicine.MedicineType;
                        _context.MedicineTypes.Add(type);
                        await _context.SaveChangesAsync();
                    }
                    extractedMedicine.MedicineTypeId = type.Id;

                    var brand = await _context.Brands.FirstOrDefaultAsync(b => b.Name == extractedMedicine.Brand.Name);
                    if (brand == null)
                    {
                        brand = extractedMedicine.Brand;
                        _context.Brands.Add(brand);
                        await _context.SaveChangesAsync();
                    }
                    extractedMedicine.BrandId = brand.Id;

                    medicine = await _medicineService.AddMedicineAsync(extractedMedicine);
                }
                else
                {
                    // Update existing if needed (advanced: merge fields)
                    existingMedicine.Composition = extractedMedicine.Composition ?? existingMedicine.Composition;
                    // etc.
                    await _medicineService.UpdateMedicineAsync(existingMedicine.Id, existingMedicine);
                    medicine = existingMedicine;
                }

                // If quantities provided, update stock
                if (quantities != null && quantities.TryGetValue(medicine.Name, out var quantity) && quantity > 0)
                {
                    // Assume for current branch (from context)
                    //var branchId = (Guid) HttpContext.Items["TenantBranchId"] ?? Guid.Empty; // Assume middleware sets
                    var branchId = HttpContext.Items["TenantBranchId"] ?? Guid.Empty; // Assume middleware sets
                    var inventory = await _context.Inventories.FirstOrDefaultAsync(i => i.MedicineId == medicine.Id && i.BranchId == (Guid)branchId);
                    if (inventory == null)
                    {
                        inventory = new Inventory
                        {
                            MedicineId = medicine.Id,
                            BranchId = (Guid)branchId,
                            BatchNumber = "OCR-" + DateTime.UtcNow.ToString("yyyyMMdd"), // Auto batch
                            ExpiryDate = DateTime.UtcNow.AddYears(2), // Assume
                            QuantityInStock = 0,
                            PurchasePrice = 0, // Assume
                            RetailPrice = 0 // Assume
                        };
                        await _inventoryService.AddStockAsync(inventory);
                    }
                    await _inventoryService.UpdateStockAsync(inventory.Id, quantity, "In"); // Add to stock
                }

                results.Add(new { FileName = image.FileName, Medicine = medicine, UpdatedStock = quantities != null });
            }
            catch (Exception ex)
            {
                results.Add(new { FileName = image.FileName, Error = ex.Message });
            }
        }

        return Ok(results);
    }
}