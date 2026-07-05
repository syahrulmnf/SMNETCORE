using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using SMNETCORE.DAL.BaseDAL.Common;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Text.Json;


namespace SMNETCORE.DAL.BaseDAL.Context
{
    public partial class BaseDALContext : DbContext
    {

        protected DBProvider Provider { get; set; }
        internal BaseDALContext()
        {
        }

        internal BaseDALContext(DBProvider provider)
        {
            Provider = provider;
        }

        internal BaseDALContext(DbContextOptions options, DBProvider provider) : base(options)
        {
            Provider = provider;
        }


        public bool IsOpen { get => this.Database.GetDbConnection().State == System.Data.ConnectionState.Open; }
        public bool IsClosed { get => this.Database.GetDbConnection().State == System.Data.ConnectionState.Closed; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            

            //OnModelCreatingPartial(modelBuilder);
        }
        internal void OnModelCreatingPartialPublic(ModelBuilder modelBuilder) => OnModelCreatingPartial(modelBuilder);
        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
        /// <summary>
        /// Updates an existing record/Adds a new record and saves all changes to the underlying database. 
        /// </summary>
        /// <typeparam name="T">The type of the entity to update.</typeparam>
        /// <param name="entity">The entity to update.</param>
        public void Update<T>(T entity, bool saveChangesUpdate = true, bool forceNewRecord = false) where T : class, new()
        {
            bool newRecord = IsNewRecord(entity);

            this.Entry<T>(entity).State = newRecord || forceNewRecord ? EntityState.Added : EntityState.Modified;
            if (saveChangesUpdate) SaveChangeWithValidation();
        }

