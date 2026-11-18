using System.Security.Claims; 

using Api.Shared.Persistence; 

using Microsoft.AspNetCore.Authentication; 

using Microsoft.EntityFrameworkCore; 

using Microsoft.Identity.Web; 

 

namespace Api.Shared.Auth; 

 

// Runs after the Entra token is validated, on every request. 

public sealed class AppClaimsTransformation(AppDbContext db) : IClaimsTransformation 

{ 

    public async Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal) 

    { 

        if (principal.Identity?.IsAuthenticated != true || 

            principal.HasClaim(c => c.Type == AppClaims.UserAccountId)) 

            return principal;                                   // not signed in, or already done 

 

        var objectId = principal.GetObjectId();                 // Entra's unique user id ("oid") 

        if (objectId is null) return principal; 

 

        // 1) Returning user: find by Entra object id 

        var account = await db.UserAccounts 

            .Include(a => a.RoleAssignments) 

            .SingleOrDefaultAsync(a => a.EntraObjectId == objectId); 

 

        // 2) First sign-in of an INVITED user: match by email once, then lock to the object id 

        if (account is null) 

        { 

            var email = (principal.FindFirst("preferred_username")?.Value 

                      ?? principal.FindFirst(ClaimTypes.Upn)?.Value 

                      ?? principal.FindFirst(ClaimTypes.Email)?.Value)?.ToLowerInvariant(); 

 

            if (email is not null) 

            { 

                account = await db.UserAccounts 

                    .Include(a => a.RoleAssignments) 

                    .SingleOrDefaultAsync(a => a.EntraObjectId == null && a.Email == email); 

                if (account is not null) 

                { 

                    account.EntraObjectId = objectId; 

                    await db.SaveChangesAsync(); 

                } 

            } 

        } 

 

        // 3) No account (not invited) or deactivated: add nothing, so the API returns 403 

        if (account is null || !account.IsActive) return principal; 

 

        var appIdentity = new ClaimsIdentity(); 

        appIdentity.AddClaim(new Claim(AppClaims.UserAccountId, account.Id.ToString())); 

        foreach (var ra in account.RoleAssignments) 

        { 

            appIdentity.AddClaim(new Claim(ClaimTypes.Role, ra.Role)); 

            if (ra.BuildingId is Guid buildingId) 

                appIdentity.AddClaim(new Claim(AppClaims.BuildingScope, $"{ra.Role}:{buildingId}")); 

        } 

 

        var result = principal.Clone(); 

        result.AddIdentity(appIdentity); 

        return result; 

    } 

}