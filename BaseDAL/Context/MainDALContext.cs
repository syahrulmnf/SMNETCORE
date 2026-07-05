using Microsoft.EntityFrameworkCore;
using SMNETCORE.DAL.BaseDAL.Common;
using SMNETCORE.DAL.BaseDAL.Models;
using System.Configuration;

namespace SMNETCORE.DAL.BaseDAL.Context
{
    public partial class MainCatalogueDALContext : BaseDALContext
    {
        internal MainCatalogueDALContext() : this(DALGlobals.DBOptionsConfig<MainCatalogueDALContext>(null), DBProvider.SQLServer){ }
        internal MainCatalogueDALContext(DBProvider provider) : this(DALGlobals.DBOptionsConfig<MainCatalogueDALContext>(null, provider), provider) { }

        internal MainCatalogueDALContext(DbContextOptions<MainCatalogueDALContext> options, DBProvider provider)
            : base(options, provider)
        {
        }

       


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {


            //OnModelCreatingPartial(modelBuilder);
        }
        public DbSet<TenantDTOModel> Tenants {  get; set; }
    }
}
