using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

public class FlattenRolesMiddleware
{
    private readonly RequestDelegate _next;

    public FlattenRolesMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var userClaims = context.User.Claims.ToList();

        var resourceAccessClaim = userClaims.FirstOrDefault(c => c.Type == "resource_access");
        if (resourceAccessClaim != null)
        {
            var resourceAccess = JsonSerializer.Deserialize<Dictionary<string, dynamic>>(resourceAccessClaim.Value);
            if (resourceAccess != null && resourceAccess.TryGetValue("paws-claws-client", out var clientRolesObj))
            {
                if (clientRolesObj is JsonElement rolesElement &&
                    rolesElement.TryGetProperty("roles", out var roles))
                {
                    foreach (var role in roles.EnumerateArray())
                    {
                        var roleValue = role.GetString();
                        if (!string.IsNullOrEmpty(roleValue))
                        {
                            var claimsIdentity = context.User.Identity as ClaimsIdentity;
                            
                            if (roleValue == "client_role" || roleValue == "seller_role")
                            {
                                claimsIdentity?.AddClaim(new Claim(roleValue, "true"));
                            }
                        }
                    }
                }
            }
        }

        await _next(context);
    }
}
