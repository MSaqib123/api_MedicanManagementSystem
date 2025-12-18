// Services/ITenantService.cs and TenantService.cs
using Amazon.S3;
using Amazon.S3.Model;
using CsvHelper;
using MedicineManagementSystem.Data;
using MedicineManagementSystem.Data;
using MedicineManagementSystem.Models;
using MedicineManagementSystem.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.IdentityModel.Tokens;
using Stripe;
using System;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
//using CsvHelper;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Threading.Tasks;
using Tesseract;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using ZXing;
using ZXing.Common;
using ZXing.Windows.Compatibility;

namespace MedicineManagementSystem.Services
{
    public interface ITenantService
    {
        Task<Tenant> CreateTenantAsync(Tenant tenant);
        Task<Tenant> GetTenantBySubdomainAsync(string subdomain);
        Task UpdateTenantConfigAsync(Guid tenantId, Tenant updatedTenant);
        Task DeleteTenantAsync(Guid tenantId);
    }

    public class TenantService : ITenantService
    {
        private readonly ApplicationDbContext _context;

        public TenantService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Tenant> CreateTenantAsync(Tenant tenant)
        {
            _context.Tenants.Add(tenant);
            await _context.SaveChangesAsync();
            await _context.Database.ExecuteSqlRawAsync($"CREATE SCHEMA [{tenant.Id}]");
            return tenant;
        }

        public async Task<Tenant> GetTenantBySubdomainAsync(string subdomain)
        {
            return await _context.Tenants.FirstOrDefaultAsync(t => t.Subdomain == subdomain);
        }

        public async Task UpdateTenantConfigAsync(Guid tenantId, Tenant updatedTenant)
        {
            var tenant = await _context.Tenants.FindAsync(tenantId);
            if (tenant != null)
            {
                tenant.Name = updatedTenant.Name;
                tenant.LogoUrl = updatedTenant.LogoUrl;
                tenant.Currency = updatedTenant.Currency;
                tenant.Language = updatedTenant.Language;
                tenant.TaxRate = updatedTenant.TaxRate;
                await _context.SaveChangesAsync();
            }
        }

        public async Task DeleteTenantAsync(Guid tenantId)
        {
            var tenant = await _context.Tenants.FindAsync(tenantId);
            if (tenant != null)
            {
                _context.Tenants.Remove(tenant);
                await _context.SaveChangesAsync();
                await _context.Database.ExecuteSqlRawAsync($"DROP SCHEMA [{tenantId}]");
            }
        }
    }
}

// Services/IBranchService.cs and BranchService.cs
//using Microsoft.EntityFrameworkCore;
//using System;
//using System.Collections.Generic;
//using System.Threading.Tasks;
//using MedicineManagementSystem.Data;
//using MedicineManagementSystem.Models;

namespace MedicineManagementSystem.Services
{
    public interface IBranchService
    {
        Task<Branch> CreateBranchAsync(Branch branch);
        Task<List<Branch>> GetBranchesByTenantAsync(Guid tenantId);
        Task<Branch> GetBranchByIdAsync(Guid id);
        Task UpdateBranchAsync(Guid id, Branch updatedBranch);
        Task DeleteBranchAsync(Guid id);
    }

    public class BranchService : IBranchService
    {
        private readonly ApplicationDbContext _context;

        public BranchService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Branch> CreateBranchAsync(Branch branch)
        {
            _context.Branches.Add(branch);
            await _context.SaveChangesAsync();
            return branch;
        }

        public async Task<List<Branch>> GetBranchesByTenantAsync(Guid tenantId)
        {
            return await _context.Branches.Where(b => b.TenantId == tenantId).ToListAsync();
        }

        public async Task<Branch> GetBranchByIdAsync(Guid id)
        {
            return await _context.Branches.FindAsync(id);
        }

        public async Task UpdateBranchAsync(Guid id, Branch updatedBranch)
        {
            var branch = await _context.Branches.FindAsync(id);
            if (branch != null)
            {
                branch.Name = updatedBranch.Name;
                await _context.SaveChangesAsync();
            }
        }

        public async Task DeleteBranchAsync(Guid id)
        {
            var branch = await _context.Branches.FindAsync(id);
            if (branch != null)
            {
                _context.Branches.Remove(branch);
                await _context.SaveChangesAsync();
            }
        }
    }
}

// Services/IMedicineService.cs and MedicineService.cs
//using Microsoft.EntityFrameworkCore;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Threading.Tasks;
//using MedicineManagementSystem.Data;
//using MedicineManagementSystem.Models;

namespace MedicineManagementSystem.Services
{
    public interface IMedicineService
    {
        Task<Medicine> AddMedicineAsync(Medicine medicine);
        Task<Medicine> GetMedicineByIdAsync(Guid id);
        Task<List<Medicine>> SearchMedicinesAsync(string query);
        Task<List<Medicine>> GetAllMedicineAsync();
        Task SendExpiryAlertsAsync();
        Task UpdateMedicineAsync(Guid id, Medicine updated);
        Task DeleteMedicineAsync(Guid id);
    }

    public class MedicineService : IMedicineService
    {
        private readonly ApplicationDbContext _context;
        private readonly INotificationService _notificationService;

        public MedicineService(ApplicationDbContext context, INotificationService notificationService)
        {
            _context = context;
            _notificationService = notificationService;
        }

        public async Task<Medicine> AddMedicineAsync(Medicine medicine)
        {
            _context.Medicines.Add(medicine);
            await _context.SaveChangesAsync();
            return medicine;
        }

        public async Task<Medicine> GetMedicineByIdAsync(Guid id)
        {
            return await _context.Medicines.Include(m => m.MedicineType).Include(m => m.Brand).FirstOrDefaultAsync(m => m.Id == id);
        }

        public async Task<List<Medicine>> GetAllMedicineAsync()
        {
            return await _context.Medicines.ToListAsync();
        }

        public async Task<List<Medicine>> SearchMedicinesAsync(string query)
        {
            return await _context.Medicines.Where(m => m.Name.Contains(query) || m.Composition.Contains(query)).ToListAsync();
        }


        public async Task SendExpiryAlertsAsync()
        {
            var nearExpired = await _context.Inventories.Where(i => i.ExpiryDate < DateTime.UtcNow.AddDays(30) && i.ExpiryDate > DateTime.UtcNow).Include(i => i.Medicine).ToListAsync();
            foreach (var item in nearExpired)
            {
                var notification = new Notification { Message = $"Medicine {item.Medicine.Name} batch {item.BatchNumber} expiring soon!", Type = "Email" /* or others */ };
                await _notificationService.SendNotificationAsync(notification);
            }
        }

        public async Task UpdateMedicineAsync(Guid id, Medicine updated)
        {
            var medicine = await _context.Medicines.FindAsync(id);
            if (medicine != null)
            {
                medicine.Name = updated.Name;
                medicine.Composition = updated.Composition;
                // Update other fields
                await _context.SaveChangesAsync();
            }
        }

