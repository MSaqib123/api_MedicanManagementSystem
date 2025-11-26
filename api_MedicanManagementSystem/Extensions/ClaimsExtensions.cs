// Extensions/ClaimsExtensions.cs - Extension methods
using System.Security.Claims;
using System.Linq;

namespace MedicineManagementSystem.Extensions
{
    public static class ClaimsExtensions
    {
        public static string GetTenantId(this ClaimsPrincipal principal)
        {
            var claim = principal.Claims.FirstOrDefault(c => c.Type == "TenantId")?.Value;
            if(claim == null)
            {
                throw new System.Exception("TenantId claim not found");
            }
            return claim;
        }

        public static bool HasBranchAccess(this ClaimsPrincipal principal, string level)
        {
            return principal.HasClaim("BranchAccess", level);
        }
        // More extensions
    }
}