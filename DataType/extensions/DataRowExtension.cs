using SMNETCORE.Logging;
using System.Data;

namespace SMNETCORE.DataType.Extensions
{
    /// Summary:
    ///     Defines the extension methods to the System.Data.DataRow class. This is a
    ///     static class.
    public static class DataRowExtensions
    {
        /// Summary:
        ///     Provides strongly-typed access to each of the column values in the specified
        ///     row. The System.Data.DataRowExtensions.Field<T0>(System.Data.DataRow,System.Data.DataColumn)
        ///     method also supports nullable types.
        ///
        /// Parameters:
        ///   row:
        ///     The input System.Data.DataRow, which acts as the this instance for the extension
        ///     method.
        ///
        ///   column:
        ///     The input System.Data.DataColumn object that specifies the column to return
        ///     the value of.
        ///
        /// Type parameters:
        ///   T:
        ///     A generic parameter that specifies the return type of the column.
        ///
        /// Returns:
        ///     The value, of type T, of the System.Data.DataColumn specified by column.
        ///
        /// Exceptions:
        ///   System.InvalidCastException:
        ///     The value type of the underlying column could not be cast to the type specified
        ///     by the generic parameter, T.
        ///
        ///   System.IndexOutOfRangeException:
        ///     The column specified by column does not occur in the System.Data.DataTable
        ///     that the System.Data.DataRow is a part of.
        ///
        ///   System.NullReferenceException:
        ///     A null value was assigned to a non-nullable type.
        public static T FeedbackField<T>(this DataRow row, DataColumn column)
        {
            T data = row.Field<T>(column);
            try
            {
                if (typeof(T) == typeof(string) || typeof(T) == typeof(String))
                {
                    if (data != null)
                    {
                        var strData = data.To<string>().Value;
                        if (!string.IsNullOrEmpty(strData)) data = strData.TrimString().To<T>().Value;
                    }
                }
            }
            catch (Exception exc)
            {
                Logger.LogError(exc, LogCategoryType.Common);
            }
            return data;
        }

        ///
        /// Summary:
        ///     Provides strongly-typed access to each of the column values in the specified
        ///     row. The System.Data.DataRowExtensions.Field<T0>(System.Data.DataRow,System.Int32)
        ///     method also supports nullable types.
        ///
        /// Parameters:
        ///   row:
        ///     The input System.Data.DataRow, which acts as the this instance for the extension
        ///     method.
        ///
        ///   columnIndex:
        ///     The column index.
        ///
        /// Type parameters:
        ///   T:
        ///     A generic parameter that specifies the return type of the column.
        ///
        /// Returns:
        ///     The value, of type T, of the System.Data.DataColumn specified by columnIndex.
        ///
        /// Exceptions:
        ///   System.InvalidCastException:
        ///     The value type of the underlying column could not be cast to the type specified
        ///     by the generic parameter, T.
        ///
        ///   System.IndexOutOfRangeException:
        ///     The column specified by ordinal does not exist in the System.Data.DataTable
        ///     that the System.Data.DataRow is a part of.
        ///
        ///   System.NullReferenceException:
        ///     A null value was assigned to a non-nullable type.
        public static T FeedbackField<T>(this DataRow row, int columnIndex)
        {
            T data = row.Field<T>(columnIndex);
            try
            {
                if (typeof(T) == typeof(string) || typeof(T) == typeof(String))
                {
                    if (data != null)
                    {
                        var strData = data.To<string>().Value;
                        if (!string.IsNullOrEmpty(strData)) data = strData.TrimString().To<T>().Value;
                    }
                }
            }
            catch (Exception exc)
            {
                Logger.LogError(exc, LogCategoryType.Common);
            }
            return data;
        }

