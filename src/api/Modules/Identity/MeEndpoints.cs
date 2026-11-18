using System.Security.Claims; 

using Api.Shared.Auth; 

using Api.Shared.Persistence; 

using Microsoft.AspNetCore.Http.HttpResults; 

using Microsoft.EntityFrameworkCore; 

 

namespace Api.Modules.Identity; 

 

public sealed record RoleDto(string Role, Guid? BuildingId, Guid? UnitId); 

public sealed record MeResponse(Guid Id, string FullName, string Email, List<RoleDto> Roles); 

 

public static class MeEndpoints 

{ 

    public static RouteGroupBuilder MapMeEndpoints(this RouteGroupBuilder api) 

    { 

        api.MapGet("/me", GetMe).WithTags("Identity"); 

        return api; 

    } 

 

    static async Task<Results<Ok<MeResponse>, NotFound>> GetMe( 

        ClaimsPrincipal user, AppDbContext db, CancellationToken ct) 

    { 

        var id = Guid.Parse(user.FindFirstValue(AppClaims.UserAccountId)!); 

        var me = await db.UserAccounts.AsNoTracking() 

            .Where(a => a.Id == id) 

            .Select(a => new MeResponse(a.Id, a.Person.FullName, a.Email, 

                a.RoleAssignments.Select(r => new RoleDto(r.Role, r.BuildingId, r.UnitId)).ToList())) 

            .SingleOrDefaultAsync(ct); 

        return me is null ? TypedResults.NotFound() : TypedResults.Ok(me); 

    } 

}