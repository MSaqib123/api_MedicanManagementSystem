using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using MedicineManagementSystem.Data;
using MedicineManagementSystem.Services;
using Microsoft.Extensions.DependencyInjection;

namespace MedicineManagementSystem.Middlewares
{
    public class TenantMiddleware
    {
        private readonly RequestDelegate _next;

        public TenantMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var tenantService = context.RequestServices.GetRequiredService<ITenantService>();
            var dbContext = context.RequestServices.GetRequiredService<ApplicationDbContext>();

            var host = context.Request.Host.Host;
            var subdomain = host.Split('.')[0];
            var tenant = await tenantService.GetTenantBySubdomainAsync(subdomain);

            if (tenant != null)
            {
                dbContext.TenantId = tenant.Id.ToString();
                context.Items["Tenant"] = tenant;
            }
            else
            {
                context.Response.StatusCode = 404;
                await context.Response.WriteAsync("Tenant not found");
                return;
            }

            await _next(context);
        }
    }
}