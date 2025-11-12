// Models/IAuditable.cs - For audit trail
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;
using System;
using MedicineManagementSystem.Models;

namespace MedicineManagementSystem.Models
{
    public interface IAuditable
    {
        DateTime CreatedAt { get; set; }
        Guid CreatedByUserId { get; set; }
        DateTime? UpdatedAt { get; set; }
        Guid? UpdatedByUserId { get; set; }
    }
}



namespace MedicineManagementSystem.Models
{
    public class ApplicationUser : IdentityUser<Guid>, IAuditable
    {
        public Guid TenantId { get; set; }
        public Tenant Tenant { get; set; }
        public Guid? BranchId { get; set; }
        public Branch Branch { get; set; }
        public bool TwoFactorEnabled { get; set; }
        public ICollection<ActivityLog> ActivityLogs { get; set; } = new List<ActivityLog>();
        public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
        public DateTime CreatedAt { get; set; }
        public Guid CreatedByUserId { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public Guid? UpdatedByUserId { get; set; }
    }
}

namespace MedicineManagementSystem.Models
{
    public class ApplicationRole : IdentityRole<Guid>
    {
        // Additional properties if needed
    }
}

namespace MedicineManagementSystem.Models
{
    public class Tenant : IAuditable
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; }
        public string Subdomain { get; set; }
        public string LogoUrl { get; set; }
        public string Currency { get; set; } = "PKR";
        public string Language { get; set; } = "en";
        public decimal TaxRate { get; set; } = 0.17m;
        public ICollection<Branch> Branches { get; set; } = new List<Branch>();
        public ICollection<Subscription> Subscriptions { get; set; } = new List<Subscription>();
        public ICollection<ApplicationUser> Users { get; set; } = new List<ApplicationUser>();
        public ICollection<Backup> Backups { get; set; } = new List<Backup>();
        public DateTime CreatedAt { get; set; }
        public Guid CreatedByUserId { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public Guid? UpdatedByUserId { get; set; }
    }
}


namespace MedicineManagementSystem.Models
{
    public class Branch : IAuditable
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; }
        public Guid TenantId { get; set; }
        public Tenant Tenant { get; set; }
        public ICollection<Inventory> Inventories { get; set; } = new List<Inventory>();
        public ICollection<Sale> Sales { get; set; } = new List<Sale>();
        public ICollection<ApplicationUser> Users { get; set; } = new List<ApplicationUser>();
        public DateTime CreatedAt { get; set; }
        public Guid CreatedByUserId { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public Guid? UpdatedByUserId { get; set; }
    }
}

// Models/MedicineType.cs
namespace MedicineManagementSystem.Models
{
    public class MedicineType : IAuditable
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; } // Tablet, Capsule, Liquid, Drops, Injection, Ointment, Syrup, Powder, Inhaler, Patch, Suppository, Cream, Gel, Spray, etc.
        public ICollection<Medicine> Medicines { get; set; } = new List<Medicine>();
        public DateTime CreatedAt { get; set; }
        public Guid CreatedByUserId { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public Guid? UpdatedByUserId { get; set; }
    }
}

// Models/Brand.cs
namespace MedicineManagementSystem.Models
{
    public class Brand : IAuditable
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; } // Palebrook, Karshii, BM, Masood, Pfizer, GSK, Abbott, Novartis, etc.
        public ICollection<Medicine> Medicines { get; set; } = new List<Medicine>();
        public DateTime CreatedAt { get; set; }
        public Guid CreatedByUserId { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public Guid? UpdatedByUserId { get; set; }
    }
}

namespace MedicineManagementSystem.Models
{
    public class Medicine : IAuditable
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; }
        public Guid MedicineTypeId { get; set; }
        public MedicineType MedicineType { get; set; }
        public Guid BrandId { get; set; }
        public Brand Brand { get; set; }
        public string Composition { get; set; }
        public string Dosage { get; set; }
        public string SideEffects { get; set; }
        public string Category { get; set; }
        public string SubCategory { get; set; }
        public ICollection<Inventory> Inventories { get; set; } = new List<Inventory>();
        public string Barcode { get; set; }
        public DateTime CreatedAt { get; set; }
        public Guid CreatedByUserId { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public Guid? UpdatedByUserId { get; set; }
    }
}

namespace MedicineManagementSystem.Models
{
    public class Inventory : IAuditable
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid MedicineId { get; set; }
        public Medicine Medicine { get; set; }
        public Guid BranchId { get; set; }
        public Branch Branch { get; set; }
        public string BatchNumber { get; set; }
        public DateTime ExpiryDate { get; set; }
        public int QuantityInStock { get; set; }
        public int QuantitySold { get; set; }
        public int QuantityOut { get; set; }
        public decimal PurchasePrice { get; set; }
        public decimal RetailPrice { get; set; }
        public decimal ProfitMargin { get; set; }
        public int MinStockLevel { get; set; } = 10;
        public bool IsExpired => ExpiryDate < DateTime.UtcNow;
        public string StockHandlingMethod { get; set; } = "FIFO";
        public ICollection<StockTransfer> Transfers { get; set; } = new List<StockTransfer>();
        public DateTime CreatedAt { get; set; }
        public Guid CreatedByUserId { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public Guid? UpdatedByUserId { get; set; }
    }
}