        ///
        /// Summary:
        ///     Provides strongly-typed access to each of the column values in the specified
        ///     row. The System.Data.DataRowExtensions.Field<T0>(System.Data.DataRow,System.String)
        ///     method also supports nullable types.
        ///
        /// Parameters:
        ///   row:
        ///     The input System.Data.DataRow, which acts as the this instance for the extension
        ///     method.
        ///
        ///   columnName:
        ///     The name of the column to return the value of.
        ///
        /// Type parameters:
        ///   T:
        ///     A generic parameter that specifies the return type of the column.
        ///
        /// Returns:
        ///     The value, of type T, of the System.Data.DataColumn specified by columnName.
        ///
        /// Exceptions:
        ///   System.InvalidCastException:
        ///     The value type of the underlying column could not be cast to the type specified
        ///     by the generic parameter, T.
        ///
        ///   System.IndexOutOfRangeException:
        ///     The column specified by columnName does not occur in the System.Data.DataTable
        ///     that the System.Data.DataRow is a part of.
        ///
        ///   System.NullReferenceException:
        ///     A null value was assigned to a non-nullable type.
        public static T FeedbackField<T>(this DataRow row, string columnName)
        {
            if (!string.IsNullOrEmpty(columnName)) columnName = columnName.TrimString();
            var column = row.Table.Columns.Cast<DataColumn>().FirstOrDefault(d => d.ColumnName.ToLower() == columnName.ToLower());
            if (column == null)
            {
                Logger.Log("Column : " + columnName + ", Does not exist ", LogCategoryType.Common, LogLevelType.Warning);
                return default(T);
            }
            
            T data = row.Field<T>(column);
            try
            {
                if (typeof(T) == typeof(string) || typeof(T) == typeof(String))
                {
                    if (data != null)
                    {
                        var strData = data.To<string>().Value;
                        if (!string.IsNullOrEmpty(strData)) data = strData.TrimString().To<T>().Value;
                    }
                }
            }
            catch (Exception exc)
            {
                Logger.LogError(exc, LogCategoryType.Common);
            }
            return data;
        }

        ///
        /// Summary:
        ///     Provides strongly-typed access to each of the column values in the specified
        ///     row. The System.Data.DataRowExtensions.Field<T0>(System.Data.DataRow,System.Data.DataColumn,System.Data.DataRowVersion)
        ///     method also supports nullable types.
        ///
        /// Parameters:
        ///   row:
        ///     The input System.Data.DataRow, which acts as the this instance for the extension
        ///     method.
        ///
        ///   column:
        ///     The input System.Data.DataColumn object that specifies the column to return
        ///     the value of.
        ///
        ///   version:
        ///     A System.Data.DataRowVersion enumeration that specifies the version of the
        ///     column value to return, such as Current or Original version.
        ///
        /// Type parameters:
        ///   T:
        ///     A generic parameter that specifies the return type of the column.
        ///
        /// Returns:
        ///     The value, of type T, of the System.Data.DataColumn specified by column and
        ///     version.
        ///
        /// Exceptions:
        ///   System.InvalidCastException:
        ///     The value type of the underlying column could not be cast to the type specified
        ///     by the generic parameter, T.
        ///
        ///   System.IndexOutOfRangeException:
        ///     The column specified by column does not exist in the System.Data.DataTable
        ///     that the System.Data.DataRow is a part of.
        ///
        ///   System.NullReferenceException:
        ///     A null value was assigned to a non-nullable type.
        public static T FeedbackField<T>(this DataRow row, DataColumn column, DataRowVersion version)
        {
            T data = row.Field<T>(column, version);
            try
            {
                if (typeof(T) == typeof(string) || typeof(T) == typeof(String))
                {
                    if (data != null)
                    {
                        var strData = data.To<string>().Value;
                        if (!string.IsNullOrEmpty(strData)) data = strData.TrimString().To<T>().Value;
                    }
                }
            }
            catch (Exception exc)
            {
                Logger.LogError(exc, LogCategoryType.Common);
            }
            return data;
        }

