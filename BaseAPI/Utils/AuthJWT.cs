using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace SMNETCORE.BaseAPI.Utils
{
    public class AuthJWT
    {
        public static async Task<string?> Invoke(HttpContext context)
        {
            var authHeader =
                context.Request.Headers["Authorization"]
                    .FirstOrDefault();

            string? token = null;

            if (!string.IsNullOrWhiteSpace(authHeader) &&
                authHeader.StartsWith("Bearer "))
            {
                token = authHeader.Substring("Bearer ".Length);
            }

            return token;
        }
    }
}
