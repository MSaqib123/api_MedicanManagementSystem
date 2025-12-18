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
public class SubCategoryController : ControllerBase
{
    private readonly ISubCategoryService _subCategoryService;
    public SubCategoryController(ISubCategoryService subCategoryService)
    {
        _subCategoryService = subCategoryService;
    }

    [HttpPost]
    public async Task<IActionResult> CreateSubCategory([FromBody] SubCategory subCategory)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var created = await _subCategoryService.CreateSubCategoryAsync(subCategory);
        return CreatedAtAction(nameof(GetSubCategoryById), new { id = created.Id }, created);
    }

    [HttpGet]
    public async Task<IActionResult> GetAllSubCategories()
    {
        var subCategories = await _subCategoryService.GetAllSubCategoriesAsync();
        return Ok(subCategories);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetSubCategoryById(Guid id)
    {
        var subCategory = await _subCategoryService.GetSubCategoryByIdAsync(id);
        return subCategory == null ? NotFound() : Ok(subCategory);
    }

    [HttpGet("by-category/{categoryId}")]
    public async Task<IActionResult> GetSubCategoriesByCategory(Guid categoryId)
    {
        var data = await _subCategoryService.GetSubCategoriesByCategoryAsync(categoryId);
        return Ok(data);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateSubCategory(Guid id, [FromBody] SubCategory subCategory)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var updated = await _subCategoryService.UpdateSubCategoryAsync(id, subCategory);
        return Ok(updated);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteSubCategory(Guid id)
    {
        await _subCategoryService.DeleteSubCategoryAsync(id);
        return NoContent();
    }
}


//POST/api/subcategory
//GET/api/subcategory
//GET/api / subcategory /{ id}
//GET / api / subcategory / by - category /{ categoryId}
//PUT / api / subcategory /{ id}
//DELETE / api / subcategory /{ id}
