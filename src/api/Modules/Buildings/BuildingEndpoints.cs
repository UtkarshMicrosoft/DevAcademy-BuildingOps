using System.Security.Claims; 

using Api.Shared.Auth; 

using Api.Shared.Persistence; 

using Microsoft.AspNetCore.Http.HttpResults; 

using Microsoft.EntityFrameworkCore; 

 

namespace Api.Modules.Buildings; 

 

public static class BuildingEndpoints 

{ 

    public static RouteGroupBuilder MapBuildingEndpoints(this RouteGroupBuilder api) 

    { 

        var g = api.MapGroup("/buildings").WithTags("Buildings"); 

        g.MapGet("/", ListBuildings); 

        g.MapGet("/{id:guid}", GetBuilding); 

        g.MapPost("/", CreateBuilding).RequireAuthorization(Policies.Administrator); 

        g.MapPost("/{id:guid}/floors", CreateFloor); 

        g.MapPost("/{id:guid}/floors/{floorId:guid}/units", CreateUnit); 

        return api; 

    } 

 

    // GET /api/v1/buildings: each role sees only its own buildings 

    static async Task<Ok<List<BuildingSummary>>> ListBuildings( 

        AppDbContext db, ClaimsPrincipal user, CancellationToken ct) 

    { 

        var query = db.Buildings.AsNoTracking().Where(b => b.IsActive); 

        if (!user.IsAdmin()) 

        { 

            var ids = user.VisibleBuildingIds(); 

            query = query.Where(b => ids.Contains(b.Id)); 

        } 

        var list = await query 

            .OrderBy(b => b.Name) 

            .Select(b => new BuildingSummary(b.Id, b.Name, b.Address, b.Floors.Count)) 

            .ToListAsync(ct); 

        return TypedResults.Ok(list); 

    } 

 

    // GET /api/v1/buildings/{id}: object-level check 

    static async Task<Results<Ok<BuildingDetail>, NotFound, ForbidHttpResult>> GetBuilding( 

        Guid id, AppDbContext db, ClaimsPrincipal user, CancellationToken ct) 

    { 

        var b = await db.Buildings.AsNoTracking() 

            .Include(x => x.Floors.OrderBy(f => f.Level)) 

            .ThenInclude(f => f.Units.OrderBy(u => u.Number)) 

            .SingleOrDefaultAsync(x => x.Id == id, ct); 

 

        if (b is null) return TypedResults.NotFound(); 

        if (!user.CanSeeBuilding(id)) return TypedResults.Forbid(); 

 

        return TypedResults.Ok(new BuildingDetail(b.Id, b.Name, b.Address, 

            b.Floors.Select(f => new FloorDto(f.Id, f.Level, f.Label, 

                f.Units.Select(u => new UnitDto(u.Id, u.Number, u.Type, u.AreaM2, u.Rooms)).ToList())) 

            .ToList())); 

    } 

 

    // POST /api/v1/buildings: Administrator only 

    static async Task<Created<BuildingSummary>> CreateBuilding( 

        CreateBuildingRequest req, AppDbContext db, CancellationToken ct) 

    { 

        var b = new Building { Name = req.Name.Trim(), Address = req.Address.Trim() }; 

        db.Buildings.Add(b); 

        await db.SaveChangesAsync(ct); 

        return TypedResults.Created($"/api/v1/buildings/{b.Id}", new BuildingSummary(b.Id, b.Name, b.Address, 0)); 

    } 

 

    // POST /api/v1/buildings/{id}/floors: Admin, or the Manager of THIS building 

    static async Task<Results<Created<FloorDto>, NotFound, ForbidHttpResult, ProblemHttpResult>> CreateFloor( 

        Guid id, CreateFloorRequest req, AppDbContext db, ClaimsPrincipal user, CancellationToken ct) 

    { 

        if (!await db.Buildings.AnyAsync(b => b.Id == id, ct)) return TypedResults.NotFound(); 

        if (!user.CanManageBuilding(id)) return TypedResults.Forbid(); 

        if (await db.Floors.AnyAsync(f => f.BuildingId == id && f.Level == req.Level, ct)) 

            return TypedResults.Problem(title: "Floor already exists", 

                detail: $"Level {req.Level} already exists in this building.", statusCode: 409); 

 

        var floor = new Floor { BuildingId = id, Level = req.Level, Label = req.Label.Trim() }; 

        db.Floors.Add(floor); 

        await db.SaveChangesAsync(ct); 

        return TypedResults.Created($"/api/v1/buildings/{id}", new FloorDto(floor.Id, floor.Level, floor.Label, [])); 

    } 

 

    // POST /api/v1/buildings/{id}/floors/{floorId}/units 

    static async Task<Results<Created<UnitDto>, NotFound, ForbidHttpResult, ProblemHttpResult>> CreateUnit( 

        Guid id, Guid floorId, CreateUnitRequest req, AppDbContext db, ClaimsPrincipal user, CancellationToken ct) 

    { 

        if (!await db.Floors.AnyAsync(f => f.Id == floorId && f.BuildingId == id, ct)) return TypedResults.NotFound(); 

        if (!user.CanManageBuilding(id)) return TypedResults.Forbid(); 

        var number = req.Number.Trim().ToUpperInvariant(); 

        if (await db.Units.AnyAsync(u => u.FloorId == floorId && u.Number == number, ct)) 

            return TypedResults.Problem(title: "Unit already exists", 

                detail: $"Unit {number} already exists on this floor.", statusCode: 409); 

 

        var unit = new Unit { FloorId = floorId, Number = number, Type = req.Type, AreaM2 = req.AreaM2, Rooms = req.Rooms }; 

        db.Units.Add(unit); 

        await db.SaveChangesAsync(ct); 

        return TypedResults.Created($"/api/v1/buildings/{id}", 

            new UnitDto(unit.Id, unit.Number, unit.Type, unit.AreaM2, unit.Rooms)); 

    } 

} 