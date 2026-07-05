using Azure;
using Microsoft.EntityFrameworkCore;
using SMNETCORE.Common.Models;
using SMNETCORE.DAL.BaseDAL.Context;
using SMNETCORE.DAL.BaseDAL.Models;
using SMNETCORE.DAL.BaseDAL.Repositories;
using SMNETCORE.DAL.BaseDAL.Repositories.Tenant;

namespace SMNETCORE.JWTAuthentication.Configs
{
    public class Tenant
    {
        public static void Injections(WebApplicationBuilder builder)
        {
            
            builder.Services.AddDbContext<MainCatalogueDALContext>(options =>
            {
                options.UseSqlServer(
                    builder.Configuration.GetConnectionString("MainCatalogueContext"));
            });
            builder.Services.AddScoped<TenantDTOModel>();
            builder.Services.AddScoped<AuthContextDTOModel>();
        }
    }
}
