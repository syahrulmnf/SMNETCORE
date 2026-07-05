using Azure;
using Microsoft.AspNetCore.Http;
using SMNETCORE.Common.Enums;
using SMNETCORE.Common.Models;
using SMNETCORE.DataType.Extensions;
using System;
using System.Collections.Generic;
using System.Text;

namespace BaseAPI.Utils
{
    public class AuthMiddleware
    {
        private readonly RequestDelegate _next;

        public AuthMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(
           HttpContext context,
           AuthContextDTOModel requestContext)
        {
            var tenantToken = await AuthJWT.Invoke(context);

            if (tenantToken.HasStringValue())
            {
                requestContext.BearerToken = tenantToken;
            }
            context.Items["AuthContext"] = requestContext;
            if (context.User.Identity?.IsAuthenticated == true)
            {
                requestContext.sub = context.User.Claims.FirstOrDefault(c => c.Type == JWTClaimType.Sub)?.Value;
                requestContext.tenant = context.User.Claims.FirstOrDefault(c => c.Type == JWTClaimType.Tenant)?.Value;
                requestContext.role = context.User.Claims.FirstOrDefault(c => c.Type == JWTClaimType.Role)?.Value;
                requestContext.username = context.User.Claims.FirstOrDefault(c => c.Type == JWTClaimType.UserName)?.Value;
                requestContext.email = context.User.Claims.FirstOrDefault(c => c.Type == JWTClaimType.Email)?.Value;
                requestContext.FirstName = context.User.Claims.FirstOrDefault(c => c.Type == JWTClaimType.FirstName)?.Value;
                requestContext.LastName = context.User.Claims.FirstOrDefault(c => c.Type == JWTClaimType.LastName)?.Value.NullableToString();
                requestContext.TimeZone = context.User.Claims.FirstOrDefault(c => c.Type == JWTClaimType.TimeZone)?.Value.NullableToString();
                requestContext.TimeZoneName = context.User.Claims.FirstOrDefault(c => c.Type == JWTClaimType.TimeZoneName)?.Value;
                requestContext.AddressId = context.User.Claims.FirstOrDefault(c => c.Type == JWTClaimType.AddressId)?.Value.NullableToString();
                requestContext.Currency = context.User.Claims.FirstOrDefault(c => c.Type == JWTClaimType.Currency)?.Value;
                requestContext.IsAdmin = requestContext.role != null && requestContext.role.Equals("admin", StringComparison.OrdinalIgnoreCase);
            }
            await _next(context);
        }
    }
}
