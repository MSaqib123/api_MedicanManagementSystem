// Data/ApplicationDbContext.cs - EF Core Context with Identity and multi-tenant
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using MedicineManagementSystem.Models;
using System.Threading.Tasks;
using System.Threading;
using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;

namespace MedicineManagementSystem.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>, IDataProtectionKeyContext
    {
        public string TenantId { get; set; }

        public DbSet<DataProtectionKey> DataProtectionKeys { get; set; }
        public DbSet<Tenant> Tenants { get; set; }
        public DbSet<Branch> Branches { get; set; }
        public DbSet<Medicine> Medicines { get; set; }
        public DbSet<MedicineType> MedicineTypes { get; set; }
        public DbSet<Brand> Brands { get; set; }
        public DbSet<Inventory> Inventories { get; set; }
        public DbSet<StockTransfer> StockTransfers { get; set; }
        public DbSet<Sale> Sales { get; set; }
        public DbSet<Purchase> Purchases { get; set; }
        public DbSet<Supplier> Suppliers { get; set; }
        public DbSet<Subscription> Subscriptions { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<Backup> Backups { get; set; }
        public DbSet<ActivityLog> ActivityLogs { get; set; }
        public DbSet<SaleItem> SaleItems { get; set; }
        public DbSet<PurchaseItem> PurchaseItems { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Multi-tenant schema for non-Identity tables
            modelBuilder.Entity<Tenant>().ToTable("Tenants");
            modelBuilder.Entity<Branch>().ToTable("Branches");
            // Etc. for others

            // Relationships
            // === SAFE: Single cascade path ===
            modelBuilder.Entity<ApplicationUser>()
                .HasOne(u => u.Tenant)
                .WithMany(t => t.Users)
                .HasForeignKey(u => u.TenantId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ApplicationUser>()
                .HasOne(u => u.Branch)
                .WithMany(b => b.Users)
                .HasForeignKey(u => u.BranchId)
                .OnDelete(DeleteBehavior.Restrict); // Prevent cascade from Branch → User

            modelBuilder.Entity<Branch>()
                .HasOne(b => b.Tenant)
                .WithMany(t => t.Branches)
                .HasForeignKey(b => b.TenantId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Medicine>()
                .HasOne(m => m.Brand)
                .WithMany(b => b.Medicines)
                .HasForeignKey(m => m.BrandId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Medicine>()
                .HasOne(m => m.MedicineType)
                .WithMany(mt => mt.Medicines)
                .HasForeignKey(m => m.MedicineTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Inventory>()
                .HasOne(i => i.Medicine)
                .WithMany(m => m.Inventories)
                .HasForeignKey(i => i.MedicineId)
                .OnDelete(DeleteBehavior.Cascade); // Safe: Medicine → Inventory

            modelBuilder.Entity<Inventory>()
                .HasOne(i => i.Branch)
                .WithMany(b => b.Inventories)
                .HasForeignKey(i => i.BranchId)
                .OnDelete(DeleteBehavior.Restrict); // Prevent cascade cycle

            // === STOCK TRANSFER: NO CASCADE ON EITHER ===
            modelBuilder.Entity<StockTransfer>()
                .HasOne(st => st.FromBranch)
                .WithMany(b => b.StockTransfersFrom)
                .HasForeignKey(st => st.FromBranchId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<StockTransfer>()
                .HasOne(st => st.ToBranch)
                .WithMany(b => b.StockTransfersTo)
                .HasForeignKey(st => st.ToBranchId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Sale>()
                .HasOne(s => s.Branch)
                .WithMany(b => b.Sales)
                .HasForeignKey(s => s.BranchId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Purchase>()
                .HasOne(p => p.Supplier)
                .WithMany(s => s.Purchases)
                .HasForeignKey(p => p.SupplierId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ActivityLog>()
                .HasOne(al => al.User)
                .WithMany(u => u.ActivityLogs)
                .HasForeignKey(al => al.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<SaleItem>()
                .HasOne(si => si.Sale)
                .WithMany(s => s.Items)
                .HasForeignKey(si => si.SaleId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<SaleItem>()
                .HasOne(si => si.Inventory)
                .WithMany()
                .HasForeignKey(si => si.InventoryId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<PurchaseItem>()
                .HasOne(pi => pi.Purchase)
                .WithMany(p => p.Items)
                .HasForeignKey(pi => pi.PurchaseId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<PurchaseItem>()
                .HasOne(pi => pi.Medicine)
                .WithMany()
                .HasForeignKey(pi => pi.MedicineId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Subscription>()
                .HasOne(sub => sub.Tenant)
                .WithMany(t => t.Subscriptions)
                .HasForeignKey(sub => sub.TenantId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Notification>()
                .HasOne(n => n.User)
                .WithMany()
                .HasForeignKey(n => n.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Backup>()
                .HasOne(b => b.Tenant)
                .WithMany()
                .HasForeignKey(b => b.TenantId)
                .OnDelete(DeleteBehavior.Restrict);

            // Global filter for multi-tenant (but since schema switch, not needed)
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            if (!string.IsNullOrEmpty(TenantId))
            {
                Database.ExecuteSqlRaw($"SET SCHEMA '{TenantId}'");
            }
            // Audit trail
            foreach (var entry in ChangeTracker.Entries<IAuditable>())
            {
                if (entry.State == EntityState.Added)
                {
                    entry.Entity.CreatedAt = DateTime.UtcNow;
                    entry.Entity.CreatedByUserId = GetCurrentUserId(); // From HttpContext or service
                }
                else if (entry.State == EntityState.Modified)
                {
                    entry.Entity.UpdatedAt = DateTime.UtcNow;
                    entry.Entity.UpdatedByUserId = GetCurrentUserId();
                }
            }
            return base.SaveChangesAsync(cancellationToken);
        }

        private Guid GetCurrentUserId()
        {
            // Implement via IHttpContextAccessor
            return Guid.Empty; // Placeholder
        }
    }
}