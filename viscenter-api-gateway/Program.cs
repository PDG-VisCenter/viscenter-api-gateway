using Keycloak.AuthServices.Authentication;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

// Keycloak

builder.Services.AddKeycloakWebApiAuthentication(configuration);

// Ocelot
builder.Configuration.AddJsonFile("ocelot.json", optional: false, reloadOnChange: true);

builder.Services.AddOcelot();

var app = builder.Build();

// Middleware
app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<FlattenRolesMiddleware>();
app.UseOcelot().Wait();


app.Run();
