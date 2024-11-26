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

        AddRoleClaim(context, userClaims, "paws-claws-client", "client_role");

        AddRoleClaim(context, userClaims, "paws-claws-client", "seller_role");

        var claimsIdentity = context.User.Identity as ClaimsIdentity;
        if (claimsIdentity != null &&
            (claimsIdentity.HasClaim(c => c.Type == "client_role" && c.Value == "true") ||
             claimsIdentity.HasClaim(c => c.Type == "seller_role" && c.Value == "true")))
        {
            claimsIdentity.AddClaim(new Claim("allowed_role", "true"));
        }

        await _next(context);
    }

    private void AddRoleClaim(HttpContext context, List<Claim> userClaims, string resourceKey, string roleKey)
    {
        var resourceAccessClaim = userClaims.FirstOrDefault(c => c.Type == "resource_access");
        if (resourceAccessClaim != null)
        {
            var resourceAccess = JsonSerializer.Deserialize<Dictionary<string, dynamic>>(resourceAccessClaim.Value);
            if (resourceAccess != null && resourceAccess.TryGetValue(resourceKey, out var clientRolesObj))
            {
                if (clientRolesObj is JsonElement rolesElement &&
                    rolesElement.TryGetProperty("roles", out var roles))
                {
                    foreach (var role in roles.EnumerateArray())
                    {
                        if (role.GetString() == roleKey)
                        {
                            var claimsIdentity = context.User.Identity as ClaimsIdentity;
                            claimsIdentity?.AddClaim(new Claim(roleKey, "true"));
                        }
                    }
                }
            }
        }
    }
}