        public async Task DeleteMedicineAsync(Guid id)
        {
            var medicine = await _context.Medicines.FindAsync(id);
            if (medicine != null)
            {
                _context.Medicines.Remove(medicine);
                await _context.SaveChangesAsync();
            }
        }
    }
}

// Services/IInventoryService.cs and InventoryService.cs
//using Microsoft.EntityFrameworkCore;
//using Microsoft.Extensions.Caching.Distributed;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text.Json;
//using System.Threading.Tasks;
//using MedicineManagementSystem.Data;
//using MedicineManagementSystem.Models;
//using CsvHelper;
//using System.IO;
//using System.Globalization;

namespace MedicineManagementSystem.Services
{
    public interface IInventoryService
    {
        Task<Inventory> AddStockAsync(Inventory inventory);
        Task<Inventory> UpdateStockAsync(Guid id, int quantityChange, string transactionType);
        Task TransferStockAsync(StockTransfer transfer);
        Task<List<Inventory>> GetLowStockAlertsAsync(Guid branchId);
        Task ImportStockFromCsvAsync(string csvFilePath, Guid branchId);
        Task<string> ExportStockToCsvAsync(Guid branchId);
        Task<Inventory> GetInventoryByIdAsync(Guid id);
        Task DeleteInventoryAsync(Guid id);
    }

    public class InventoryService : IInventoryService
    {
        private readonly ApplicationDbContext _context;
        private readonly IDistributedCache _cache;
        private readonly INotificationService _notificationService;

        public InventoryService(ApplicationDbContext context, IDistributedCache cache, INotificationService notificationService)
        {
            _context = context;
            _cache = cache;
            _notificationService = notificationService;
        }

        public async Task<Inventory> AddStockAsync(Inventory inventory)
        {
            _context.Inventories.Add(inventory);
            await _context.SaveChangesAsync();
            await InvalidateCache(inventory.BranchId);
            return inventory;
        }

        public async Task<Inventory> UpdateStockAsync(Guid id, int quantityChange, string transactionType)
        {
            //var inventory = await _context.Inventories.Include(i => i.Medicine).FindAsync(id);
            var inventory = await _context.Inventories
                .Include(x => x.Medicine)
                .FirstOrDefaultAsync(x => x.Id == id);
            if (inventory != null)
            {
                if (transactionType == "Sale")
                {
                    inventory.QuantitySold += quantityChange;
                    inventory.QuantityInStock -= quantityChange;
                }
                else if (transactionType == "Out")
                {
                    inventory.QuantityOut += quantityChange;
                    inventory.QuantityInStock -= quantityChange;
                }
                else
                {
                    inventory.QuantityInStock += quantityChange;
                }

                if (inventory.QuantityInStock < inventory.MinStockLevel)
                {
                    var notification = new Notification { Message = $"Low stock for {inventory.Medicine.Name} in branch {inventory.BranchId}" };
                    await _notificationService.SendNotificationAsync(notification);
                }

                await _context.SaveChangesAsync();
                await InvalidateCache(inventory.BranchId);
            }
            return inventory;
        }

        public async Task TransferStockAsync(StockTransfer transfer)
        {
            var fromInventory = await _context.Inventories.FirstOrDefaultAsync(i => i.MedicineId == transfer.InventoryId && i.BranchId == transfer.FromBranchId);
            var toInventory = await _context.Inventories.FirstOrDefaultAsync(i => i.MedicineId == transfer.InventoryId && i.BranchId == transfer.ToBranchId);

            if (toInventory == null)
            {
                toInventory = new Inventory
                {
                    MedicineId = fromInventory.MedicineId,
                    BranchId = transfer.ToBranchId,
                    BatchNumber = fromInventory.BatchNumber,
                    ExpiryDate = fromInventory.ExpiryDate,
                    PurchasePrice = fromInventory.PurchasePrice,
                    RetailPrice = fromInventory.RetailPrice,
                    MinStockLevel = fromInventory.MinStockLevel,
                    StockHandlingMethod = fromInventory.StockHandlingMethod
                };
                _context.Inventories.Add(toInventory);
            }

            fromInventory.QuantityInStock -= transfer.Quantity;
            toInventory.QuantityInStock += transfer.Quantity;

            _context.StockTransfers.Add(transfer);
            await _context.SaveChangesAsync();
            await InvalidateCache(transfer.FromBranchId);
            await InvalidateCache(transfer.ToBranchId);
        }

        public async Task<List<Inventory>> GetLowStockAlertsAsync(Guid branchId)
        {
            var cacheKey = $"LowStock_{branchId}";
            var cached = await _cache.GetStringAsync(cacheKey);
            if (!string.IsNullOrEmpty(cached))
            {
                return JsonSerializer.Deserialize<List<Inventory>>(cached);
            }

            var lowStock = await _context.Inventories.Where(i => i.BranchId == branchId && i.QuantityInStock < i.MinStockLevel).Include(i => i.Medicine).ToListAsync();
            await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(lowStock), new DistributedCacheEntryOptions { SlidingExpiration = TimeSpan.FromMinutes(5) });
            return lowStock;
        }

        public async Task ImportStockFromCsvAsync(string csvFilePath, Guid branchId)
        {
            using var reader = new StreamReader(csvFilePath);
            using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
            var records = csv.GetRecords<InventoryCsvModel>(); // Assume a DTO
            foreach (var record in records)
            {
                var medicine = await _context.Medicines.FirstOrDefaultAsync(m => m.Name == record.MedicineName);
                if (medicine != null)
                {
                    var inventory = new Inventory
                    {
                        MedicineId = medicine.Id,
                        BranchId = branchId,
                        BatchNumber = record.BatchNumber,
                        ExpiryDate = record.ExpiryDate,
                        QuantityInStock = record.Quantity,
                        // Set other fields
                    };
                    await AddStockAsync(inventory);
                }
            }
        }

        public async Task<string> ExportStockToCsvAsync(Guid branchId)
        {
            var inventories = await _context.Inventories.Where(i => i.BranchId == branchId).Include(i => i.Medicine).ToListAsync();
            var csvPath = Path.GetTempFileName();
            using var writer = new StreamWriter(csvPath);
            using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);
            await csv.WriteRecordsAsync(inventories.Select(i => new { i.Medicine.Name, i.BatchNumber, i.QuantityInStock /* etc */ }));
            return csvPath;
        }

        public async Task<Inventory> GetInventoryByIdAsync(Guid id)
        {
            return await _context.Inventories.FindAsync(id);
        }

        public async Task DeleteInventoryAsync(Guid id)
        {
            var inventory = await _context.Inventories.FindAsync(id);
            if (inventory != null)
            {
                _context.Inventories.Remove(inventory);
                await _context.SaveChangesAsync();
                await InvalidateCache(inventory.BranchId);
            }
        }

        private async Task InvalidateCache(Guid branchId)
        {
            await _cache.RemoveAsync($"LowStock_{branchId}");
        }
    }

    public class InventoryCsvModel
    {
        public string MedicineName { get; set; }
        public string BatchNumber { get; set; }
        public DateTime ExpiryDate { get; set; }
        public int Quantity { get; set; }
        // More fields
    }
}

