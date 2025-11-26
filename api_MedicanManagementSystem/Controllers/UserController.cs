using MedicineManagementSystem.Models;
using MedicineManagementSystem.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace api_MedicanManagementSystem.Controllers;


[ApiController]
[Route("api/[controller]")]
[ApiVersion("1.0")]
//[Authorize]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;

    public UserController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpPost("register")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> Register([FromBody] RegisterModel model)
    {
        var user = new ApplicationUser { UserName = model.Username, Email = model.Email, TenantId = model.TenantId, BranchId = model.BranchId };
        //var result = await _userService.RegisterUserAsync(user, model.Password, model.Roles, model.Claims);
        var claims = model.Claims
    .Select(c => new Claim(c.Type, c.Value))
    .ToList();

        var result = await _userService.RegisterUserAsync(user, model.Password, model.Roles, claims);
        return result.Succeeded ? Ok() : BadRequest(result.Errors);
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginModel model)
    {
        var token = await _userService.LoginAsync(model.Username, model.Password);
        return string.IsNullOrEmpty(token) ? Unauthorized() : Ok(new { Token = token });
    }

    [HttpPost("log-activity")]
    public async Task<IActionResult> LogActivity([FromQuery] Guid userId, [FromQuery] string action)
    {
        await _userService.LogActivityAsync(userId, action);
        return Ok();
    }

    [HttpPost("{id}/claim")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> AddClaim(Guid id, [FromBody] ClaimModel claimModel)
    {
        var claim = new Claim(claimModel.Type, claimModel.Value);
        if (claim == null) return BadRequest();
        await _userService.AddClaimToUserAsync(id, claim);
        return Ok();
    }

    [HttpGet("tenant/{tenantId}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> GetUsersByTenant(Guid tenantId)
    {
        var users = await _userService.GetUsersByTenantAsync(tenantId);
        return Ok(users);
    }

    [HttpPut("{id}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> UpdateUser([FromBody] ApplicationUser user)
    {
        await _userService.UpdateUserAsync(user);
        return Ok();
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> DeleteUser(Guid id)
    {
        await _userService.DeleteUserAsync(id);
        return Ok();
    }
}

public class RegisterModel
{
    public string? Username { get; set; }
    public string? Email { get; set; }
    public string? Password { get; set; }
    public Guid TenantId { get; set; }
    public Guid? BranchId { get; set; }
    public List<string>? Roles { get; set; }
    public List<ClaimModel>? Claims { get; set; }
}

public class LoginModel
{
    public string? Username { get; set; }
    public string? Password { get; set; }
}

public class ClaimModel
{
    public string? Type { get; set; }
    public string? Value { get; set; }
}
