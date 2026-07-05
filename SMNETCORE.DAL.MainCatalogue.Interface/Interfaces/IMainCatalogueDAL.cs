using SMNETCORE.DAL.BaseDAL.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace SMNETCORE.DAL.MainCatalogue.Interface
{
    public interface IMainCatalogueDAL
    {
        public TenantDTOModel? GetTenant(long tenantId);
        public TenantDTOModel? GetTenant(string tenantName);
        public TenantDTOModel? CreateTenant(TenantDTOModel tenant);
    }
}