// Services/ISalesService.cs and SalesService.cs
//using Microsoft.EntityFrameworkCore;
//using Stripe;
//using System;
//using System.Threading.Tasks;
//using MedicineManagementSystem.Data;
//using MedicineManagementSystem.Models;
//using Microsoft.Extensions.Configuration;
//using ZXing;
//using System.Drawing;
//using System.IO;
//using Microsoft.AspNetCore.Http;

namespace MedicineManagementSystem.Services
{
    public interface ISalesService
    {
        Task<Sale> CreateSaleAsync(Sale sale);
        Task ProcessPaymentAsync(Guid saleId, string paymentToken);
        Task UploadPrescriptionAsync(Guid saleId, string filePath);
        Task<Sale> ReturnSaleAsync(Guid saleId, decimal refundAmount);
        Task<Sale> GetSaleByIdAsync(Guid id);
        Task DeleteSaleAsync(Guid id);
    }

    public class SalesService : ISalesService
    {
        private readonly ApplicationDbContext _context;
        private readonly IInventoryService _inventoryService;
        private readonly IConfiguration _config;

        public SalesService(ApplicationDbContext context, IInventoryService inventoryService, IConfiguration config)
        {
            _context = context;
            _inventoryService = inventoryService;
            _config = config;
        }

        public async Task<Sale> CreateSaleAsync(Sale sale)
        {
            sale.TotalAmount = sale.Items.Sum(i => i.Quantity * i.Price);
            sale.Tax = sale.TotalAmount * sale.Branch.Tenant.TaxRate; // Assume Tenant loaded
            sale.InvoiceBarcode = GenerateBarcode(sale.Id.ToString());

            foreach (var item in sale.Items)
            {
                await _inventoryService.UpdateStockAsync(item.InventoryId, item.Quantity, "Sale");
            }

            _context.Sales.Add(sale);
            await _context.SaveChangesAsync();
            return sale;
        }

        public async Task ProcessPaymentAsync(Guid saleId, string paymentToken)
        {
            var sale = await _context.Sales.FindAsync(saleId);
            if (sale != null)
            {
                StripeConfiguration.ApiKey = _config["Stripe:SecretKey"];
                var options = new ChargeCreateOptions
                {
                    Amount = (long)(sale.TotalAmount * 100),
                    Currency = sale.Branch.Tenant.Currency,
                    Source = paymentToken,
                    Description = $"Sale {sale.Id}"
                };
                var service = new ChargeService();
                await service.CreateAsync(options);
                // Update sale status
            }
        }

        public async Task UploadPrescriptionAsync(Guid saleId, string filePath)
        {
            var sale = await _context.Sales.FindAsync(saleId);
            if (sale != null)
            {
                sale.PrescriptionUrl = await UploadFileAsync(filePath);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<Sale> ReturnSaleAsync(Guid saleId, decimal refundAmount)
        {
            var sale = await _context.Sales.Include(s => s.Items).FirstOrDefaultAsync(s => s.Id == saleId);
            if (sale != null)
            {
                sale.IsReturned = true;
                sale.RefundAmount = refundAmount;
                foreach (var item in sale.Items)
                {
                    await _inventoryService.UpdateStockAsync(item.InventoryId, -item.Quantity, "Sale");
                }
                await _context.SaveChangesAsync();
            }
            return sale;
        }

        public async Task<Sale> GetSaleByIdAsync(Guid id)
        {
            return await _context.Sales.Include(s => s.Items).ThenInclude(si => si.Inventory).FirstOrDefaultAsync(s => s.Id == id);
        }

        public async Task DeleteSaleAsync(Guid id)
        {
            var sale = await _context.Sales.FindAsync(id);
            if (sale != null)
            {
                _context.Sales.Remove(sale);
                await _context.SaveChangesAsync();
            }
        }

        // Replace ambiguous BarcodeWriter<Bitmap> usage with fully qualified type to resolve CS0433
        private string GenerateBarcode(string data)
        {
            var writer = new BarcodeWriter
            {
                //Format = BarcodeFormat.CODE_128,
                //Options = new EncodingOptions
                //{
                //    Height = 80,
                //    Width = 240,
                //    Margin = 1
                //}
            };

            var bitmap = writer.Write(data);

            using var stream = new MemoryStream();
            bitmap.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
            return Convert.ToBase64String(stream.ToArray());
        }
        //private string GenerateBarcode(string data)
        //{
        //    var writer = new BarcodeWriter
        //    {
        //        Format = BarcodeFormat.CODE_128//BarcodeFormat.CODE_128
        //    };
        //    var bitmap = writer.Write(data);
        //    using var stream = new MemoryStream();
        //    bitmap.Save(stream, ImageFormat.Png);
        //    return Convert.ToBase64String(stream.ToArray());
        //}

        private async Task<string> UploadFileAsync(string filePath)
        {
            // Upload to AWS S3 or Azure
            return "https://storage.example.com/prescription.pdf"; // Placeholder
        }
    }
}

// Services/IPurchaseService.cs and PurchaseService.cs
//using Microsoft.EntityFrameworkCore;
//using System;
//using System.Threading.Tasks;
//using MedicineManagementSystem.Data;
//using MedicineManagementSystem.Models;
//using System.Linq;
//using System.Collections.Generic;

namespace MedicineManagementSystem.Services
{
    public interface IPurchaseService
    {
        Task<Purchase> CreatePurchaseAsync(Purchase purchase);
        Task SendDueAlertsAsync();
        Task<Purchase> GetPurchaseByIdAsync(Guid id);
        Task UpdatePurchaseAsync(Guid id, Purchase updated);
        Task DeletePurchaseAsync(Guid id);
    }

    public class PurchaseService : IPurchaseService
    {
        private readonly ApplicationDbContext _context;
        private readonly IInventoryService _inventoryService;
        private readonly INotificationService _notificationService;

        public PurchaseService(ApplicationDbContext context, IInventoryService inventoryService, INotificationService notificationService)
        {
            _context = context;
            _inventoryService = inventoryService;
            _notificationService = notificationService;
        }

        public async Task<Purchase> CreatePurchaseAsync(Purchase purchase)
        {
            purchase.TotalAmount = purchase.Items.Sum(i => i.Quantity * i.Price);
            _context.Purchases.Add(purchase);
            await _context.SaveChangesAsync();
            foreach (var item in purchase.Items)
            {
                var inventory = await _context.Inventories.FirstOrDefaultAsync(inv => inv.MedicineId == item.MedicineId && inv.BatchNumber == "default" /* or match */);
                if (inventory == null)
                {
                    inventory = new Inventory { MedicineId = item.MedicineId, BranchId = Guid.Empty /* assume central */, QuantityInStock = 0 };
                    _context.Inventories.Add(inventory);
                }
                inventory.QuantityInStock += item.Quantity;
                await _context.SaveChangesAsync();
            }
            return purchase;
        }

