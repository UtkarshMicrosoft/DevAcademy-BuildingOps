namespace Api.Modules.Identity; 

 

public sealed class Person                      // a human; may have no account 

{ 

    public Guid Id { get; set; } 

    public required string FullName { get; set; } 

    public string? Phone { get; set; }          // ? = optional (nullable) 

} 

 

public sealed class UserAccount                 // the ability to sign in 

{ 

    public Guid Id { get; set; } 

    public Guid PersonId { get; set; } 

    public Person Person { get; set; } = null!; 

    public required string Email { get; set; }  // must match the Entra sign-in name 

    public string? EntraObjectId { get; set; }  // filled on FIRST sign-in 

    public bool IsActive { get; set; } = true;  // deactivate, never delete (M1-04) 

    public List<RoleAssignment> RoleAssignments { get; set; } = []; 

} 

 

public sealed class RoleAssignment              // what they may do, and WHERE 

{ 

    public Guid Id { get; set; } 

    public Guid UserAccountId { get; set; } 

    public required string Role { get; set; } 

    public Guid? BuildingId { get; set; }       // null only for Administrator 

    public Guid? UnitId { get; set; }           // set for Residents 

}