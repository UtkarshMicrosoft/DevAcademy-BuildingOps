using System.Text.Json.Serialization; 

using Api.Modules.Buildings; 

using Api.Modules.Identity; 

using Api.Shared.Auth; 

using Api.Shared.Persistence; 

using Microsoft.EntityFrameworkCore; 

 

var builder = WebApplication.CreateBuilder(args); 

 

// ---- 1. Register services (dependency injection container) ---- 

builder.Services.AddDbContext<AppDbContext>(o => 

    o.UseNpgsql(builder.Configuration.GetConnectionString("Default"))); 

builder.Services.AddBuildingOpsAuth(builder.Configuration); 

builder.Services.AddValidation();                       // runs [Required], [Range]... automatically 

builder.Services.AddProblemDetails();                   // standard error format 

builder.Services.AddOpenApi(); 

builder.Services.ConfigureHttpJsonOptions(o => 

    o.SerializerOptions.Converters.Add(new JsonStringEnumConverter())); // "Apartment" instead of 0 

 

var app = builder.Build(); 

 

// ---- 2. Middleware pipeline (order matters!) ---- 

app.UseExceptionHandler(); 

app.UseStatusCodePages(); 

if (app.Environment.IsDevelopment()) 

{ 

    app.MapOpenApi().AllowAnonymous();                  // /openapi/v1.json (development only) 

    await DemoData.SeedAsync(app.Services); 

} 

app.UseAuthentication();                                // who are you? 

app.UseAuthorization();                                 // may you do this? 

 

// ---- 3. Endpoints ---- 

app.MapGet("/health", () => Results.Ok(new { status = "ok" })).AllowAnonymous(); 

 

var api = app.MapGroup("/api/v1").RequireAuthorization(); 

api.MapMeEndpoints(); 

api.MapBuildingEndpoints(); 

 

app.Run(); 

 

public partial class Program { }           