using MedicineManagementSystem.Models;
using MedicineManagementSystem.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace api_MedicanManagementSystem.Controllers;

[ApiController]
[Route("api/[controller]")]
[ApiVersion("1.0")]
//[Authorize]
public class MedicineController : ControllerBase
{
    private readonly IMedicineService _medicineService;

    public MedicineController(IMedicineService medicineService)
    {
        _medicineService = medicineService;
    }

    [HttpPost]
    [Authorize(Policy = "Pharmacist")]
    public async Task<IActionResult> AddMedicine([FromBody] Medicine medicine)
    {
        var added = await _medicineService.AddMedicineAsync(medicine);
        return Ok(added);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetMedicine(Guid id)
    {
        var medicine = await _medicineService.GetMedicineByIdAsync(id);
        return medicine != null ? Ok(medicine) : NotFound();
    }

    [HttpGet("search")]
    public async Task<IActionResult> Search([FromQuery] string query)
    {
        var results = await _medicineService.SearchMedicinesAsync(query);
        return Ok(results);
    }

    [HttpPost("alerts/expiry")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> SendExpiryAlerts()
    {
        await _medicineService.SendExpiryAlertsAsync();
        return Ok();
    }

    [HttpPut("{id}")]
    [Authorize(Policy = "Pharmacist")]
    public async Task<IActionResult> UpdateMedicine(Guid id, [FromBody] Medicine updated)
    {
        await _medicineService.UpdateMedicineAsync(id, updated);
        return Ok();
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> DeleteMedicine(Guid id)
    {
        await _medicineService.DeleteMedicineAsync(id);
        return Ok();
    }
}