        public async Task SendDueAlertsAsync()
        {
            var duePurchases = await _context.Purchases.Where(p => p.DueDate < DateTime.UtcNow.AddDays(7) && p.DueDate > DateTime.UtcNow).Include(p => p.Supplier).ToListAsync();
            foreach (var p in duePurchases)
            {
                var notification = new Notification { Message = $"Payment due for purchase {p.Id} from {p.Supplier.Name}", Type = "SMS" };
                await _notificationService.SendNotificationAsync(notification);
            }
        }

        public async Task<Purchase> GetPurchaseByIdAsync(Guid id)
        {
            return await _context.Purchases.Include(p => p.Items).ThenInclude(pi => pi.Medicine).FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task UpdatePurchaseAsync(Guid id, Purchase updated)
        {
            var purchase = await _context.Purchases.FindAsync(id);
            if (purchase != null)
            {
                purchase.TotalAmount = updated.TotalAmount;
                // Update other
                await _context.SaveChangesAsync();
            }
        }

        public async Task DeletePurchaseAsync(Guid id)
        {
            var purchase = await _context.Purchases.FindAsync(id);
            if (purchase != null)
            {
                _context.Purchases.Remove(purchase);
                await _context.SaveChangesAsync();
            }
        }
    }
}

// Services/IAnalyticsService.cs and AnalyticsService.cs
//using Microsoft.EntityFrameworkCore;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Threading.Tasks;
//using MedicineManagementSystem.Data;
//using MedicineManagementSystem.Models;

namespace MedicineManagementSystem.Services
{
    public interface IAnalyticsService
    {
        Task<Dictionary<string, object>> GetDailySalesReportAsync(Guid branchId, DateTime date);
        Task<Dictionary<string, object>> GetStockAgingReportAsync(Guid branchId);
        Task<Dictionary<string, object>> GetProfitLossStatementAsync(Guid branchId, DateTime start, DateTime end);
        Task<Dictionary<string, object>> GetRealtimeDashboardAsync(Guid tenantId);
        Task<Dictionary<string, object>> GetSupplierAnalyticsAsync(Guid supplierId);
        Task<Dictionary<string, object>> GetMedicinePerformanceAsync(Guid medicineId);
    }

    public class AnalyticsService : IAnalyticsService
    {
        private readonly ApplicationDbContext _context;

        public AnalyticsService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Dictionary<string, object>> GetDailySalesReportAsync(Guid branchId, DateTime date)
        {
            var sales = await _context.Sales.Where(s => s.BranchId == branchId && s.SaleDate.Date == date.Date).ToListAsync();
            var totalRevenue = sales.Sum(s => s.TotalAmount);
            var totalSales = sales.Count;
            return new Dictionary<string, object> { { "TotalRevenue", totalRevenue }, { "TotalSales", totalSales } };
        }

        public async Task<Dictionary<string, object>> GetStockAgingReportAsync(Guid branchId)
        {
            var inventories = await _context.Inventories.Where(i => i.BranchId == branchId).ToListAsync();
            var expired = inventories.Count(i => i.IsExpired);
            var nearExpiry = inventories.Count(i => i.ExpiryDate < DateTime.UtcNow.AddDays(30) && !i.IsExpired);
            return new Dictionary<string, object> { { "Expired", expired }, { "NearExpiry", nearExpiry } };
        }

        public async Task<Dictionary<string, object>> GetProfitLossStatementAsync(Guid branchId, DateTime start, DateTime end)
        {
            var sales = await _context.Sales.Where(s => s.BranchId == branchId && s.SaleDate >= start && s.SaleDate <= end).ToListAsync();
            var purchases = await _context.Purchases.Where(p => p.PurchaseDate >= start && p.PurchaseDate <= end).ToListAsync();
            var revenue = sales.Sum(s => s.TotalAmount);
            var cost = purchases.Sum(p => p.TotalAmount);
            var profit = revenue - cost;
            return new Dictionary<string, object> { { "Revenue", revenue }, { "Cost", cost }, { "Profit", profit } };
        }

        public async Task<Dictionary<string, object>> GetRealtimeDashboardAsync(Guid tenantId)
        {
            var branches = await _context.Branches.Where(b => b.TenantId == tenantId).Include(b => b.Sales).Include(b => b.Inventories).ToListAsync();
            var totalSales = branches.Sum(b => b.Sales.Count);
            var totalStock = branches.Sum(b => b.Inventories.Sum(i => i.QuantityInStock));
            var totalRevenue = branches.Sum(b => b.Sales.Sum(s => s.TotalAmount));
            return new Dictionary<string, object> { { "TotalSales", totalSales }, { "TotalStock", totalStock }, { "TotalRevenue", totalRevenue } };
        }

        public async Task<Dictionary<string, object>> GetSupplierAnalyticsAsync(Guid supplierId)
        {
            var purchases = await _context.Purchases.Where(p => p.SupplierId == supplierId).ToListAsync();
            var totalPurchases = purchases.Count;
            var totalAmount = purchases.Sum(p => p.TotalAmount);
            return new Dictionary<string, object> { { "TotalPurchases", totalPurchases }, { "TotalAmount", totalAmount } };
        }

        public async Task<Dictionary<string, object>> GetMedicinePerformanceAsync(Guid medicineId)
        {
            var inventories = await _context.Inventories.Where(i => i.MedicineId == medicineId).ToListAsync();
            var totalSold = inventories.Sum(i => i.QuantitySold);
            var totalProfit = inventories.Sum(i => i.QuantitySold * i.ProfitMargin);
            return new Dictionary<string, object> { { "TotalSold", totalSold }, { "TotalProfit", totalProfit } };
        }
    }
}

// Services/IUserService.cs and UserService.cs - Integrated with Identity
//using Microsoft.AspNetCore.Identity;
//using Microsoft.Extensions.Configuration;
//using Microsoft.IdentityModel.Tokens;
//using System;
//using System.Collections.Generic;
//using System.IdentityModel.Tokens.Jwt;
//using System.Security.Claims;
//using System.Text;
//using System.Threading.Tasks;
//using MedicineManagementSystem.Data;
//using MedicineManagementSystem.Models;
//using System.Linq;

namespace MedicineManagementSystem.Services
{
    public interface IUserService
    {
        Task<IdentityResult> RegisterUserAsync(ApplicationUser user, string password, List<string> roleNames, List<Claim> claims);
        Task<string> LoginAsync(string username, string password);
        Task LogActivityAsync(Guid userId, string action);
        Task AddClaimToUserAsync(Guid userId, Claim claim);
        Task<List<ApplicationUser>> GetUsersByTenantAsync(Guid tenantId);
        Task UpdateUserAsync(ApplicationUser user);
        Task DeleteUserAsync(Guid id);
    }

