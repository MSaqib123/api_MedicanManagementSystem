// Controllers/BrandController.cs
using Amazon.S3.Model;
using MedicineManagementSystem.Models;
using MedicineManagementSystem.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using System;
using Twilio.Types;

[ApiController]
[Route("api/[controller]")]
[ApiVersion("1.0")]
//[Authorize]
public class MedicineTypeController : ControllerBase
{
    private readonly IMedicineTypeService _medicineTypeService;

    public MedicineTypeController(IMedicineTypeService medicineTypeService)
    {
        _medicineTypeService = medicineTypeService;
    }

    [HttpPost]
    public async Task<IActionResult> CreateMedicineType([FromBody] MedicineType medicineType)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var created = await _medicineTypeService.CreateMedicineTypeAsync(medicineType);
        return CreatedAtAction(nameof(GetMedicineTypeById), new { id = created.Id }, created);
    }

    [HttpGet]
    public async Task<IActionResult> GetAllMedicineTypes()
    {
        var types = await _medicineTypeService.GetAllMedicineTypesAsync();
        return Ok(types);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetMedicineTypeById(Guid id)
    {
        var type = await _medicineTypeService.GetMedicineTypeByIdAsync(id);
        return type == null ? NotFound() : Ok(type);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateMedicineType(Guid id, [FromBody] MedicineType medicineType)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var updated = await _medicineTypeService.UpdateMedicineTypeAsync(id, medicineType);
        return Ok(updated);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteMedicineType(Guid id)
    {
        await _medicineTypeService.DeleteMedicineTypeAsync(id);
        return NoContent();
    }
}


//POST / api / medicinetype
//GET / api / medicinetype
//GET / api / medicinetype /{ id}
//PUT / api / medicinetype /{ id}
//DELETE / api / medicinetype /{ id}
