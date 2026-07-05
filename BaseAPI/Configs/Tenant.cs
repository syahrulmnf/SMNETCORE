using Microsoft.EntityFrameworkCore;
using SMNETCORE.DAL.BaseDAL.Context;
using SMNETCORE.DAL.BaseDAL.Models;
using SMNETCORE.DAL.BaseDAL.Repositories;
using SMNETCORE.DAL.BaseDAL.Repositories.Tenant;
using SMNETCORE.BaseAPI.Utils;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;

namespace SMNETCORE.BaseAPI.Configs
{
    public class Tenant
    {
        public static void BaseApiInjections(WebApplicationBuilder builder)
        {
            
            builder.Services.AddDbContext<MainCatalogueDALContext>(options =>
            {
                options.UseSqlServer(
                    builder.Configuration.GetConnectionString("MainCatalogueContext"));
            });
            builder.Services.AddScoped<TenantDTOModel>();
        }
    }
}
