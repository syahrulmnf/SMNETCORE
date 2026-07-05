using SMNETCORE.DAL.TenantDAL.Interface.Models;

namespace JJWTService.Services.Interface
{
    public interface IJwtService
    {
        string GenerateToken(TenantUserDTOModel userTenant, TenantRoleDTOModel roles);
    }
}