        ///
        /// Summary:
        ///     Provides strongly-typed access to each of the column values in the specified
        ///     row. The System.Data.DataRowExtensions.Field<T0>(System.Data.DataRow,System.Int32,System.Data.DataRowVersion)
        ///     method also supports nullable types.
        ///
        /// Parameters:
        ///   row:
        ///     The input System.Data.DataRow, which acts as the this instance for the extension
        ///     method.
        ///
        ///   columnIndex:
        ///     The zero-based ordinal of the column to return the value of.
        ///
        ///   version:
        ///     A System.Data.DataRowVersion enumeration that specifies the version of the
        ///     column value to return, such as Current or Original version.
        ///
        /// Type parameters:
        ///   T:
        ///     A generic parameter that specifies the return type of the column.
        ///
        /// Returns:
        ///     The value, of type T, of the System.Data.DataColumn specified by ordinal
        ///     and version.
        ///
        /// Exceptions:
        ///   System.InvalidCastException:
        ///     The value type of the underlying column could not be cast to the type specified
        ///     by the generic parameter, T.
        ///
        ///   System.IndexOutOfRangeException:
        ///     The column specified by ordinal does not exist in the System.Data.DataTable
        ///     that the System.Data.DataRow is a part of.
        ///
        ///   System.NullReferenceException:
        ///     A null value was assigned to a non-nullable type.
        public static T FeedbackField<T>(this DataRow row, int columnIndex, DataRowVersion version)
        {
            T data = row.Field<T>(columnIndex, version);
            try
            {
                if (typeof(T) == typeof(string) || typeof(T) == typeof(String))
                {
                    if (data != null)
                    {
                        var strData = data.To<string>().Value;
                        if (!string.IsNullOrEmpty(strData)) data = strData.TrimString().To<T>().Value;
                    }
                }
            }
            catch (Exception exc)
            {
                Logger.LogError(exc, LogCategoryType.Common);
            }
            return data;
        }

        ///
        /// Summary:
        ///     Provides strongly-typed access to each of the column values in the specified
        ///     row. The System.Data.DataRowExtensions.Field<T0>(System.Data.DataRow,System.String,System.Data.DataRowVersion)
        ///     method also supports nullable types.
        ///
        /// Parameters:
        ///   row:
        ///     The input System.Data.DataRow, which acts as the this instance for the extension
        ///     method.
        ///
        ///   columnName:
        ///     The name of the column to return the value of.
        ///
        ///   version:
        ///     A System.Data.DataRowVersion enumeration that specifies the version of the
        ///     column value to return, such as Current or Original version.
        ///
        /// Type parameters:
        ///   T:
        ///     A generic parameter that specifies the return type of the column.
        ///
        /// Returns:
        ///     The value, of type T, of the System.Data.DataColumn specified by columnName
        ///     and version.
        ///
        /// Exceptions:
        ///   System.InvalidCastException:
        ///     The value type of the underlying column could not be cast to the type specified
        ///     by the generic parameter, T.
        ///
        ///   System.IndexOutOfRangeException:
        ///     The column specified by columnName does not exist in the System.Data.DataTable
        ///     that the System.Data.DataRow is a part of.
        ///
        ///   System.NullReferenceException:
        ///     A null value was assigned to a non-nullable type.
        public static T FeedbackField<T>(this DataRow row, string columnName, DataRowVersion version)
        {
            if (!string.IsNullOrEmpty(columnName)) columnName = columnName.TrimString();
            var column = row.Table.Columns.Cast<DataColumn>().FirstOrDefault(d => d.ColumnName.ToLower() == columnName.ToLower());
            if (column == null) return default(T);
            T data = row.Field<T>(column, version);
            try
            {
                if (typeof(T) == typeof(string) || typeof(T) == typeof(String))
                {
                    if (data != null)
                    {
                        var strData = data.To<string>().Value;
                        if (!string.IsNullOrEmpty(strData)) data = strData.TrimString().To<T>().Value;
                    }
                }
            }
            catch (Exception exc)
            {
                Logger.LogError(exc, LogCategoryType.Common);
            }
            return data;
        }

        ///
        /// Summary:
        ///     Sets a new value for the specified column in the System.Data.DataRow. The
        ///     System.Data.DataRowExtensions.SetField<T0>(System.Data.DataRow,System.Data.DataColumn,T0)
        ///     method also supports nullable types.
        ///
        /// Parameters:
        ///   row:
        ///     The input System.Data.DataRow, which acts as the this instance for the extension
        ///     method.
        ///
        ///   column:
        ///     The input System.Data.DataColumn specifies which row value to retrieve.
        ///
        ///   value:
        ///     The new row value for the specified column, of type T.
        ///
        /// Type parameters:
        ///   T:
        ///     A generic parameter that specifies the value type of the column.
        ///
        /// Exceptions:
        ///   System.ArgumentException:
        ///     The column specified by column cannot be found.
        ///
        ///   System.ArgumentNullException:
        ///     The column is null.
        ///
        ///   System.Data.DeletedRowInaccessibleException:
        ///     Occurs when attempting to set a value on a deleted row.
        ///
        ///   System.InvalidCastException:
        ///     The value type of the underlying column could not be cast to the type specified
        ///     by the generic parameter, T.
        public static void FeedbackSetField<T>(this DataRow row, DataColumn column, T value)
        {
            
            try
            {
                if (typeof(T) == typeof(string) || typeof(T) == typeof(String))
                {
                    if (value != null)
                    {
                        var strData = value.To<string>().Value;
                        if (!string.IsNullOrEmpty(strData)) value = strData.TrimString().To<T>().Value;
                    }
                }
            }
            catch (Exception exc)
            {
                Logger.LogError(exc, LogCategoryType.Common);
            }
            row.SetField<T>(column, value);
        }