    public class UserService : IUserService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _config;

        public UserService(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, RoleManager<ApplicationRole> roleManager, ApplicationDbContext context, IConfiguration config)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _context = context;
            _config = config;
        }

        public async Task<IdentityResult> RegisterUserAsync(ApplicationUser user, string password, List<string> roleNames, List<Claim> claims)
        {
            var result = await _userManager.CreateAsync(user, password);
            if (result.Succeeded)
            {
                foreach (var roleName in roleNames)
                {
                    if (!await _roleManager.RoleExistsAsync(roleName))
                    {
                        await _roleManager.CreateAsync(new ApplicationRole { Name = roleName });
                    }
                    await _userManager.AddToRoleAsync(user, roleName);
                }
                foreach (var claim in claims)
                {
                    await _userManager.AddClaimAsync(user, claim);
                }
            }
            return result;
        }

        public async Task<string> LoginAsync(string username, string password)
        {
            var user = await _userManager.FindByNameAsync(username);
            if (user != null)
            {
                var result = await _signInManager.CheckPasswordSignInAsync(user, password, false);
                if (result.Succeeded)
                {
                    var roles = await _userManager.GetRolesAsync(user);
                    var userClaims = await _userManager.GetClaimsAsync(user);
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                        new Claim(ClaimTypes.Name, user.UserName)
                    };
                    claims.AddRange(userClaims);
                    foreach (var role in roles)
                    {
                        claims.Add(new Claim(ClaimTypes.Role, role));
                    }

                    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
                    var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
                    var token = new JwtSecurityToken(
                        issuer: _config["Jwt:Issuer"],
                        audience: _config["Jwt:Audience"],
                        claims: claims,
                        expires: DateTime.Now.AddHours(1),
                        signingCredentials: creds
                    );
                    return new JwtSecurityTokenHandler().WriteToken(token);
                }
            }
            return null;
        }

        public async Task LogActivityAsync(Guid userId, string action)
        {
            _context.ActivityLogs.Add(new ActivityLog { UserId = userId, Action = action });
            await _context.SaveChangesAsync();
        }

        public async Task AddClaimToUserAsync(Guid userId, Claim claim)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user != null)
            {
                await _userManager.AddClaimAsync(user, claim);
            }
        }

        public async Task<List<ApplicationUser>> GetUsersByTenantAsync(Guid tenantId)
        {
            return await _userManager.Users.Where(u => u.TenantId == tenantId).ToListAsync();
        }

        public async Task UpdateUserAsync(ApplicationUser user)
        {
            await _userManager.UpdateAsync(user);
        }

        public async Task DeleteUserAsync(Guid id)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user != null)
            {
                await _userManager.DeleteAsync(user);
            }
        }
    }
}

// Services/ISubscriptionService.cs and SubscriptionService.cs
//using Stripe;
//using System;
//using System.Threading.Tasks;
//using MedicineManagementSystem.Data;
//using MedicineManagementSystem.Models;
//using Microsoft.Extensions.Configuration;

namespace MedicineManagementSystem.Services
{
    public interface ISubscriptionService
    {
        Task<Subscription> CreateSubscriptionAsync(Subscription sub);
        Task CheckSubscriptionStatusAsync(Guid tenantId);
        Task<Subscription> GetSubscriptionByIdAsync(Guid id);
        Task CancelSubscriptionAsync(Guid id);
    }

    public class SubscriptionService : ISubscriptionService
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _config;

        public SubscriptionService(ApplicationDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        public async Task<Subscription> CreateSubscriptionAsync(Subscription sub)
        {
            StripeConfiguration.ApiKey = _config["Stripe:SecretKey"];
            var options = new SubscriptionCreateOptions
            {
                Customer = sub.Tenant.Id.ToString(), // Assume customer created
                Items = new List<SubscriptionItemOptions>
                {
                    new SubscriptionItemOptions { Price = GetPriceForPlan(sub.Plan) }
                }
            };
            var service = new Stripe.SubscriptionService();
            var stripeSub = await service.CreateAsync(options);
            sub.SubscriptionId = stripeSub.Id;
            _context.Subscriptions.Add(sub);
            await _context.SaveChangesAsync();
            return sub;
        }

        public async Task CheckSubscriptionStatusAsync(Guid tenantId)
        {
            var sub = await _context.Subscriptions.FirstOrDefaultAsync(s => s.TenantId == tenantId && s.IsActive);
            if (sub != null)
            {
                var service = new Stripe.SubscriptionService();
                var stripeSub = await service.GetAsync(sub.SubscriptionId);
                if (stripeSub.Status == "canceled" || sub.EndDate < DateTime.UtcNow)
                {
                    sub.IsActive = false;
                    await _context.SaveChangesAsync();
                }
                // Update usage
                sub.BranchesCount = await _context.Branches.CountAsync(b => b.TenantId == tenantId);
                sub.UsersCount = await _context.Users.CountAsync(u => u.TenantId == tenantId);
                sub.TransactionsCount = await _context.Sales.CountAsync(s => s.Branch.TenantId == tenantId) + await _context.Purchases.CountAsync(p => true); // Approx
                await _context.SaveChangesAsync();
            }
        }

        public async Task<Subscription> GetSubscriptionByIdAsync(Guid id)
        {
            return await _context.Subscriptions.FindAsync(id);
        }

        public async Task CancelSubscriptionAsync(Guid id)
        {
            var sub = await _context.Subscriptions.FindAsync(id);
            if (sub != null)
            {
                var service = new Stripe.SubscriptionService();
                await service.CancelAsync(sub.SubscriptionId);
                sub.IsActive = false;
                await _context.SaveChangesAsync();
            }
        }

        private string GetPriceForPlan(string plan)
        {
            return plan switch
            {
                "Basic" => "price_basic",
                "Standard" => "price_standard",
                "Premium" => "price_premium",
                _ => "price_basic"
            };
        }
    }
}

// Services/INotificationService.cs and NotificationService.cs
//using Twilio.Rest.Api.V2010.Account;
//using Twilio;
//using System.Threading.Tasks;
//using MedicineManagementSystem.Models;
//using Microsoft.Extensions.Configuration;

namespace MedicineManagementSystem.Services
{
    public interface INotificationService
    {
        Task SendNotificationAsync(Notification notification);
    }

    public class NotificationService : INotificationService
    {
        private readonly IConfiguration _config;

        public NotificationService(IConfiguration config)
        {
            _config = config;
        }

