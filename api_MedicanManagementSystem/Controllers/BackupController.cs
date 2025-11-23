using MedicineManagementSystem.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace api_MedicanManagementSystem.Controllers;





[ApiController]
[Route("api/[controller]")]
[ApiVersion("1.0")]
[Authorize(Policy = "AdminOnly")]

public class BackupController : ControllerBase
{
    private readonly IBackupService _backupService;

    public BackupController(IBackupService backupService)
    {
        _backupService = backupService;
    }

    [HttpPost("daily/{tenantId}")]
    public async Task<IActionResult> PerformDailyBackup(Guid tenantId)
    {
        await _backupService.PerformDailyBackupAsync(tenantId);
        return Ok();
    }

    [HttpPost("restore/{backupId}")]
    public async Task<IActionResult> RestoreBackup(Guid backupId)
    {
        await _backupService.RestoreBackupAsync(backupId);
        return Ok();
    }
}

