using SMNETCORE.DAL.BaseDAL.Models;
using SMNETCORE.DAL.BaseDAL.Repositories.Tenant;
using SMNETCORE.Logging;
using SMNETCORE.TenantDAL.Interface;
using System;
using System.Collections.Generic;
using System.Text;

namespace SMNETCORE.DAL.TenantDAL
{
    internal class TenantDAL : TenantRepository<TenantDTOModel>, ITenantDAL
    {
        public TenantDAL(TenantDTOModel tenant) : base(tenant)
        {
        }

        public TenantDTOModel? CreateTenant(TenantDTOModel tenant)
        {
            try
            {
                using (var context = NewInternalContext())
                {
                    context.Update(tenant, false, false);
                    context.SaveChangeWithValidation();
                    return tenant;
                }
            }
            catch (Exception exc)
            {
                Logger.LogError(exc, LogCategoryType.Common);
                throw exc;
            }
        }

        public TenantDTOModel? GetTenant(long tenantId)
        {
            try
            {
                using (var context = NewInternalContext())
                {
                    return context.Tenants.FirstOrDefault(d => d.Id == tenantId && d.IsActive);
                }
            }
            catch (Exception exc)
            {
                Logger.LogError(exc, LogCategoryType.Common);
                throw exc;
            }
        }

        public TenantDTOModel? GetTenant(string tenantName)
        {
            try
            {
                using (var context = NewInternalContext())
                {
                    return context.Tenants.FirstOrDefault(d => d.Name == tenantName && d.IsActive);
                }
            }
            catch (Exception exc)
            {
                Logger.LogError(exc, LogCategoryType.Common);
                throw exc;
            }
        }
    }
}
