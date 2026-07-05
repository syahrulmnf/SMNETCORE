
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using SMNETCORE.Cache.Enums;
using SMNETCORE.Common;
using SMNETCORE.DAL.BaseDAL.Common;
using SMNETCORE.DAL.BaseDAL.Context;
using SMNETCORE.DAL.BaseDAL.Models;
using SMNETCORE.DataType.Extensions;
using SMNETCORE.Logging;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Linq.Expressions;
using System.Transactions;

namespace SMNETCORE.DAL.BaseDAL.Repositories.Tenant
{
    public class TenantRepository<T> : IDisposable
        where T : DalContextBaseModel, new()
    {
        public TenantDTOModel Tenant { get; set; } = new TenantDTOModel();
        public DBProvider DBProvider { get; set; } = DBProvider.SQLServer;
        public TenantRepository(TenantDTOModel tenant, DBProvider dbProvider = DBProvider.SQLServer)
        {
            Tenant = tenant;
            DBProvider = dbProvider;
        }


        internal TenantDALContext InternalContext
        {
            get
            {
                return TenantContextManager.GetContextForTenant(Tenant.Name, DBProvider);
            }
        }

   
    

        /// <summary>
        /// Use this for Thread
        /// Will be destroyed, Disposable
        /// </summary>
        public TenantDALContext NewInternalContext()
        {
            if (Tenant == null) throw new Exception("Tenant Name not properly set");
            var temp = new TenantDALContext(Tenant, DBProvider  );
            return temp;
        }


     
        public virtual IEnumerable<T> Get(
            Expression<Func<T, bool>> filter = null,
            Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null,
            string includeProperties = "")
        {
            IQueryable<T> query = InternalContext.Set<T>();
            if (filter != null)
            {
                query = query.Where(filter);
            }

            foreach (var includeProperty in includeProperties.Split
                (new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
            {
                query = query.Include(includeProperty);
            }

            if (orderBy != null)
            {
                var dta = orderBy(query).ToList();
                return dta;
            }
            else
            {
                var dta = query.ToList();
                return dta;
            }
        }

        public IEnumerable<KeyValuePair<string, object>> GetKeys(T entity)
        {
            var properties = entity.GetType().GetProperties().Where(d => d.IsDefined(typeof(KeyAttribute), false)).EnumToList();
            if (properties.IsValid())
            {
                var result = properties.Where(d => entity.HasProperty(d.Name)).Select(d => new KeyValuePair<string, object>(d.Name, entity.GetPropertyValue(d.Name))).EnumToList();
                return result;
            }
            return new List<KeyValuePair<string, object>>();
        }


        public virtual T GetByID(params object[] id)
        {
            try
            {
                var dta = InternalContext.Set<T>().Find(id);
                return dta;
            }
            catch (Exception exc)
            {
                Logger.LogError(exc, LogCategoryType.Common);
                throw exc;
            }
        }

        public virtual void Insert(ref T entity, bool forceNewRecord = true)
        {
            try
            {
                using (var context = NewInternalContext())
                {
                    context.Update(entity, true, forceNewRecord);
                }
            }
            catch (Exception exc)
            {
                Logger.LogError(exc, LogCategoryType.Common);
                throw exc;
            }
        }

        public virtual void Insert(ref IEnumerable<T> entityData, bool forceNewRecord = true)
        {
            try
            {
                var entity = entityData.EnumToList();
                using (var context = NewInternalContext())
                {
                    List<T> dtaEntity = new List<T>();
                    foreach (var dtaModel in entity)
                    {
                        context.Update(dtaModel, false, forceNewRecord);
                     
                    }
                    context.SaveChangeWithValidation();
                    entityData = entity;
                }
            }
            catch (Exception exc)
            {
                Logger.LogError(exc, LogCategoryType.Common);
                throw exc;
            }

        }

        public virtual void DeleteVirtually(ref T data)
        {
            try
            {
                if (!data.HasProperty("Deleted")) throw new Exception("Table does not have Deleted Column.");
                data.SetPropertyValue("Deleted", true);
                Update(ref data);
            }
            catch (Exception exc)
            {
                Logger.LogError(exc, LogCategoryType.Common);
                throw exc;
            }
        }


        public virtual void SetInActive(ref T data)
        {
            try
            {
                if (!data.HasProperty("Active") && !data.HasProperty("IsActive")) throw new Exception("Table does not have Deleted Column.");
                data.SetPropertyValue("Active", false);
                data.SetPropertyValue("IsActive", false);
                Update(ref data);
            }
            catch (Exception exc)
            {
                Logger.LogError(exc, LogCategoryType.Common);
                throw exc;
            }
        }


        public virtual void DeleteById(object id)
        {
            try
            {
                using (var context = NewInternalContext())
                {
                    T? entityToDelete = context.Set<T>().Find(id);
                    if(entityToDelete != null) Delete(ref entityToDelete);
                }
            }
            catch (Exception exc)
            {
                Logger.LogError(exc, LogCategoryType.Common);
                throw exc;
            }

        }

        public virtual void Delete(ref T entityToDelete)
        {
            try
            {
                using (var context = NewInternalContext())
                {
                    context.Delete(entityToDelete);
                }
            }
            catch (Exception exc)
            {
                Logger.LogError(exc, LogCategoryType.Common);
                throw exc;
            }
        }

        public virtual void Delete(ref IEnumerable<T> entityToDeleteData)
        {
            try
            {
                var entityToDelete = entityToDeleteData.EnumToList();
                using (var context = NewInternalContext())
                {
                    foreach (var dtModel in entityToDelete)
                    {
                        context.Delete(dtModel, false);
                    }
                    //dbSet.Remove(dta);
                    context.SaveChangeWithValidation();
                    entityToDeleteData = entityToDelete;
                }
            }
            catch (Exception exc)
            {
                Logger.LogError(exc, LogCategoryType.Common);
                throw exc;
            }
        }

        public virtual void UpdateKeysChecking(ref T entityToUpdate, bool forceNewRecord = false)
        {
            try
            {
                using (var context = NewInternalContext())
                {
                    var keysData = GetKeys(entityToUpdate);
                    if (!keysData.IsValid())
                    {
                        context.Update(entityToUpdate, false, forceNewRecord);
                    }
                    else
                    {
                        var valuesKeys = keysData.Select(d => d.Value).ToArray();
                        var existsData = context.Set<T>().Find(valuesKeys);

                        if (existsData != null)
                        {
                            var entry = context.Entry(existsData);
                            entry.CurrentValues.SetValues(entityToUpdate);
                        }
                        else
                        {
                            context.Update(entityToUpdate, false, forceNewRecord);
                        }
                    }
                    context.SaveChangeWithValidation();
                }
            }
            catch (Exception exc)
            {
                Logger.LogError(exc, LogCategoryType.Common);
                throw exc;
            }
        }

        public virtual void UpdateKeysChecking(ref List<T> entityToUpdateList, bool disableAutoDetectChanges = true, bool forceNewRecord = false)
        {
            var tmp = entityToUpdateList.AsEnumerable();
            UpdateKeysChecking(ref tmp, disableAutoDetectChanges, forceNewRecord);
            entityToUpdateList = tmp.EnumToList();
        }


        public virtual void UpdateKeysChecking(ref IEnumerable<T> entityToUpdateListData, bool disableAutoDetectChanges = true, bool forceNewRecord = false)
        {
            var transactionOptions = new TransactionOptions();
            try
            {
                transactionOptions.IsolationLevel = System.Transactions.IsolationLevel.ReadUncommitted;
                //create the transaction scope, passing our options in
                using (var transactionScope = Globals.CreateTransactionScope(TransactionScopeOption.RequiresNew, new TimeSpan(1, 30, 0), transactionOptions))
                {
                    using (var context = NewInternalContext())
                    {
                        bool backup = context.ChangeTracker.AutoDetectChangesEnabled;
                        if (disableAutoDetectChanges) context.ChangeTracker.AutoDetectChangesEnabled = false;

                        try
                        {
                            var entityToUpdateList = entityToUpdateListData.EnumToList();
                            foreach (var entityToUpdate in entityToUpdateList)
                            {
                                try
                                {
                                    var keysData = GetKeys(entityToUpdate);
                                    if (!keysData.IsValid())
                                    {
                                        context.Update(entityToUpdate, false, forceNewRecord);
                                    }
                                    else
                                    {
                                        var valuesKeys = keysData.Select(d => d.Value).ToArray();
                                        var existsData = context.Set<T>().Find(valuesKeys);

                                        if (existsData != null)
                                        {
                                            var entry = context.Entry(existsData);
                                            entry.CurrentValues.SetValues(entityToUpdate);
                                        }
                                        else
                                        {
                                            context.Update(entityToUpdate, false, forceNewRecord);
                                        }
                                    }
                                    context.SaveChangeWithValidation();
                                }
                                catch (Exception exc)
                                {
                                    var setting = Globals.GenericHelper.JSONConvertsSetting(isObject: false);
                                    var strMessage = JsonConvert.SerializeObject(entityToUpdate, setting);
                                    Logger.LogError("Update Keys And Checking Error Data : " + strMessage, LogCategoryType.DataRepository);
                                    throw exc;
                                }
                            }

                            entityToUpdateListData = entityToUpdateList;
                        }
                        catch (Exception exc)
                        {
                            Logger.LogError(exc, LogCategoryType.DataRepository);
                            throw exc;

                        }
                        finally
                        {
                            if (disableAutoDetectChanges) context.ChangeTracker.AutoDetectChangesEnabled = backup;
                            transactionScope.Complete();
                        }
                    }
                }
            }
            catch (Exception exc)
            {
                Logger.LogError(exc, LogCategoryType.Common);
                throw exc;
            }
        }


        public virtual void Update(ref T entityToUpdate)
        {
            try
            {
                var tmp = entityToUpdate;
                UpdateKeysChecking(ref tmp);
                entityToUpdate = tmp;
            }
            catch (Exception exc)
            {
                Logger.LogError(exc, LogCategoryType.Common);
                throw exc;
            }
        }

        public virtual void Update(ref List<T> entityToUpdate)
        {
            try
            {
                var tmp = entityToUpdate.AsEnumerable();
                UpdateKeysChecking(ref tmp);
                entityToUpdate = tmp.EnumToList();
            }
            catch (Exception exc)
            {
                Logger.LogError(exc, LogCategoryType.Common);
                throw exc;
            }
        }


        private bool disposed = false;

        public virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    InternalContext.Dispose();
                }
            }
            this.disposed = true;
        }

        void IDisposable.Dispose()
        {
            if (InternalContext != null) Dispose(true);
            GC.SuppressFinalize(this);
        }

        public virtual string CacheKeyName { get { return "Repository"; } }
        public virtual CacheCollectionType CacheTypeEnum { get { return CacheCollectionType.Generic; } }
    }
}
