using System.Security.Claims; 

 

namespace Api.Shared.Auth; 

 

public static class AccessRules 

{ 

    public static bool IsAdmin(this ClaimsPrincipal user) => user.IsInRole(Roles.Administrator); 

 

    public static Guid[] VisibleBuildingIds(this ClaimsPrincipal user) => 

        user.FindAll(AppClaims.BuildingScope) 

            .Select(c => Guid.Parse(c.Value.Split(':')[1])) 

            .Distinct() 

            .ToArray(); 

 

    public static bool CanSeeBuilding(this ClaimsPrincipal user, Guid buildingId) => 

        user.IsAdmin() || user.VisibleBuildingIds().Contains(buildingId); 

 

    public static bool CanManageBuilding(this ClaimsPrincipal user, Guid buildingId) => 

        user.IsAdmin() || user.HasClaim(AppClaims.BuildingScope, $"{Roles.BuildingManager}:{buildingId}"); 

} 