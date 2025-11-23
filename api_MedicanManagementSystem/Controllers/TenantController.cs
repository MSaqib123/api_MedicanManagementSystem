using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MedicineManagementSystem.Services;
using MedicineManagementSystem.Models;
using System.Security.Claims;
using System.ComponentModel.DataAnnotations;

[ApiController]
[Route("api/[controller]")]
[ApiVersion("1.0")]
//[Authorize]
public class TenantController : ControllerBase
{
    private readonly ITenantService _tenantService;

    public TenantController(ITenantService tenantService)
    {
        _tenantService = tenantService;
    }

    [HttpPost]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> CreateTenant([FromBody] Tenant tenant)
    {
        var created = await _tenantService.CreateTenantAsync(tenant);
        return Ok(created);
    }

    [HttpGet("{subdomain}")]
    public async Task<IActionResult> GetTenant(string subdomain)
    {
        var tenant = await _tenantService.GetTenantBySubdomainAsync(subdomain);
        return tenant != null ? Ok(tenant) : NotFound();
    }

    [HttpPut("{id}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> UpdateTenant(Guid id, [FromBody] Tenant updated)
    {
        await _tenantService.UpdateTenantConfigAsync(id, updated);
        return Ok();
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> DeleteTenant(Guid id)
    {
        await _tenantService.DeleteTenantAsync(id);
        return Ok();
    }
}


