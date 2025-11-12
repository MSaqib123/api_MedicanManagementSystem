//using Microsoft.AspNetCore.Http;
//using Microsoft.AspNetCore.Mvc;

//namespace api_MedicanManagementSystem.Controllers
//{
//    [Route("api/[controller]")]
//    [ApiController]
//    public class TenantController : ControllerBase
//    {
//    }
//}

// Controllers/TenantController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Threading.Tasks;
using MedicineManagementSystem.Services;
using MedicineManagementSystem.Models;
using System.Security.Claims;

[ApiController]
[Route("api/[controller]")]
[ApiVersion("1.0")]
[Authorize]
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

// Controllers/BranchController.cs
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.AspNetCore.Authorization;
//using System;
//using System.Threading.Tasks;
//using MedicineManagementSystem.Services;
//using MedicineManagementSystem.Models;

[ApiController]
[Route("api/[controller]")]
[ApiVersion("1.0")]
[Authorize]
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

// Controllers/MedicineController.cs
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.AspNetCore.Authorization;
//using System;
//using System.Threading.Tasks;
//using MedicineManagementSystem.Services;
//using MedicineManagementSystem.Models;

[ApiController]
[Route("api/[controller]")]
[ApiVersion("1.0")]
[Authorize]
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

// Controllers/InventoryController.cs
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.AspNetCore.Authorization;
//using System;
//using System.Threading.Tasks;
//using MedicineManagementSystem.Services;
//using MedicineManagementSystem.Models;
//using Microsoft.AspNetCore.Http;

[ApiController]
[Route("api/[controller]")]
[ApiVersion("1.0")]
[Authorize]
public class InventoryController : ControllerBase
{
    private readonly IInventoryService _inventoryService;

    public InventoryController(IInventoryService inventoryService)
    {
        _inventoryService = inventoryService;
    }

    [HttpPost]
    [Authorize(Policy = "Pharmacist")]
    public async Task<IActionResult> AddStock([FromBody] Inventory inventory)
    {
        var added = await _inventoryService.AddStockAsync(inventory);
        return Ok(added);
    }

    [HttpPut("{id}/update")]
    [Authorize(Policy = "Pharmacist")]
    public async Task<IActionResult> UpdateStock(Guid id, [FromQuery] int quantityChange, [FromQuery] string transactionType)
    {
        var updated = await _inventoryService.UpdateStockAsync(id, quantityChange, transactionType);
        return Ok(updated);
    }

