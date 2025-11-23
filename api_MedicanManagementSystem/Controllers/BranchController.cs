using MedicineManagementSystem.Models;
using MedicineManagementSystem.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace api_MedicanManagementSystem.Controllers;


[ApiController]
[Route("api/[controller]")]
[ApiVersion("1.0")]
//[Authorize]
public class BranchController : ControllerBase
{
    private readonly IBranchService _branchService;

    public BranchController(IBranchService branchService)
    {
        _branchService = branchService;
    }

    [HttpPost]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> CreateBranch([FromBody] Branch branch)
    {
        var created = await _branchService.CreateBranchAsync(branch);
        return Ok(created);
    }

    [HttpGet("tenant/{tenantId}")]
    public async Task<IActionResult> GetBranches(Guid tenantId)
    {
        var branches = await _branchService.GetBranchesByTenantAsync(tenantId);
        return Ok(branches);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetBranch(Guid id)
    {
        var branch = await _branchService.GetBranchByIdAsync(id);
        return branch != null ? Ok(branch) : NotFound();
    }

    [HttpPut("{id}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> UpdateBranch(Guid id, [FromBody] Branch updated)
    {
        await _branchService.UpdateBranchAsync(id, updated);
        return Ok();
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> DeleteBranch(Guid id)
    {
        await _branchService.DeleteBranchAsync(id);
        return Ok();
    }
}
