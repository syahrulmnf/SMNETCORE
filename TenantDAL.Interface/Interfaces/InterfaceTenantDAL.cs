using SMNETCORE.DAL.BaseDAL.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace SMNETCORE.TenantDAL.Interface
{
    public interface ITenantDAL
    {
        public TenantDTOModel? GetTenant(long tenantId);
        public TenantDTOModel? GetTenant(string tenantName);
        public TenantDTOModel? CreateTenant(TenantDTOModel tenant);
    }
}
