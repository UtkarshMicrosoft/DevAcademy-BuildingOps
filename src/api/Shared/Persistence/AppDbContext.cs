using Api.Modules.Buildings; 

using Api.Modules.Identity; 

using Microsoft.EntityFrameworkCore; 

 

namespace Api.Shared.Persistence; 

 

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options) 

{ 

    public DbSet<Building> Buildings => Set<Building>(); 

    public DbSet<Floor> Floors => Set<Floor>(); 

    public DbSet<Unit> Units => Set<Unit>(); 

    public DbSet<Person> Persons => Set<Person>(); 

    public DbSet<UserAccount> UserAccounts => Set<UserAccount>(); 

    public DbSet<RoleAssignment> RoleAssignments => Set<RoleAssignment>(); 

 

    protected override void OnModelCreating(ModelBuilder b) 

    { 

        b.Entity<Building>(e => 

        { 

            e.Property(x => x.Name).HasMaxLength(100); 

            e.Property(x => x.Address).HasMaxLength(200); 

            e.HasIndex(x => x.Name).IsUnique(); 

        }); 

        b.Entity<Floor>(e => 

        { 

            e.Property(x => x.Label).HasMaxLength(50); 

            e.HasIndex(x => new { x.BuildingId, x.Level }).IsUnique();   // no duplicate floor 4 

        }); 

        b.Entity<Unit>(e => 

        { 

            e.Property(x => x.Number).HasMaxLength(20); 

            e.Property(x => x.Type).HasConversion<string>().HasMaxLength(20); // store "Apartment", not 0 

            e.Property(x => x.AreaM2).HasPrecision(8, 2); 

            e.HasIndex(x => new { x.FloorId, x.Number }).IsUnique(); 

        }); 

        b.Entity<Person>(e => e.Property(x => x.FullName).HasMaxLength(150)); 

        b.Entity<UserAccount>(e => 

        { 

            e.Property(x => x.Email).HasMaxLength(254); 

            e.HasIndex(x => x.Email).IsUnique(); 

            e.HasIndex(x => x.EntraObjectId).IsUnique(); 

            e.HasOne(x => x.Person).WithOne().HasForeignKey<UserAccount>(x => x.PersonId); 

        }); 

        b.Entity<RoleAssignment>(e => 

        { 

            e.Property(x => x.Role).HasMaxLength(30); 

            e.HasOne<Building>().WithMany().HasForeignKey(x => x.BuildingId); 

            e.HasOne<Unit>().WithMany().HasForeignKey(x => x.UnitId); 

        }); 

    } 

} 