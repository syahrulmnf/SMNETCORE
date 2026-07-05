using SMNETCORE.Cache;
using SMNETCORE.Cache.Redis;
using SMNETCORE.Common.Models;
using SMNETCORE.DAL.BaseDAL.Models;
using SMNETCORE.DAL.BaseDAL.Repositories;
using SMNETCORE.DAL.MainCatalogue.Interface;
using SMNETCORE.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace SMNETCORE.MainCatalogue.DAL
{
    public class MainCatalogueDAL : MainCatalogueRepository<TenantDTOModel>, IMainCatalogueDAL
    {
        public MainCatalogueDAL() { }
        public MainCatalogueDAL(TenantDTOModel tenant, AuthContextDTOModel authModel) 
        {
            this.Tenant = tenant;
            this.AuthContext = authModel;
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
                var key = HashKeyGenerator.Get(tenantName, ClassName, "Tenants",
                    true, this.AuthContext.BearerToken,
                    Cache.Enums.CacheCollectionType.Tenants);
                var tenant = RedisManager.Instance.GetData<TenantDTOModel>(key);
                using (var context = NewInternalContext())
                {
                    tenant = context.Tenants.FirstOrDefault(d => d.Name == tenantName && d.IsActive);
                }
                if(tenant != null)
                {
                    RedisManager.Instance.SetData(key, tenant);
                }
                return tenant;
            }
            catch (Exception exc)
            {
                Logger.LogError(exc, LogCategoryType.Common);
                throw exc;
            }
        }

        public const string ClassName = "MainCatalogueDAL";
    }
}
