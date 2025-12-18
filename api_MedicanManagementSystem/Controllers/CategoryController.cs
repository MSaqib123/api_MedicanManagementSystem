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
public class CategoryController : ControllerBase
{
    private readonly ICategoryService _categoryService;

    public CategoryController(ICategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    [HttpPost]
    public async Task<IActionResult> CreateCategory([FromBody] Category category)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var created = await _categoryService.CreateCategoryAsync(category);
        return CreatedAtAction(nameof(GetCategoryById), new { id = created.Id }, created);
    }

    [HttpGet]
    public async Task<IActionResult> GetAllCategories()
    {
        var categories = await _categoryService.GetAllCategoriesAsync();
        return Ok(categories);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetCategoryById(Guid id)
    {
        var category = await _categoryService.GetCategoryByIdAsync(id);
        return category == null ? NotFound() : Ok(category);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateCategory(Guid id, [FromBody] Category category)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var updated = await _categoryService.UpdateCategoryAsync(id, category);
        return Ok(updated);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteCategory(Guid id)
    {
        await _categoryService.DeleteCategoryAsync(id);
        return NoContent();
    }
}


//POST / api / category
//GET / api / category
//GET / api / category /{ id}
//PUT / api / category /{ id}
//DELETE / api / category /{ id}
