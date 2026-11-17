namespace Api.Modules.Buildings; 

 

public enum UnitType { Apartment, Commercial, Storage, Parking } 

 

public sealed class Building 

{ 

    public Guid Id { get; set; } 

    public required string Name { get; set; } 

    public required string Address { get; set; } 

    public bool IsActive { get; set; } = true;          // archive, never delete (PX-06) 

    public List<Floor> Floors { get; set; } = []; 

} 

 

public sealed class Floor 

{ 

    public Guid Id { get; set; } 

    public Guid BuildingId { get; set; }                // foreign key to Building 

    public int Level { get; set; } 

    public required string Label { get; set; } 

    public List<Unit> Units { get; set; } = []; 

} 

 

public sealed class Unit 

{ 

    public Guid Id { get; set; } 

    public Guid FloorId { get; set; }                   // foreign key to Floor 

    public required string Number { get; set; }         // for example "4B" 

    public UnitType Type { get; set; } 

    public decimal AreaM2 { get; set; }                 // decimal, never double 

    public int Rooms { get; set; } 

} 