        ///
        /// Summary:
        ///     Sets a new value for the specified column in the System.Data.DataRow the
        ///     method is called on. The System.Data.DataRowExtensions.SetField<T0>(System.Data.DataRow,System.Int32,T0)
        ///     method also supports nullable types.
        ///
        /// Parameters:
        ///   row:
        ///     The input System.Data.DataRow, which acts as the this instance for the extension
        ///     method.
        ///
        ///   columnIndex:
        ///     The zero-based ordinal of the column to set the value of.
        ///
        ///   value:
        ///     The new row value for the specified column, of type T.
        ///
        /// Type parameters:
        ///   T:
        ///     A generic parameter that specifies the value type of the column.
        ///
        /// Exceptions:
        ///   System.Data.DeletedRowInaccessibleException:
        ///     Occurs when attempting to set a value on a deleted row.
        ///
        ///   System.IndexOutOfRangeException:
        ///     The ordinal argument is out of range.
        ///
        ///   System.InvalidCastException:
        ///     The value type of the underlying column could be not cast to the type specified
        ///     by the generic parameter, T.
        public static void FeedbackSetField<T>(this DataRow row, int columnIndex, T value)
        {

            try
            {
                if (typeof(T) == typeof(string) || typeof(T) == typeof(String))
                {
                    if (value != null)
                    {
                        var strData = value.To<string>().Value;
                        if (!string.IsNullOrEmpty(strData)) value = strData.TrimString().To<T>().Value;
                    }
                }
            }
            catch (Exception exc)
            {
                Logger.LogError(exc, LogCategoryType.Common);
            }
            row.SetField<T>(columnIndex, value);
        }

        ///
        /// Summary:
        ///     Sets a new value for the specified column in the System.Data.DataRow. The
        ///     System.Data.DataRowExtensions.SetField<T0>(System.Data.DataRow,System.String,T0)
        ///     method also supports nullable types.
        ///
        /// Parameters:
        ///   row:
        ///     The input System.Data.DataRow, which acts as the this instance for the extension
        ///     method.
        ///
        ///   columnName:
        ///     The name of the column to set the value of.
        ///
        ///   value:
        ///     The new row value for the specified column, of type T.
        ///
        /// Type parameters:
        ///   T:
        ///     A generic parameter that specifies the value type of the column.
        ///
        /// Exceptions:
        ///   System.ArgumentException:
        ///     The column specified by columnName cannot be found.
        ///
        ///   System.Data.DeletedRowInaccessibleException:
        ///     Occurs when attempting to set a value on a deleted row.
        ///
        ///   System.InvalidCastException:
        ///     The value type of the underlying column could not be cast to the type specified
        ///     by the generic parameter, T.
        public static void FeedbackSetField<T>(this DataRow row, string columnName, T value)
        {

            try
            {
                if (!string.IsNullOrEmpty(columnName)) columnName = columnName.TrimString();
                var column = row.Table.Columns.Cast<DataColumn>().FirstOrDefault(d => d.ColumnName.ToLower() == columnName.ToLower());
                if (column == null) return;

                if (typeof(T) == typeof(string) || typeof(T) == typeof(String))
                {
                    if (value != null)
                    {
                        var strData = value.To<string>().Value;
                        if (!string.IsNullOrEmpty(strData)) value = strData.TrimString().To<T>().Value;
                    }
                }
                row.SetField<T>(column, value);
            }
            catch (Exception exc)
            {
                Logger.LogError(exc, LogCategoryType.Common);
            }
        }
    }
}