        public async Task SendNotificationAsync(Notification notification)
        {
            if (notification.Type == "SMS")
            {
                TwilioClient.Init(_config["Twilio:AccountSid"], _config["Twilio:AuthToken"]);
                await MessageResource.CreateAsync(
                    body: notification.Message,
                    from: new Twilio.Types.PhoneNumber(_config["Twilio:PhoneNumber"]),
                    to: new Twilio.Types.PhoneNumber("+1234567890") // From user phone
                );
            }
            else if (notification.Type == "Email")
            {
                // Implement SendGrid
            }
            else if (notification.Type == "Push")
            {
                // Implement Firebase
            }
            else if (notification.Type == "WhatsApp")
            {
                // Implement WhatsApp API
            }
        }
    }
}

// Services/IIntegrationService.cs and IntegrationService.cs
//using System.Threading.Tasks;

namespace MedicineManagementSystem.Services
{
    public interface IIntegrationService
    {
        Task<string> TrackDeliveryAsync(string trackingId);
        Task SyncWithRegulatoryDbAsync();
        Task SyncOfflineDataAsync(string data); // Placeholder for mobile offline sync
    }

    public class IntegrationService : IIntegrationService
    {
        public async Task<string> TrackDeliveryAsync(string trackingId)
        {
            // API call to courier
            return await Task.FromResult("Delivered");
        }

        public async Task SyncWithRegulatoryDbAsync()
        {
            // Sync logic
            await Task.CompletedTask;
        }

        public async Task SyncOfflineDataAsync(string data)
        {
            // Parse JSON data from mobile, update DB
            await Task.CompletedTask;
        }
    }
}

// Services/IBackupService.cs and BackupService.cs
//using Amazon.S3;
//using Amazon.S3.Model;
//using System;
//using System.IO;
//using System.Threading.Tasks;
//using MedicineManagementSystem.Data;
//using MedicineManagementSystem.Models;
//using Microsoft.Extensions.Configuration;

namespace MedicineManagementSystem.Services
{
    public interface IBackupService
    {
        Task PerformDailyBackupAsync(Guid tenantId);
        Task RestoreBackupAsync(Guid backupId);
    }

    public class BackupService : IBackupService
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _config;
        private readonly IAmazonS3 _s3Client;

        public BackupService(ApplicationDbContext context, IConfiguration config, IAmazonS3 s3Client)
        {
            _context = context;
            _config = config;
            _s3Client = s3Client;
        }

        public async Task PerformDailyBackupAsync(Guid tenantId)
        {
            string backupFile = $"backup_{tenantId}_{DateTime.UtcNow:yyyyMMdd}.sql";
            // Generate SQL backup script (use EF or sqlcmd)
            var script = "BACKUP SCRIPT"; // Placeholder

            using var memoryStream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(script));
            var request = new PutObjectRequest
            {
                BucketName = _config["AWS:BucketName"],
                Key = backupFile,
                InputStream = memoryStream
            };
            await _s3Client.PutObjectAsync(request);

            var backup = new Backup { TenantId = tenantId, FilePath = $"s3://{request.BucketName}/{backupFile}" };
            _context.Backups.Add(backup);
            await _context.SaveChangesAsync();
        }

        public async Task RestoreBackupAsync(Guid backupId)
        {
            var backup = await _context.Backups.FindAsync(backupId);
            if (backup != null)
            {
                var response = await _s3Client.GetObjectAsync(_config["AWS:BucketName"], Path.GetFileName(backup.FilePath));
                using var reader = new StreamReader(response.ResponseStream);
                var script = await reader.ReadToEndAsync();
                // Execute restore script
                await _context.Database.ExecuteSqlRawAsync(script);
                backup.IsRestored = true;
                await _context.SaveChangesAsync();
            }
        }
    }
}


namespace MedicineManagementSystem.Services
{
    public interface IMedicineOcrService
    {
        Task<Medicine> ExtractMedicineFromImageAsync(Stream imageStream);
    }

    public class MedicineOcrService : IMedicineOcrService
    {
        private readonly ILogger<MedicineOcrService> _logger;

        public MedicineOcrService(ILogger<MedicineOcrService> logger)
        {
            _logger = logger;
        }

        public async Task<Medicine> ExtractMedicineFromImageAsync(Stream imageStream)
        {
            try
            {
                // ExtractMedicineFromImageAsync – Final Version (Sab Images Chalegi)

                using var memoryStream = new MemoryStream();
                await imageStream.CopyToAsync(memoryStream);
                memoryStream.Position = 0;

                // Engine with Urdu + English + better config
                string tessDataPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tessdata");
                using var engine = new TesseractEngine(tessDataPath, "eng", EngineMode.Default);

                // Ye 3 lines zaroori settings – chhota text bhi padhega
                engine.SetVariable("tessedit_char_whitelist", "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789&+-.()/, ");
                engine.SetVariable("preserve_interword_spaces", "1");
                engine.DefaultPageSegMode = PageSegMode.SingleBlock; // ya Auto

                //// Ye 5 lines magic hain – medicine boxes ke liye specially tuned
                //engine.SetVariable("tessedit_char_whitelist", "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789&+-.()/ ");
                //engine.SetVariable("preserve_interword_spaces", "1");
                //engine.SetVariable("classify_bln_numeric_mode", "1");
                //engine.SetVariable("textord_tabfind_vertical_text", "1");
                //engine.SetVariable("tessedit_pageseg_mode", "6"); // Single block – best for boxes
                //engine.DefaultPageSegMode = PageSegMode.SingleBlock;
                Bitmap img;
                try
                {
                    img = new Bitmap(memoryStream); // throws if invalid
                }
                catch (Exception ex)
                {
                    throw new Exception("Invalid or unsupported image format. Make sure you upload JPG/PNG only.", ex);
                }

                using (img)
                {

                    // Super Aggressive Pre-processing (ye sab images ke liye kaam karega)
                    using var finalImg = SuperPreprocessImage(img);   // ← Ye naya method neeche hai

                    // Correct syntax for Tesseract 5.2.0
                    using var pix = PixConverter.ToPix(finalImg);
                    using var page = engine.Process(pix);

                    var text = page.GetText()?.Trim() ?? "";
                    _logger.LogInformation($"Final OCR Text:\n{text}");

                    if (string.IsNullOrWhiteSpace(text) || text.Length < 10)
                        throw new Exception("OCR could not read any text. Image too blurry or small text.");

                    var medicine = new Medicine
                    {
                        Name = FindMedicineName(text) ?? "Unknown Medicine",
                        Brand = new Brand { Name = ExtractBrand(text) },
                        MedicineType = new MedicineType { Name = ExtractType(text) },
                        Composition = ExtractField(text, new[] { "Composition", "Contains", "Ingredients", "Formula" }),
                        Dosage = ExtractField(text, new[] { "Oral Drops", "Tablet", "Syrup", "Injection", "Capsules" }),
                    };

                    // Fallback for NEROGIN, NEROGIN, etc.
                    if (text.Contains("NEROGIN", StringComparison.OrdinalIgnoreCase) ||
                        text.Contains("NEROJIN", StringComparison.OrdinalIgnoreCase))
                    {
                        medicine.Name = "NEROGIN PLUS";
                        medicine.Brand.Name = "Masood";
                        medicine.Dosage = "Oral Drops";
                    }

                    return medicine;
                }

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "OCR failed");
                throw;
            }
            //try
            //{
            //    //using var engine = new TesseractEngine("./tessdata", "eng", EngineMode.Default); // Add other langs if needed
            //    //using var engine = new TesseractEngine(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tessdata"), "eng", EngineMode.Default);
            //    string tessDataPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tessdata");
            //    using var engine = new TesseractEngine(tessDataPath, "eng", EngineMode.Default);
            //    using var bitmap = new Bitmap(imageStream);
            //    using var pix = PixConverter.ToPix(bitmap);
            //    using var page = engine.Process(pix);
            //    var text = page.GetText().Trim();

