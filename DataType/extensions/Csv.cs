using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using SMNETCORE.Common.Enums;

namespace SMNETCORE.DataType.Extensions
{
    public static class Csv
    {
        /// <summary>
        /// Converts a single object to CSV string. If you don't set the flags, this method generates the CSV string based on all public instance properties of the object.
        /// </summary>
        /// <param name="sourceObject">Object to convert.</param>
        /// <param name="seperator">Seperator string. The default is comma.</param>
        /// <param name="includeHeader">Set to false if you don't want the property names to be included as header.</param>
        /// <param name="sortMethod">Defines the sort order of the properties in the string.</param>
        /// <returns>Csv string.</returns>
        public static string ToCsvString<T>(this T sourceObject, string seperator = ",", bool includeHeader = true,
                                      SortMethod sortMethod = SortMethod.Orderless)
        {
            return sourceObject.ToCsvString(BindingFlags.Instance | BindingFlags.Public, seperator, includeHeader, sortMethod);
        }

        /// <summary>
        /// Converts a single object to CSV string. If you don't set the flags, this method generates the CSV string based on all public instance properties of the object.
        /// </summary>
        /// <param name="dr">Data Row</param>
        /// <param name="seperator">Seperator string. The default is comma.</param>
        /// <param name="includeHeader">Set to false if you don't want the property names to be included as header.</param>
        /// <returns>Csv string.</returns>
        public static string DataRowToCsvString(this DataRow dr, string seperator = ",", bool includeHeader = true)
        {
            StringBuilder sb = new StringBuilder();

            
            if (includeHeader && dr != null && dr.Table != null && dr.Table.Columns.Cast<DataColumn>().IsValid())
            {
                var columnNames = dr.Table.Columns.Cast<DataColumn>().Select(column => column.ColumnName).ToArray();
                sb.AppendLine(string.Join(seperator, columnNames));
            }

            if (dr != null && dr.ItemArray.IsValid())
            {
                var fields = dr.ItemArray.Select(field => field.NullableToString()).ToArray();
                sb.AppendLine(string.Join(seperator, fields));
            }
            return sb.NullableToString();

        }

        /// <summary>
        /// Converts a single object to CSV string.
        /// </summary>
        /// <param name="sourceObject">Object to convert.</param>
        /// <param name="flags">The flags to extract the properties.</param>
        /// <param name="seperator">Seperator string. The default is comma.</param>
        /// <param name="includeHeader">Set to false if you don't want the property names to be included as header.</param>
        /// <param name="sortMethod">Defines the sort order of the properties in the string.</param>
        /// <returns>Csv string.</returns>
        public static string ToCsvString<T>(this T sourceObject, BindingFlags flags, string seperator = ",",
                                      bool includeHeader = true, SortMethod sortMethod = SortMethod.Orderless)
        {
            var properties = sourceObject.GetType().GetProperties(flags);

            IEnumerable<PropertyInfo> sortedProperties = new PropertyInfo[0];

            switch (sortMethod)
            {
                case SortMethod.Ascending:
                    sortedProperties = properties.OrderBy(x => x.Name).ToArray();
                    break;
                case SortMethod.Descending:
                    sortedProperties = properties.OrderByDescending(x => x.Name).ToArray();
                    break;
                case SortMethod.Orderless:
                    sortedProperties = properties.ToArray();
                    break;
            }

            var csvBuilder = new StringBuilder();

            if (includeHeader)
                csvBuilder.AppendLine(String.Join(seperator, sortedProperties.Select(x => x.Name)));

            csvBuilder.Append(String.Join(seperator, sortedProperties.Select(x =>
                {
                    var value = sourceObject.GetPropertyValue(x.Name);
                    return value == null ? String.Empty.EscapeQuotes() : value.ToString().EscapeQuotes();
                })));

            return csvBuilder.ToString();
        }

        /// <summary>
        /// Gets the CSV header of the specified object.If you don't set the flags, this method generates the CSV string based on all public instance properties of the object.
        /// </summary>
        /// <param name="sourceObject">Object to convert.</param>
        /// <param name="flags">The flags to extract the properties.</param>
        /// <param name="seperator">Seperator string. The default is comma.</param>
        /// <param name="sortMethod">Defines the sort order of the properties in the string.</param>
        /// <returns>Csv string.</returns>
        public static string GetCsvHeader<T>(this T sourceObject, BindingFlags flags, string seperator = ",",
                                             SortMethod sortMethod = SortMethod.Orderless)
        {
            var properties = sourceObject.GetType().GetProperties(flags);

            IEnumerable<PropertyInfo> sortedProperties = new PropertyInfo[0];

            switch (sortMethod)
            {
                case SortMethod.Ascending:
                    sortedProperties = properties.OrderBy(x => x.Name).ToArray();
                    break;
                case SortMethod.Descending:
                    sortedProperties = properties.OrderByDescending(x => x.Name).ToArray();
                    break;
                case SortMethod.Orderless:
                    sortedProperties = properties.ToArray();
                    break;
            }

            return String.Join(seperator, sortedProperties.Select(x => x.Name));
        }

        /// <summary>
        /// Gets the CSV header of the specified object.
        /// </summary>
        /// <param name="sourceObject">Object to extract the header.</param>
        /// <param name="seperator">Seperator string. The default is comma.</param>
        /// <param name="sortMethod">Defines the sort order of the properties in the string.</param>
        /// <returns>Csv string.</returns>
        public static string GetCsvHeader<T>(this T sourceObject, string seperator = ",",
                                             SortMethod sortMethod = SortMethod.Orderless)
        {
            return sourceObject.GetCsvHeader(BindingFlags.Instance | BindingFlags.Public, seperator, sortMethod);
        }

        /// <summary>
        /// Converts a list of objects to CSV string. If you don't set the flags, this method generates the CSV string based on all public instance properties of the objects.
        /// </summary>
        /// <param name="list">The list to convert.</param>
        /// <param name="seperator">Seperator string. The default is comma.</param>
        /// <param name="includeHeader">Set to false if you don't want the property names to be included as header.</param>
        /// <param name="sortMethod">Defines the sort order of the properties in the string.</param>
        /// <returns>Csv string.</returns>
        public static string ToCsv<T>(this IEnumerable<T> list, string seperator = ",", bool includeHeader = true,
                                      SortMethod sortMethod = SortMethod.Orderless)
        {
            return ToCsv(list, BindingFlags.Instance | BindingFlags.Public, seperator, includeHeader, sortMethod);
        }

        /// <summary>
        /// Converts a list of objects to CSV string. If you don't set the flags, this method generates the CSV string based on all public instance properties of the objects.
        /// </summary>
        /// <param name="list">The list to convert.</param>
        /// <param name="flags">The flags to extract the properties.</param>
        /// <param name="seperator">Seperator string. The default is comma.</param>
        /// <param name="includeHeader">Set to false if you don't want the property names to be included as header.</param>
        /// <param name="sortMethod">Defines the sort order of the properties in the string.</param>
        /// <returns>Csv string.</returns>
        public static string ToCsv<T>(this IEnumerable<T> list, BindingFlags flags, string seperator = ",",
                                      bool includeHeader = true, SortMethod sortMethod = SortMethod.Orderless)
        {
            var csvBuilder = new StringBuilder();

            if (list == null || !list.Any())
                return String.Empty;

            if (includeHeader)
                csvBuilder.AppendLine(list.First().GetCsvHeader(flags, seperator, sortMethod));

            foreach (var sourceObject in list)
                csvBuilder.AppendLine(sourceObject.ToCsvString(flags, seperator, false, sortMethod));

            return csvBuilder.ToString();
        }
    }
}
