// BackgroundServices/ExpiryAlertBackgroundService.cs - For scheduled alerts
using MedicineManagementSystem.Data;
using MedicineManagementSystem.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MedicineManagementSystem.BackgroundServices
{
    public class ExpiryAlertBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;

        public ExpiryAlertBackgroundService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using var scope = _serviceProvider.CreateScope();
                var medicineService = scope.ServiceProvider.GetRequiredService<IMedicineService>();
                await medicineService.SendExpiryAlertsAsync();
                await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
            }
        }
    }
}

// Similarly for LowStockAlertBackgroundService and DuePaymentAlertBackgroundService
// LowStockAlertBackgroundService.cs
//using Microsoft.Extensions.Hosting;
//using System;
//using System.Threading;
//using System.Threading.Tasks;
//using MedicineManagementSystem.Services;
//using Microsoft.EntityFrameworkCore;
//using MedicineManagementSystem.Data;

namespace MedicineManagementSystem.BackgroundServices
{
    public class LowStockAlertBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;

        public LowStockAlertBackgroundService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using var scope = _serviceProvider.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();
                var branches = await context.Branches.ToListAsync();
                foreach (var branch in branches)
                {
                    var lowStock = await context.Inventories.Where(i => i.BranchId == branch.Id && i.QuantityInStock < i.MinStockLevel).ToListAsync();
                    foreach (var item in lowStock)
                    {
                        await notificationService.SendNotificationAsync(new Models.Notification { Message = $"Low stock in {branch.Name} for item {item.Id}" });
                    }
                }
                await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
            }
        }
    }
}

// DuePaymentAlertBackgroundService.cs
//using Microsoft.Extensions.Hosting;
//using System;
//using System.Threading;
//using System.Threading.Tasks;
//using MedicineManagementSystem.Services;

namespace MedicineManagementSystem.BackgroundServices
{
    public class DuePaymentAlertBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;

        public DuePaymentAlertBackgroundService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using var scope = _serviceProvider.CreateScope();
                var purchaseService = scope.ServiceProvider.GetRequiredService<IPurchaseService>();
                await purchaseService.SendDueAlertsAsync();
                await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
            }
        }
    }
}