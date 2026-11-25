namespace Api.Shared.Auth; 

 

public static class Roles 

{ 

    public const string Administrator = "Administrator"; 

    public const string BuildingManager = "BuildingManager"; 

    public const string Resident = "Resident"; 

    public const string Technician = "Technician"; 

} 

 

public static class AppClaims 

{ 

    public const string UserAccountId = "app_user_id";   // our database account id 

    public const string BuildingScope = "app_building";  // "Role:buildingId" 

} 

 

public static class Policies 

{ 

    public const string Administrator = "Administrator"; 

}