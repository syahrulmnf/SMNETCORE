using JJWTService.Services.Interface;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using SMNETCORE.Common.Enums;
using SMNETCORE.DAL.BaseDAL.Models;
using SMNETCORE.DAL.TenantDAL.Interface.Models;
using SMNETCORE.DataType.Extensions;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace SMNETCORE.Services.JWTService
{
    
    public class JwtService:IJwtService
    {
        private IConfiguration _config;
        private TenantDTOModel _tenant; 
        public JwtService(IConfiguration config, TenantDTOModel tenant)
        {
            _config = config;
            _tenant = tenant;
        }

        public string GenerateToken(TenantUserDTOModel userTenant, TenantRoleDTOModel roles)
        {
            var claims = new List<Claim>
        {
            new Claim(JWTClaimType.Sub, Guid.NewGuid().ToString()),
            new Claim(JWTClaimType.Tenant, _tenant.Id.NullableToString()),
            new Claim(JWTClaimType.Role, roles.NameList.JoinText()),
            new Claim(JWTClaimType.UserName, userTenant.Name),
            new Claim(JWTClaimType.Email, userTenant.Email),
            new Claim(JWTClaimType.FirstName, userTenant.FirstName),
            new Claim(JWTClaimType.LastName, userTenant.LastName),
            new Claim(JWTClaimType.TimeZone, userTenant.TimeZone.NullableToString()),
            new Claim(JWTClaimType.TimeZoneName, userTenant.TimeZoneName),
            new Claim(JWTClaimType.AddressId, userTenant.AddressId.NullableToString()),
            new Claim(JWTClaimType.Currency, userTenant.Currency)
        };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_tenant.Secrets));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _tenant.Issuer,
                audience: _tenant.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddHours(3),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
