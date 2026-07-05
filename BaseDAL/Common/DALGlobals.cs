using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
using SMNETCORE.DAL.BaseDAL.Context;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace SMNETCORE.DAL.BaseDAL.Common
{
    public enum DBProvider
    {
        SQLServer,
        PostgreSQL,
        MySQL,
    }
    public class DALGlobals
    {
        
        public static DbContextOptions<T> DBOptionsConfig<T>(string? contextName = null, DBProvider provider = DBProvider.SQLServer) where T : DbContext
        {
            switch (provider)
            { 
                case DBProvider.SQLServer:
                    return new DbContextOptionsBuilder<T>()
                        .UseSqlServer(ConfigurationManager.ConnectionStrings[contextName ?? DALGlobals.MainDBContextName].ConnectionString)
                        .Options;
                case DBProvider.PostgreSQL:
                    return new DbContextOptionsBuilder<T>()
                        .UseNpgsql(ConfigurationManager.ConnectionStrings[contextName ?? DALGlobals.MainDBContextName].ConnectionString)
                        .Options;
                case DBProvider.MySQL:
                    return new DbContextOptionsBuilder<T>()
                        .UseMySQL(ConfigurationManager.ConnectionStrings[contextName ?? DALGlobals.MainDBContextName].ConnectionString)
                        .Options;
                default:
                    throw new NotSupportedException($"The provider {provider} is not supported.");
            }  
        }


        public static void SetTransactionManagerField(string fieldName, object value)
        {
            typeof(TransactionManager).GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Static).SetValue(null, value);
        }

        public const string MainDBContextName = "MainCatalogueContext";
        public static TransactionScope CreateTransactionScope(TransactionScopeOption option, TimeSpan timeout, TransactionOptions transactionOptions)
        {
            SetTransactionManagerField("_cachedMaxTimeout", true);
            SetTransactionManagerField("_maximumTimeout", timeout);
            transactionOptions.Timeout = timeout;
            var temp = new TransactionScope(option, transactionOptions);

            return temp;
        }
    }
}
