using Microsoft.AspNetCore.Authentication; 

using Microsoft.AspNetCore.Authorization; 

using Microsoft.Identity.Web; 

 

namespace Api.Shared.Auth; 

 

public static class AuthSetup 

{ 

    public static IServiceCollection AddBuildingOpsAuth(this IServiceCollection services, IConfiguration config) 

    { 

        // Validates Entra v2 tokens: signature, issuer (your tenant), audience (your API), expiry 

        services.AddMicrosoftIdentityWebApiAuthentication(config, "AzureAd"); 

        services.AddScoped<IClaimsTransformation, AppClaimsTransformation>(); 

 

        var signedInAppUser = new AuthorizationPolicyBuilder() 

            .RequireAuthenticatedUser()              // valid Entra token 

            .RequireScope("access_as_user")          // token was issued for OUR API scope 

            .RequireClaim(AppClaims.UserAccountId)   // has an active account in OUR database 

            .Build(); 

 

        services.AddAuthorizationBuilder() 

            .SetDefaultPolicy(signedInAppUser) 

            .SetFallbackPolicy(signedInAppUser)      // deny by default (SEC-03) 

            .AddPolicy(Policies.Administrator, p => p.RequireRole(Roles.Administrator)); 

 

        return services; 

    } 

}