    [HttpPost("transfer")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> TransferStock([FromBody] StockTransfer transfer)
    {
        await _inventoryService.TransferStockAsync(transfer);
        return Ok();
    }

    [HttpGet("lowstock/{branchId}")]
    public async Task<IActionResult> GetLowStockAlerts(Guid branchId)
    {
        var alerts = await _inventoryService.GetLowStockAlertsAsync(branchId);
        return Ok(alerts);
    }

    [HttpPost("import")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> ImportStock([FromForm] IFormFile csvFile, [FromQuery] Guid branchId)
    {
        var path = Path.GetTempFileName();
        using (var stream = System.IO.File.Create(path))
        {
            await csvFile.CopyToAsync(stream);
        }
        await _inventoryService.ImportStockFromCsvAsync(path, branchId);
        return Ok();
    }

    [HttpGet("export/{branchId}")]
    public async Task<IActionResult> ExportStock(Guid branchId)
    {
        var csvPath = await _inventoryService.ExportStockToCsvAsync(branchId);
        var bytes = System.IO.File.ReadAllBytes(csvPath);
        return File(bytes, "text/csv", "stock.csv");
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetInventory(Guid id)
    {
        var inventory = await _inventoryService.GetInventoryByIdAsync(id);
        return inventory != null ? Ok(inventory) : NotFound();
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> DeleteInventory(Guid id)
    {
        await _inventoryService.DeleteInventoryAsync(id);
        return Ok();
    }
}

// Controllers/SalesController.cs
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.AspNetCore.Authorization;
//using System;
//using System.Threading.Tasks;
//using MedicineManagementSystem.Services;
//using MedicineManagementSystem.Models;
//using Microsoft.AspNetCore.Http;

[ApiController]
[Route("api/[controller]")]
[ApiVersion("1.0")]
[Authorize]
public class SalesController : ControllerBase
{
    private readonly ISalesService _salesService;

    public SalesController(ISalesService salesService)
    {
        _salesService = salesService;
    }

    [HttpPost]
    [Authorize(Policy = "Pharmacist")]
    public async Task<IActionResult> CreateSale([FromBody] Sale sale)
    {
        var created = await _salesService.CreateSaleAsync(sale);
        return Ok(created);
    }

    [HttpPost("{id}/payment")]
    [Authorize(Policy = "Pharmacist")]
    public async Task<IActionResult> ProcessPayment(Guid id, [FromBody] string paymentToken)
    {
        await _salesService.ProcessPaymentAsync(id, paymentToken);
        return Ok();
    }

    [HttpPost("{id}/prescription")]
    [Authorize(Policy = "Pharmacist")]
    public async Task<IActionResult> UploadPrescription(Guid id, [FromForm] IFormFile file)
    {
        var path = Path.GetTempFileName();
        using (var stream = System.IO.File.Create(path))
        {
            await file.CopyToAsync(stream);
        }
        await _salesService.UploadPrescriptionAsync(id, path);
        return Ok();
    }

    [HttpPost("{id}/return")]
    [Authorize(Policy = "Pharmacist")]
    public async Task<IActionResult> ReturnSale(Guid id, [FromQuery] decimal refundAmount)
    {
        var returned = await _salesService.ReturnSaleAsync(id, refundAmount);
        return Ok(returned);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetSale(Guid id)
    {
        var sale = await _salesService.GetSaleByIdAsync(id);
        return sale != null ? Ok(sale) : NotFound();
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> DeleteSale(Guid id)
    {
        await _salesService.DeleteSaleAsync(id);
        return Ok();
    }
}

// Controllers/PurchaseController.cs
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.AspNetCore.Authorization;
//using System;
//using System.Threading.Tasks;
//using MedicineManagementSystem.Services;
//using MedicineManagementSystem.Models;

[ApiController]
[Route("api/[controller]")]
[ApiVersion("1.0")]
[Authorize]
public class PurchaseController : ControllerBase
{
    private readonly IPurchaseService _purchaseService;

    public PurchaseController(IPurchaseService purchaseService)
    {
        _purchaseService = purchaseService;
    }

    [HttpPost]
    [Authorize(Policy = "Accountant")]
    public async Task<IActionResult> CreatePurchase([FromBody] Purchase purchase)
    {
        var created = await _purchaseService.CreatePurchaseAsync(purchase);
        return Ok(created);
    }

    [HttpPost("alerts/due")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> SendDueAlerts()
    {
        await _purchaseService.SendDueAlertsAsync();
        return Ok();
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetPurchase(Guid id)
    {
        var purchase = await _purchaseService.GetPurchaseByIdAsync(id);
        return purchase != null ? Ok(purchase) : NotFound();
    }

    [HttpPut("{id}")]
    [Authorize(Policy = "Accountant")]
    public async Task<IActionResult> UpdatePurchase(Guid id, [FromBody] Purchase updated)
    {
        await _purchaseService.UpdatePurchaseAsync(id, updated);
        return Ok();
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> DeletePurchase(Guid id)
    {
        await _purchaseService.DeletePurchaseAsync(id);
        return Ok();
    }
}

// Controllers/AnalyticsController.cs
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.AspNetCore.Authorization;
//using System;
//using System.Threading.Tasks;
//using MedicineManagementSystem.Services;

[ApiController]
[Route("api/[controller]")]
[ApiVersion("1.0")]
[Authorize(Policy = "Accountant")]
public class AnalyticsController : ControllerBase
{
    private readonly IAnalyticsService _analyticsService;

    public AnalyticsController(IAnalyticsService analyticsService)
    {
        _analyticsService = analyticsService;
    }

    [HttpGet("daily-sales/{branchId}")]
    public async Task<IActionResult> GetDailySales(Guid branchId, [FromQuery] DateTime date)
    {
        var report = await _analyticsService.GetDailySalesReportAsync(branchId, date);
        return Ok(report);
    }

    [HttpGet("stock-aging/{branchId}")]
    public async Task<IActionResult> GetStockAging(Guid branchId)
    {
        var report = await _analyticsService.GetStockAgingReportAsync(branchId);
        return Ok(report);
    }

    [HttpGet("profit-loss/{branchId}")]
    public async Task<IActionResult> GetProfitLoss(Guid branchId, [FromQuery] DateTime start, [FromQuery] DateTime end)
    {
        var report = await _analyticsService.GetProfitLossStatementAsync(branchId, start, end);
        return Ok(report);
    }

    [HttpGet("dashboard/{tenantId}")]
    public async Task<IActionResult> GetDashboard(Guid tenantId)
    {
        var dashboard = await _analyticsService.GetRealtimeDashboardAsync(tenantId);
        return Ok(dashboard);
    }

    [HttpGet("supplier/{supplierId}")]
    public async Task<IActionResult> GetSupplierAnalytics(Guid supplierId)
    {
        var analytics = await _analyticsService.GetSupplierAnalyticsAsync(supplierId);
        return Ok(analytics);
    }

    [HttpGet("medicine/{medicineId}")]
    public async Task<IActionResult> GetMedicinePerformance(Guid medicineId)
    {
        var performance = await _analyticsService.GetMedicinePerformanceAsync(medicineId);
        return Ok(performance);
    }
}

// Controllers/UserController.cs
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.AspNetCore.Authorization;
//using System;
//using System.Threading.Tasks;
//using MedicineManagementSystem.Services;
//using MedicineManagementSystem.Models;
//using System.Collections.Generic;
//using System.Security.Claims;

[ApiController]
[Route("api/[controller]")]
[ApiVersion("1.0")]
[Authorize]
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
        if (claim != null) return BadRequest();
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
    public string Username { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }
    public Guid TenantId { get; set; }
    public Guid? BranchId { get; set; }
    public List<string> Roles { get; set; }
    public List<ClaimModel> Claims { get; set; }
}

public class LoginModel
{
    public string Username { get; set; }
    public string Password { get; set; }
}

public class ClaimModel
{
    public string Type { get; set; }
    public string Value { get; set; }
}

// Controllers/SubscriptionController.cs
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.AspNetCore.Authorization;
//using System;
//using System.Threading.Tasks;
//using MedicineManagementSystem.Services;
//using MedicineManagementSystem.Models;

[ApiController]
[Route("api/[controller]")]
[ApiVersion("1.0")]
[Authorize(Policy = "AdminOnly")]
public class SubscriptionController : ControllerBase
{
    private readonly ISubscriptionService _subscriptionService;

    public SubscriptionController(ISubscriptionService subscriptionService)
    {
        _subscriptionService = subscriptionService;
    }

    [HttpPost]
    public async Task<IActionResult> CreateSubscription([FromBody] Subscription sub)
    {
        var created = await _subscriptionService.CreateSubscriptionAsync(sub);
        return Ok(created);
    }

    [HttpGet("check/{tenantId}")]
    public async Task<IActionResult> CheckStatus(Guid tenantId)
    {
        await _subscriptionService.CheckSubscriptionStatusAsync(tenantId);
        return Ok();
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetSubscription(Guid id)
    {
        var sub = await _subscriptionService.GetSubscriptionByIdAsync(id);
        return sub != null ? Ok(sub) : NotFound();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> CancelSubscription(Guid id)
    {
        await _subscriptionService.CancelSubscriptionAsync(id);
        return Ok();
    }
}

// Controllers/NotificationController.cs
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.AspNetCore.Authorization;
//using System.Threading.Tasks;
//using MedicineManagementSystem.Services;
//using MedicineManagementSystem.Models;

[ApiController]
[Route("api/[controller]")]
[ApiVersion("1.0")]
[Authorize]
public class NotificationController : ControllerBase
{
    private readonly INotificationService _notificationService;

    public NotificationController(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    [HttpPost]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> SendNotification([FromBody] Notification notification)
    {
        await _notificationService.SendNotificationAsync(notification);
        return Ok();
    }
}

// Controllers/IntegrationController.cs
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.AspNetCore.Authorization;
//using System.Threading.Tasks;
//using MedicineManagementSystem.Services;

[ApiController]
[Route("api/[controller]")]
[ApiVersion("1.0")]
[Authorize]
public class IntegrationController : ControllerBase
{
    private readonly IIntegrationService _integrationService;

    public IntegrationController(IIntegrationService integrationService)
    {
        _integrationService = integrationService;
    }

    [HttpGet("track/{trackingId}")]
    public async Task<IActionResult> TrackDelivery(string trackingId)
    {
        var status = await _integrationService.TrackDeliveryAsync(trackingId);
        return Ok(status);
    }

    [HttpPost("sync-regulatory")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> SyncRegulatory()
    {
        await _integrationService.SyncWithRegulatoryDbAsync();
        return Ok();
    }

    [HttpPost("sync-offline")]
    public async Task<IActionResult> SyncOffline([FromBody] string data)
    {
        await _integrationService.SyncOfflineDataAsync(data);
        return Ok();
    }
}

// Controllers/BackupController.cs
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.AspNetCore.Authorization;
//using System;
//using System.Threading.Tasks;
//using MedicineManagementSystem.Services;

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