        public int SaveChangeWithValidation()
        {
            try
            {
                // Your code...
                // Could also be before try if you know the exception occurs in SaveChanges

                return SaveChanges();
            }
            catch (DbUpdateException ex)
            {
                var errors = new List<object>();

                foreach (var entry in ex.Entries)
                {
                    var entityName = entry.Metadata.ClrType.Name;

                    var properties = entry.Properties.Select(p => new
                    {
                        Property = p.Metadata.Name,
                        CurrentValue = p.CurrentValue,
                        OriginalValue = p.OriginalValue,
                        IsModified = p.IsModified
                    });

                    errors.Add(new
                    {
                        Entity = entityName,
                        State = entry.State.ToString(),
                        Properties = properties
                    });
                }

                var dbError = ex.InnerException?.Message;

                throw new Exception(JsonSerializer.Serialize(new
                {
                    Error = ex.Message,
                    DatabaseError = dbError,
                    Entities = errors
                }, new JsonSerializerOptions
                {
                    WriteIndented = true
                }));
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        /// <summary>
        /// Deletes an existing record from the database and saves all changes to the underlying database. 
        /// </summary>
        /// <typeparam name="T">The type of the entity to delete.</typeparam>
        /// <param name="entity">The entity to delete.</param>
        public void Delete<T>(T entity, bool saveChanges = true) where T : class, new()
        {
            bool newRecord = IsNewRecord(entity);

            if (!newRecord)
                this.Entry<T>(entity).State = EntityState.Deleted;

            if (saveChanges)
                SaveChangeWithValidation();
        }

        /// <summary>
        /// Detaches the item from the DbSet.
        /// </summary>
        /// <typeparam name="T">The type of the entity to delete.</typeparam>
        /// <param name="entity">The entity to detach.</param>
        public void Detach<T>(T entity) where T : class, new()
        {
            this.Entry<T>(entity).State = EntityState.Detached;
        }

        /// <summary>
        /// Returns true if the entity exists, otherwise returns false.
        /// </summary>
        /// <typeparam name="T">The type of entity to check.</typeparam>
        /// <param name="entity">The entity to check.</param>
        /// <returns>True if the entity exists, otherwise false.</returns>
        public bool IsNewRecord<T>(T entity) where T : class, new()
        {
            EntityEntry<T> objectEntry = Entry(entity);
            if (objectEntry.State == EntityState.Detached)
            {
                Set<T>().Attach(entity);
                objectEntry = Entry(entity);
            }

            var entityType = objectEntry.Metadata;

            // Get the primary key properties
            var primaryKey = entityType.FindPrimaryKey();
            if (primaryKey == null) return false;
            var primaryKeyProperties = primaryKey.Properties;
            // Extract the key values
            var keyValues = primaryKeyProperties.Select(p => objectEntry.CurrentValues[p]).ToList();

            return keyValues.Any(x => x != null && string.IsNullOrWhiteSpace(x.ToString()) || x.Equals(GetDefault(x.GetType())));
        }


        #region Direct Database Access Methods

        /// <summary>
        /// Opens database connection to execute direct access methods. This connection does not affect the context connection.
        /// </summary>
        internal void OpenConnection()
        {
            if (this.Database.GetDbConnection() != null && this.Database.GetDbConnection().State != ConnectionState.Open)
                this.Database.GetDbConnection().Open();
        }

        /// <summary>
        /// Closes database connection. Closing this connection does not affect the context connection.
        /// </summary>
        internal void CloseConnection()
        {
            if (Database.GetDbConnection() != null && Database.GetDbConnection().State != ConnectionState.Closed)
                Database.GetDbConnection().Close();
        }

        /// <summary>
        /// Returns a data reader from a SQL statement.
        /// </summary>
        /// <param name="sqlCommandText">Sql command text.</param>
        /// <param name="commandType">Commant type. Default value is Text.</param>
        /// <param name="commandParameters">Command parameters</param>
        /// <param name="readerBehaviour">Reader behaviour.</param>
        /// <returns>DataReader or null.</returns>
        public DbDataReader ExecuteReader(string sqlCommandText, CommandType commandType = CommandType.Text, CommandBehavior readerBehaviour = CommandBehavior.Default, params CommandParameter[] commandParameters)
        {
            return ExecuteReader(CreateCommand(sqlCommandText, commandType, commandParameters), readerBehaviour);
        }

        /// <summary>
        /// Executes a raw Sql Command against the server that doesn't return a result set and returns the number of rows affected.
        /// </summary>
        /// <param name="sqlCommandText">Sql command text.</param>
        /// <param name="commandType">Commant type. Default value is Text.</param>
        /// <param name="tableName">Table name.</param>
        /// <param name="commandParameters">Command parameters.</param>
        /// <returns>Result data table.</returns>
        public DataTable ExecuteDataTable(string sqlCommandText, string tableName = "DataTable", CommandType commandType = CommandType.Text, params CommandParameter[] commandParameters)
        {
            return ExecuteDataTable(CreateCommand(sqlCommandText, commandType, commandParameters), tableName);
        }

        /// <summary>
        /// Executes a SQL command from a string. For UPDATE, INSERT, and DELETE statements, the return value is the number of rows affected by the command. For all other types of statements, the return value is -1
        /// </summary>
        /// <param name="sqlCommandText">Sql command text.</param>
        /// <param name="commandType">Commant type. Default value is Text.</param>
        /// <param name="commandParameters">Command parameters.</param>
        /// <returns>For UPDATE, INSERT, and DELETE statements, the return value is the number of rows affected by the command. For all other types of statements, the return value is -1.</returns>
        public int ExecuteNonQuery(string sqlCommandText, CommandType commandType = CommandType.Text, params CommandParameter[] commandParameters)
        {
            return ExecuteNonQuery(CreateCommand(sqlCommandText, commandType, commandParameters));
        }

        /// <summary>
        /// Executes the query and returns the first column of the first row in the result set returned by the query. All other columns and rows are ignored.
        /// </summary>
        /// <param name="sqlCommandText">Sql command text.</param>
        /// <param name="commandType">Commant type. Default value is Text.</param>
        /// <param name="commandParameters">Command parameters.</param>
        /// <returns>The first column of the first row in the result set.</returns>
        public object ExecuteScalar(string sqlCommandText, CommandType commandType = CommandType.Text, params CommandParameter[] commandParameters)
        {
            return ExecuteScalar(CreateCommand(sqlCommandText, commandType, commandParameters));
        }

        #endregion

        #region internal Methods



        public static object GetDefault(Type type)
        {
            return type.IsValueType ? Activator.CreateInstance(type) : null;
        }

        public DbCommand CreateCommand(string sql, CommandType commandType, params CommandParameter[] dbParameters)
        {
            var dbCommand = this.Database.GetDbConnection().CreateCommand();
            if (dbCommand != null)
            {
                dbCommand.CommandText = sql;
                dbCommand.Connection = Database.GetDbConnection();
                dbCommand.CommandType = commandType;

                if (dbParameters != null)
                {
                    foreach (var param in dbParameters)
                        dbCommand.Parameters.Add(CreateParameter(param));
                }

                return dbCommand;
            }

            return null;
        }

        public DbParameter CreateParameter(CommandParameter cmdParameter)
        {
            var parm = this.Database.GetDbConnection().CreateCommand().CreateParameter();
            if (parm != null)
            {
                parm.ParameterName = cmdParameter.Name;
                parm.Value = cmdParameter.Value;
                parm.Direction = cmdParameter.Direction;
            }
            return parm;
        }

        public DbDataReader ExecuteReader(DbCommand sqlCommand, CommandBehavior readerBehaviour = CommandBehavior.Default)
        {
            OpenConnection();
            return sqlCommand.ExecuteReader(readerBehaviour);
        }


        public DataTable? ExecuteDataTable(DbCommand sqlCommand, string tableName)
        {
            var connection = Database.GetDbConnection();
            if (connection == null) return null;
            var dbFactory = DbProviderFactories.GetFactory(connection);
            if (dbFactory == null) return null;
            var adapter = dbFactory.CreateDataAdapter();
            if (adapter != null)
            {
                adapter.SelectCommand = sqlCommand;
                var dataTable = new DataTable(tableName);

                try
                {
                    OpenConnection();
                    adapter.Fill(dataTable);
                }
                finally
                {
                    CloseConnection();
                }

                return dataTable;
            }

            return null;
        }

        public int ExecuteNonQuery(DbCommand sqlCommand)
        {
            int recordCount;
            try
            {
                OpenConnection();
                recordCount = sqlCommand.ExecuteNonQuery();
            }
            finally
            {
                CloseConnection();
            }

            return recordCount;
        }

        public object ExecuteScalar(DbCommand sqlCommand)
        {
            object result;
            try
            {
                OpenConnection();
                result = sqlCommand.ExecuteScalar();
            }
            finally
            {
                CloseConnection();
            }

            return result;
        }

        #endregion


    }
}
