using SMNETCORE.DataType.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMNETCORE.DataType.CsvParsing
{
    public static class LinqToCsv
    {
        public static string ToCsv<T>(this IEnumerable<T> items, string ReportHeader = null, IEnumerable<string> columnsToIgnore = null)
            where T : class
        {
            var csvBuilder = new StringBuilder();
            var properties = typeof(T).GetProperties();
            if (columnsToIgnore != null)
            {
                properties = properties.Where(x => !columnsToIgnore.Contains(x.Name)).ToArray();
            }
            var headers = string.Join(",", properties.Select(p => p.Name).ToArray());
            if (!string.IsNullOrEmpty(ReportHeader))
                csvBuilder.AppendLine(ReportHeader + Environment.NewLine);
            csvBuilder.AppendLine(headers);
            foreach (T item in items)
            {
                string line = string.Join(",", properties.Select(p => p.GetValue(item, null).ToCsvValue()).ToArray());
                csvBuilder.AppendLine(line);
            }
            return csvBuilder.ToString();
        }

        public static string ToCsv(this List<List<KeyValuePair<string, string>>> items, string ReportHeader = null, IEnumerable<string> columns = null, IEnumerable<string> columnsToIgnore = null)
        {
            var csvBuilder = new StringBuilder();
            var properties = columns.IsValid() ? columns.EnumToList() : items.SelectMany(d => d.Select(dd => dd.Key)).Distinct().EnumToList() ;
            if (columnsToIgnore != null)
            {
                properties = properties.Where(x => !columnsToIgnore.Contains(x)).EnumToList();
            }
            var headers = string.Join(",", properties.Select(p => p).ToArray());
            if (!string.IsNullOrEmpty(ReportHeader))
                csvBuilder.AppendLine(ReportHeader + Environment.NewLine);
            csvBuilder.AppendLine(headers);
            foreach (var itemDatas in items)
            {
                var csvData = from hh in properties
                              join itm in itemDatas on hh equals itm.Key
                              select itm.Value;
                csvBuilder.AppendLine(string.Join(",", csvData));
            }
            
            return csvBuilder.ToString();
        }

        private static string ToCsvValue<T>(this T item)
        {
            if (item == null) return "\"\"";

            if (item is string)
            {
                return string.Format("\"{0}\"", item.ToString().Replace("\"", "\\\""));
            }
            double dummy;
            if (double.TryParse(item.ToString(), out dummy))
            {
                return string.Format("{0}", item);
            }
            return string.Format("\"{0}\"", item);
        }
    }
}
