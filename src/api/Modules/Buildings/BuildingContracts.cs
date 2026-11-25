using System.ComponentModel.DataAnnotations; 

 

namespace Api.Modules.Buildings; 

 

// What clients SEND (validated automatically before your code runs) 

public sealed record CreateBuildingRequest 

{ 

    [Required, StringLength(100, MinimumLength = 2)] public string Name { get; init; } = ""; 

    [Required, StringLength(200)] public string Address { get; init; } = ""; 

} 

 

public sealed record CreateFloorRequest 

{ 

    [Range(-5, 200)] public int Level { get; init; } 

    [Required, StringLength(50)] public string Label { get; init; } = ""; 

} 

 

public sealed record CreateUnitRequest 

{ 

    [Required, StringLength(20)] public string Number { get; init; } = ""; 

    public UnitType Type { get; init; } 

    [Range(typeof(decimal), "1", "10000")] public decimal AreaM2 { get; init; } 

    [Range(0, 20)] public int Rooms { get; init; } 

} 

 

// What the API RETURNS 

public sealed record BuildingSummary(Guid Id, string Name, string Address, int FloorCount); 

public sealed record BuildingDetail(Guid Id, string Name, string Address, List<FloorDto> Floors); 

public sealed record FloorDto(Guid Id, int Level, string Label, List<UnitDto> Units); 

public sealed record UnitDto(Guid Id, string Number, UnitType Type, decimal AreaM2, int Rooms); 