namespace MedicineManagementSystem.Models
{
    public class StockTransfer : IAuditable
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid InventoryId { get; set; }
        public Inventory Inventory { get; set; }
        public Guid FromBranchId { get; set; }
        public Branch FromBranch { get; set; }
        public Guid ToBranchId { get; set; }
        public Branch ToBranch { get; set; }
        public int Quantity { get; set; }
        public DateTime TransferDate { get; set; } = DateTime.UtcNow;
        public Guid TransferredByUserId { get; set; }
        public DateTime CreatedAt { get; set; }
        public Guid CreatedByUserId { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public Guid? UpdatedByUserId { get; set; }
    }
}

namespace MedicineManagementSystem.Models
{
    public class Sale : IAuditable
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid BranchId { get; set; }
        public Branch Branch { get; set; }
        public DateTime SaleDate { get; set; } = DateTime.UtcNow;
        public decimal TotalAmount { get; set; }
        public decimal Discount { get; set; }
        public decimal Tax { get; set; }
        public string PaymentMode { get; set; }
        public string PrescriptionUrl { get; set; }
        public int LoyaltyPoints { get; set; }
        public string InvoiceBarcode { get; set; }
        public bool IsReturned { get; set; }
        public decimal RefundAmount { get; set; }
        public ICollection<SaleItem> Items { get; set; } = new List<SaleItem>();
        public string AccountingEntryId { get; set; }
        public DateTime CreatedAt { get; set; }
        public Guid CreatedByUserId { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public Guid? UpdatedByUserId { get; set; }
    }

    public class SaleItem : IAuditable
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid SaleId { get; set; }
        public Sale Sale { get; set; }
        public Guid InventoryId { get; set; }
        public Inventory Inventory { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public DateTime CreatedAt { get; set; }
        public Guid CreatedByUserId { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public Guid? UpdatedByUserId { get; set; }
    }
}


namespace MedicineManagementSystem.Models
{
    public class Purchase : IAuditable
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid SupplierId { get; set; }
        public Supplier Supplier { get; set; }
        public DateTime PurchaseDate { get; set; } = DateTime.UtcNow;
        public string PurchaseOrderNumber { get; set; }
        public string GoodsReceivedNote { get; set; }
        public bool IsReturned { get; set; }
        public decimal TotalAmount { get; set; }
        public DateTime DueDate { get; set; }
        public ICollection<PurchaseItem> Items { get; set; } = new List<PurchaseItem>();
        public DateTime CreatedAt { get; set; }
        public Guid CreatedByUserId { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public Guid? UpdatedByUserId { get; set; }
    }

    public class PurchaseItem : IAuditable
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid PurchaseId { get; set; }
        public Purchase Purchase { get; set; }
        public Guid MedicineId { get; set; }
        public Medicine Medicine { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public DateTime CreatedAt { get; set; }
        public Guid CreatedByUserId { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public Guid? UpdatedByUserId { get; set; }
    }
}

namespace MedicineManagementSystem.Models
{
    public class Supplier : IAuditable
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; }
        public string Ledger { get; set; }
        public decimal CreditLimit { get; set; }
        public ICollection<Purchase> Purchases { get; set; } = new List<Purchase>();
        public DateTime CreatedAt { get; set; }
        public Guid CreatedByUserId { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public Guid? UpdatedByUserId { get; set; }
    }
}

namespace MedicineManagementSystem.Models
{
    public class ActivityLog
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid UserId { get; set; }
        public ApplicationUser User { get; set; }
        public string Action { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}

//namespace MedicineManagementSystem.Models
//{
    public class Subscription : IAuditable
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid TenantId { get; set; }
        public Tenant Tenant { get; set; }
        public string Plan { get; set; }
        public DateTime StartDate { get; set; } = DateTime.UtcNow;
        public DateTime EndDate { get; set; }
        public bool IsActive { get; set; } = true;
        public string PaymentProvider { get; set; }
        public string SubscriptionId { get; set; }
        public int BranchesCount { get; set; }
        public int UsersCount { get; set; }
        public int TransactionsCount { get; set; }
        public DateTime CreatedAt { get; set; }
        public Guid CreatedByUserId { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public Guid? UpdatedByUserId { get; set; }
    }
//}

namespace MedicineManagementSystem.Models
{
    public class Notification : IAuditable
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Type { get; set; }
        public string Message { get; set; }
        public DateTime SentAt { get; set; } = DateTime.UtcNow;
        public Guid UserId { get; set; }
        public ApplicationUser User { get; set; }
        public DateTime CreatedAt { get; set; }
        public Guid CreatedByUserId { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public Guid? UpdatedByUserId { get; set; }
    }
}


namespace MedicineManagementSystem.Models
{
    public class Backup : IAuditable
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid TenantId { get; set; }
        public Tenant Tenant { get; set; }
        public DateTime BackupDate { get; set; } = DateTime.UtcNow;
        public string FilePath { get; set; }
        public bool IsRestored { get; set; }
        public DateTime CreatedAt { get; set; }
        public Guid CreatedByUserId { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public Guid? UpdatedByUserId { get; set; }
    }
}