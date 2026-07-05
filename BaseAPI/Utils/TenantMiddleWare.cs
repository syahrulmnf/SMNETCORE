using BaseAPI.Utils;
using Microsoft.AspNetCore.Http;
using SMNETCORE.DAL.BaseDAL.Common;
using SMNETCORE.DAL.BaseDAL.Context;
using SMNETCORE.DAL.BaseDAL.Models;
using SMNETCORE.MainCatalogue.DAL;

namespace SMNETCORE.BaseAPI.Utils
{
    public class TenantMiddleware
    {
        private readonly RequestDelegate _next;

        public TenantMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(
            HttpContext context,
            TenantDTOModel tenantContext)
        {
            var tenantToken = await AuthJWT.Invoke(context);
            var host = context.Request.Host.Host;
            string? urlTenantValue = context.Request.Query.ContainsKey("tenantName") ? context.Request.Query["tenantName"].ToString() : 
                (context.Request.Path.Value??string.Empty).Split('/')[0];
            // tenant1.myapp.com
            var parts = host.Split('.');

       

            var tenant = parts[0] == "www" || parts.Length < 3 ? urlTenantValue : parts[0];
            using(var catalogueDAL = new MainCatalogueDAL())
            {
                var tenantData = catalogueDAL.GetTenant(tenant);
                if (tenantData == null)
                {
                    context.Response.StatusCode = StatusCodes.Status404NotFound;
                    await context.Response.WriteAsync("Something wrong happens!");
                    return;
                }
                tenantContext = tenantData;
            }
            context.Items["Tenant"] = tenantContext;
            context.Items["tenantName"] = tenantContext.Name;
            context.Items["tenantId"] = tenantContext.Id;
            await _next(context);
        }
    }
}
