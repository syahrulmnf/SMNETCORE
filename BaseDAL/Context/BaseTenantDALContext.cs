using Microsoft.EntityFrameworkCore;
using SMNETCORE.DAL.BaseDAL.Common;
using SMNETCORE.DAL.BaseDAL.Models;
using System.Configuration;

namespace SMNETCORE.DAL.BaseDAL.Context
{
    public partial class TenantDALContext : BaseDALContext
    {
        public string DBContextName { get => $"{Tenant.Name}_DBContext"; }
        public TenantDTOModel Tenant { get; set; } = new TenantDTOModel();
        public DbSet<TenantDTOModel> Tenants { get; set; }

        internal TenantDALContext() { }
        

        public TenantDALContext(TenantDTOModel _tenant, DBProvider provider):
            this(DALGlobals.DBOptionsConfig<TenantDALContext>($"{_tenant.Name}_DBContext", provider), provider  )
        { 
            this.Tenant = _tenant;
            this.Provider = provider;
        }

        internal TenantDALContext(DbContextOptions<TenantDALContext> options, DBProvider provider)
            : base(options, provider)
        {
            this.Provider = provider;
        }

   
     

    
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {


            //OnModelCreatingPartial(modelBuilder);
        }

    }
}