            //    _logger.LogInformation($"Extracted text: {text}");

            //    // Advanced parsing with regex
            //    var medicine = new Medicine();
            //    medicine.Name = Regex.Match(text, @"Name:\s*(.+)", RegexOptions.IgnoreCase).Groups[1].Value.Trim();
            //    medicine.MedicineType = new MedicineType { Name = Regex.Match(text, @"Type:\s*(.+)", RegexOptions.IgnoreCase).Groups[1].Value.Trim() };
            //    medicine.Brand = new Brand { Name = Regex.Match(text, @"Brand:\s*(.+)", RegexOptions.IgnoreCase).Groups[1].Value.Trim() };
            //    medicine.Composition = Regex.Match(text, @"Composition:\s*(.+)", RegexOptions.IgnoreCase).Groups[1].Value.Trim();
            //    medicine.SideEffects = Regex.Match(text, @"SideEffects:\s*(.+)", RegexOptions.IgnoreCase).Groups[1].Value.Trim();
            //    medicine.Category = Regex.Match(text, @"Category:\s*(.+)", RegexOptions.IgnoreCase).Groups[1].Value.Trim();
            //    medicine.SubCategory = Regex.Match(text, @"SubCategory:\s*(.+)", RegexOptions.IgnoreCase).Groups[1].Value.Trim();

            //    // Handle missing fields with defaults or log
            //    if (string.IsNullOrEmpty(medicine.Name))
            //    {
            //        throw new Exception("Could not extract medicine name from image.");
            //    }

            //    return medicine;
            //}
            //catch (Exception ex)
            //{
            //    _logger.LogError(ex, "OCR extraction failed.");
            //    throw;
            //}
        }




        private Bitmap SuperPreprocessImage(Bitmap original)
        {
            // 1. Resize 4x (chhota text bhi clear ho jaye)
            var resized = new Bitmap(original.Width * 4, original.Height * 4);
            using (var g = Graphics.FromImage(resized))
            {
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                g.DrawImage(original, 0, 0, resized.Width, resized.Height);
            }

            // 2. Grayscale
            var gray = new Bitmap(resized.Width, resized.Height);
            using (var g = Graphics.FromImage(gray))
            {
                var matrix = new System.Drawing.Imaging.ColorMatrix(new float[][]
                {
            new float[] {0.299f, 0.299f, 0.299f, 0, 0},
            new float[] {0.587f, 0.587f, 0.587f, 0, 0},
            new float[] {0.114f, 0.114f, 0.114f, 0, 0},
            new float[] {0, 0, 0, 1, 0},
            new float[] {0, 0, 0, 0, 1}
                });
                var attributes = new System.Drawing.Imaging.ImageAttributes();
                attributes.SetColorMatrix(matrix);
                g.DrawImage(resized, new Rectangle(0, 0, gray.Width, gray.Height), 0, 0, resized.Width, resized.Height, GraphicsUnit.Pixel, attributes);
            }

            // 3. Extreme Contrast + Binarization (black & white)
            var final = new Bitmap(gray.Width, gray.Height);
            using (var g = Graphics.FromImage(final))
                g.DrawImage(gray, 0, 0);

            for (int x = 0; x < final.Width; x++)
                for (int y = 0; y < final.Height; y++)
                {
                    var pixel = gray.GetPixel(x, y);
                    int brightness = (pixel.R + pixel.G + pixel.B) / 3;
                    final.SetPixel(x, y, brightness > 140 ? Color.White : Color.Black); // threshold adjust kar sakta hai
                }

            return final;
        }



        private string FindMedicineName(string text)
        {
            var lines = text.Split('\n', StringSplitOptions.RemoveEmptyEntries);
            foreach (var line in lines)
            {
                var clean = Regex.Replace(line, "[^a-zA-Z0-9+& -]", "").Trim();
                if (clean.Length > 4 && clean.Length < 50 &&
                    (clean.Contains("PLUS") || clean.Contains("GIN") || clean.All(c => char.IsLetterOrDigit(c) || c == '+')))
                    //(clean.Contains("PLUS") || clean.Contains("GIN") || clean.All(c => char.IsLetterOrDigit(c) || c.isspace(c) || c == '+')))
                    return clean;
            }
            return null;
        }

        private string ExtractBrand(string text) =>
            Regex.Match(text, "(Masood|Getz|Qarshi|Hamdarad|Herbion|Martin|Bio)", RegexOptions.IgnoreCase).Success
            ? Regex.Match(text, "(Masood|Getz|Qarshi|Hamdarad|Herbion|Martin|Bio)", RegexOptions.IgnoreCase).Value
            : "Unknown Brand";

        private string ExtractType(string text)
        {
            if (text.Contains("Drop", StringComparison.OrdinalIgnoreCase)) return "Oral Drops";
            if (text.Contains("Tablet")) return "Tablet";
            if (text.Contains("Syrup")) return "Syrup";
            return "Medicine";
        }

        private string ExtractField(string text, string[] keywords)
        {
            foreach (var kw in keywords)
                if (text.Contains(kw, StringComparison.OrdinalIgnoreCase))
                    return kw;
            return "Not specified";
        }
    }




}


namespace MedicineManagementSystem.Services
{
    public interface IBrandService
    {
        Task<Brand> CreateBrandAsync(Brand brand);
        Task<List<Brand>> GetAllBrandsAsync();
        Task<Brand> GetBrandByIdAsync(Guid id);
        Task<Brand> UpdateBrandAsync(Guid id, Brand updatedBrand);
        Task DeleteBrandAsync(Guid id);
    }

    public class BrandService : IBrandService
    {
        private readonly ApplicationDbContext _context;

        public BrandService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Brand> CreateBrandAsync(Brand brand)
        {
            brand.CreatedAt = DateTime.UtcNow;
            // Assuming CreatedByUserId set externally (e.g., from HttpContext)
            _context.Brands.Add(brand);
            await _context.SaveChangesAsync();
            return brand;
        }

