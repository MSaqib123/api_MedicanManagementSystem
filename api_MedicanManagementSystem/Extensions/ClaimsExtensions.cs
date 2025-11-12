// Extensions/ClaimsExtensions.cs - Extension methods
using System.Security.Claims;
using System.Linq;

namespace MedicineManagementSystem.Extensions
{
    public static class ClaimsExtensions
    {
        public static string GetTenantId(this ClaimsPrincipal principal)
        {
            return principal.Claims.FirstOrDefault(c => c.Type == "TenantId")?.Value;
        }

        public static bool HasBranchAccess(this ClaimsPrincipal principal, string level)
        {
            return principal.HasClaim("BranchAccess", level);
        }
        // More extensions
    }
}