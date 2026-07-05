using Azure;
using BaseAPI.Utils;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using SMNETCORE.BaseAPI.Utils;
using SMNETCORE.Logging;
using JWTConfigs = SMNETCORE.JWTAuthentication.Configs;
using WebConfigs = SMNETCORE.BaseAPI.Configs;
using WebUtils = SMNETCORE.BaseAPI.Utils.Utils;

var builder = WebApplication.CreateBuilder(args);

WebUtils.LoadAllSettings(builder);
// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
WebConfigs.JWT.ConfigureJWT(builder.Services
    .AddAuthentication(
        JwtBearerDefaults.AuthenticationScheme), builder.Configuration);
builder.Services.AddAuthorization();

JWTConfigs.GoogleConfig.Config(builder);
WebUtils.AddDALImplementations(builder.Services);
WebUtils.AddServicesImplementations(builder.Services);
JWTConfigs.Tenant.Injections(builder);


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}
app.UseAuthentication();
app.UseMiddleware<AuthMiddleware>();
app.UseMiddleware<TenantMiddleware>();
app.UseAuthorization();
app.MapControllers();

app.Run();