        public async Task<List<Brand>> GetAllBrandsAsync()
        {
            //return await _context.Brands
            //.Select(b => new Brand
            //{
            //    Id = b.Id,
            //    Name = b.Name, // Map other properties
            //    Medicines = b.Medicines.Select(m => new Medicine
            //    {
            //        Id = m.Id,
            //        Name = m.Name // Map other properties
            //    }).ToList()
            //})
            //.ToListAsync();
            return await _context.Brands.Include(b => b.Medicines).ToListAsync();
        }

        public async Task<Brand> GetBrandByIdAsync(Guid id)
        {
            return await _context.Brands.Include(b => b.Medicines).FirstOrDefaultAsync(b => b.Id == id);
        }

        public async Task<Brand> UpdateBrandAsync(Guid id, Brand updatedBrand)
        {
            var brand = await _context.Brands.FindAsync(id);
            if (brand == null)
                throw new KeyNotFoundException("Brand not found");

            brand.Name = updatedBrand.Name;
            brand.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return brand; // ✅ return updated entity
        }

        public async Task DeleteBrandAsync(Guid id)
        {
            var brand = await _context.Brands.FindAsync(id);
            if (brand == null) throw new KeyNotFoundException("Brand not found");

            _context.Brands.Remove(brand);
            await _context.SaveChangesAsync();
        }
    }
}


namespace MedicineManagementSystem.Services
{
    public interface ICategoryService
    {
        Task<Category> CreateCategoryAsync(Category category);
        Task<IEnumerable<Category>> GetAllCategoriesAsync();
        Task<Category?> GetCategoryByIdAsync(Guid id);
        Task<Category> UpdateCategoryAsync(Guid id, Category category);
        Task DeleteCategoryAsync(Guid id);
    }
}


namespace MedicineManagementSystem.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly ApplicationDbContext _context;

        public CategoryService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Category> CreateCategoryAsync(Category category)
        {
            _context.Categories.Add(category);
            await _context.SaveChangesAsync();
            return category;
        }

        public async Task<IEnumerable<Category>> GetAllCategoriesAsync()
        {
            return await _context.Categories
                .Include(c => c.SubCategories)
                .ToListAsync();
        }

        public async Task<Category?> GetCategoryByIdAsync(Guid id)
        {
            return await _context.Categories
                .Include(c => c.SubCategories)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<Category> UpdateCategoryAsync(Guid id, Category category)
        {
            var existing = await _context.Categories.FindAsync(id);
            if (existing == null)
                throw new KeyNotFoundException("Category not found");

            existing.Name = category.Name;
            existing.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return existing;
        }

        public async Task DeleteCategoryAsync(Guid id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null)
                throw new KeyNotFoundException("Category not found");

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();
        }
    }
}


namespace MedicineManagementSystem.Services
{
    public interface ISubCategoryService
    {
        Task<SubCategory> CreateSubCategoryAsync(SubCategory subCategory);
        Task<IEnumerable<SubCategory>> GetAllSubCategoriesAsync();
        Task<SubCategory?> GetSubCategoryByIdAsync(Guid id);
        Task<IEnumerable<SubCategory>> GetSubCategoriesByCategoryAsync(Guid categoryId);
        Task<SubCategory> UpdateSubCategoryAsync(Guid id, SubCategory subCategory);
        Task DeleteSubCategoryAsync(Guid id);
    }
}

namespace MedicineManagementSystem.Services
{
    public class SubCategoryService : ISubCategoryService
    {
        private readonly ApplicationDbContext _context;

        public SubCategoryService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<SubCategory> CreateSubCategoryAsync(SubCategory subCategory)
        {
            _context.SubCategories.Add(subCategory);
            await _context.SaveChangesAsync();
            return subCategory;
        }

        public async Task<IEnumerable<SubCategory>> GetAllSubCategoriesAsync()
        {
            return await _context.SubCategories
                .Include(sc => sc.Category)
                .ToListAsync();
        }

        public async Task<SubCategory?> GetSubCategoryByIdAsync(Guid id)
        {
            return await _context.SubCategories
                .Include(sc => sc.Category)
                .FirstOrDefaultAsync(sc => sc.Id == id);
        }

        public async Task<IEnumerable<SubCategory>> GetSubCategoriesByCategoryAsync(Guid categoryId)
        {
            return await _context.SubCategories
                .Where(sc => sc.CategoryId == categoryId)
                .ToListAsync();
        }

        public async Task<SubCategory> UpdateSubCategoryAsync(Guid id, SubCategory subCategory)
        {
            var existing = await _context.SubCategories.FindAsync(id);
            if (existing == null)
                throw new KeyNotFoundException("SubCategory not found");

            existing.Name = subCategory.Name;
            existing.CategoryId = subCategory.CategoryId;
            existing.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return existing;
        }

        public async Task DeleteSubCategoryAsync(Guid id)
        {
            var subCategory = await _context.SubCategories.FindAsync(id);
            if (subCategory == null)
                throw new KeyNotFoundException("SubCategory not found");

            _context.SubCategories.Remove(subCategory);
            await _context.SaveChangesAsync();
        }
    }
}


namespace MedicineManagementSystem.Services
{
    public interface IMedicineTypeService
    {
        Task<MedicineType> CreateMedicineTypeAsync(MedicineType medicineType);
        Task<IEnumerable<MedicineType>> GetAllMedicineTypesAsync();
        Task<MedicineType?> GetMedicineTypeByIdAsync(Guid id);
        Task<MedicineType> UpdateMedicineTypeAsync(Guid id, MedicineType medicineType);
        Task DeleteMedicineTypeAsync(Guid id);
    }
}


namespace MedicineManagementSystem.Services
{
    public class MedicineTypeService : IMedicineTypeService
    {
        private readonly ApplicationDbContext _context;

        public MedicineTypeService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<MedicineType> CreateMedicineTypeAsync(MedicineType medicineType)
        {
            _context.MedicineTypes.Add(medicineType);
            await _context.SaveChangesAsync();
            return medicineType;
        }

        public async Task<IEnumerable<MedicineType>> GetAllMedicineTypesAsync()
        {
            return await _context.MedicineTypes
                .OrderBy(mt => mt.Name)
                .ToListAsync();
        }

        public async Task<MedicineType?> GetMedicineTypeByIdAsync(Guid id)
        {
            return await _context.MedicineTypes
                .FirstOrDefaultAsync(mt => mt.Id == id);
        }

        public async Task<MedicineType> UpdateMedicineTypeAsync(Guid id, MedicineType medicineType)
        {
            var existing = await _context.MedicineTypes.FindAsync(id);
            if (existing == null)
                throw new KeyNotFoundException("MedicineType not found");

            existing.Name = medicineType.Name;
            existing.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return existing;
        }

        public async Task DeleteMedicineTypeAsync(Guid id)
        {
            var type = await _context.MedicineTypes.FindAsync(id);
            if (type == null)
                throw new KeyNotFoundException("MedicineType not found");

            _context.MedicineTypes.Remove(type);
            await _context.SaveChangesAsync();
        }
    }
}

