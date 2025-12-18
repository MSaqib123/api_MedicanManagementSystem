// Controllers/BrandController.cs
using Amazon.S3.Model;
using MedicineManagementSystem.Models;
using MedicineManagementSystem.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;

[ApiController]
[Route("api/[controller]")]
[ApiVersion("1.0")]
//[Authorize]
public class BrandController : ControllerBase
{
    private readonly IBrandService _brandService;

    public BrandController(IBrandService brandService)
    {
        _brandService = brandService;
    }

    [HttpPost]
    //[Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> CreateBrand([FromBody] Brand brand)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var created = await _brandService.CreateBrandAsync(brand);
        return CreatedAtAction(nameof(GetBrandById), new { id = created.Id }, created);
    }

    [HttpGet]
    public async Task<IActionResult> GetAllBrands()
    {
        try
        {
            var brands = await _brandService.GetAllBrandsAsync();
            return Ok(brands);
        }
        catch (Exception ex)
        {
            return NotFound();
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetBrandById(Guid id)
    {
        var brand = await _brandService.GetBrandByIdAsync(id);
        return brand != null ? Ok(brand) : NotFound();
    }

    [HttpPut("{id}")]
    //[Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> UpdateBrand(Guid id, [FromBody] Brand brand)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var updated  = await _brandService.UpdateBrandAsync(id, brand);
        return Ok(updated);
    }

    [HttpDelete("{id}")]
    //[Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> DeleteBrand(Guid id)
    {
        await _brandService.DeleteBrandAsync(id);
        return NoContent();
    }
}