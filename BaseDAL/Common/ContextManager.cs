using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using SMNETCORE.DAL.BaseDAL.Context;
using SMNETCORE.DAL.BaseDAL.Repositories.Tenant;
using System;
using System.Collections.Generic;
using System.Text;

namespace SMNETCORE.DAL.BaseDAL.Common
{
    internal class TenantContextManager
    {
        private static Lazy<Dictionary<string, TenantDALContext>> _tenantContexts = new Lazy<Dictionary<string, TenantDALContext>>(() => new Dictionary<string, TenantDALContext>(), isThreadSafe: true);

        internal static TenantDALContext GetContextForTenant(string tenantName, DBProvider provider)
        {
            if (!_tenantContexts.Value.ContainsKey(tenantName))
            {
                using (var mainContext = new MainCatalogueDALContext(DALGlobals.DBOptionsConfig<MainCatalogueDALContext>(null, provider), provider))
                {
                    var tenant = mainContext.Tenants.FirstOrDefault(t => t.Name == tenantName);
                    if (tenant != null)
                    {
                        _tenantContexts.Value[tenantName] = new TenantDALContext(tenant, provider);
                    }
                }
            }

            var context = _tenantContexts.Value[tenantName];
            if (!context.IsOpen) // Assuming IsOpen is a property that checks if the context is still valid
            {
                _tenantContexts.Value.Remove(tenantName);
                return GetContextForTenant(tenantName, provider); // Try again to get a new context
            }
            return context;
        }
    }

    internal class MainCatalogueContextManager
    {
        private static Lazy<MainCatalogueDALContext> _mainContexts = new Lazy<MainCatalogueDALContext>(() => new MainCatalogueDALContext(), isThreadSafe: true);

        internal static MainCatalogueDALContext Context
        {
            get
            {
                var context = _mainContexts.Value;
                if (!context.IsOpen) // Assuming IsOpen is a property that checks if the context is still valid
                {
                    _mainContexts = new Lazy<MainCatalogueDALContext>(() => new MainCatalogueDALContext(), isThreadSafe: true);
                    context = _mainContexts.Value;
                }
                return context;
            }

        }

        internal static MainCatalogueDALContext NewContext()
        {
            return new MainCatalogueDALContext();
        }
    }
}



