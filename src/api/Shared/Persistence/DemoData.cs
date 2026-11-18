using Api.Modules.Buildings; 

using Api.Modules.Identity; 

using Api.Shared.Auth; 

using Microsoft.EntityFrameworkCore; 

 

namespace Api.Shared.Persistence; 

 

public static class DemoData 

{ 

    public static async Task SeedAsync(IServiceProvider services) 

    { 

        using var scope = services.CreateScope(); 

        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>(); 

        var config = scope.ServiceProvider.GetRequiredService<IConfiguration>(); 

 

        await db.Database.MigrateAsync();                 // apply pending migrations 

        if (await db.Buildings.AnyAsync()) return;        // seed only once 

 

        var domain = config["Seed:EmailDomain"] 

            ?? throw new InvalidOperationException("Set Seed:EmailDomain in appsettings.json"); 

 

        var floor4 = new Floor { Level = 4, Label = "Floor 4" }; 

        foreach (var n in new[] { "4A", "4B", "4C", "4D", "4E", "4F", "4G", "4H" }) 

            floor4.Units.Add(new Unit { Number = n, Type = UnitType.Apartment, AreaM2 = 64.50m, Rooms = 3 }); 

 

        var aurora = new Building { Name = "Aurora Residence", Address = "12 Example Street" }; 

        aurora.Floors.Add(floor4); 

 

        var borealis = new Building { Name = "Borealis Court", Address = "40 Sample Avenue" }; 

        borealis.Floors.Add(new Floor { Level = 1, Label = "Floor 1", 

            Units = { new Unit { Number = "1A", Type = UnitType.Apartment, AreaM2 = 48.00m, Rooms = 2 } } }); 

 

        db.Buildings.AddRange(aurora, borealis);          // EF assigns the Ids here 

        var unit4B = floor4.Units.Single(u => u.Number == "4B"); 

 

        UserAccount Account(string first, params RoleAssignment[] roles) => new() 

        { 

            Email = $"{first.ToLowerInvariant()}@{domain}", 

            Person = new Person { FullName = first }, 

            RoleAssignments = [.. roles] 

        }; 

 

        db.UserAccounts.AddRange( 

            Account("Radu",  new RoleAssignment { Role = Roles.Administrator }), 

            Account("Mihai", new RoleAssignment { Role = Roles.BuildingManager, BuildingId = aurora.Id }, 

                             new RoleAssignment { Role = Roles.BuildingManager, BuildingId = borealis.Id }), 

            Account("Ioana", new RoleAssignment { Role = Roles.Technician, BuildingId = aurora.Id }), 

            Account("Ana",   new RoleAssignment { Role = Roles.Resident, BuildingId = aurora.Id, UnitId = unit4B.Id })); 

 

        await db.SaveChangesAsync();                      // one transaction for everything 

    } 

} 