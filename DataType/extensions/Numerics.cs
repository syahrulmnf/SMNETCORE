using SMNETCORE.Common;
using SMNETCORE.Common.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SMNETCORE.DataType.Extensions
{
    public static class Numerics
    {
        public static string ToPercent(this decimal value, decimal total)
        {
            return total == 0 ? String.Format("{0:0.00}", total) : String.Format("{0:0.00}", (value * 100) / total);
        }

         public static int ToInt(this bool value)
        {
            return value ? 1 : 0;
        }

        public static int ToInt(this bool? value)
        {
            return value.GetValueOrDefault(false).ToInt();
        }

        public static Decimal ScaleNumberValue(this decimal value, decimal? oldScale, decimal? newScale)
        {
            return value == 0 ? value :  (value * (newScale ?? 1) / (oldScale ?? 1));
        }
        public static Decimal ScaleNumberValue(this decimal value, decimal total, decimal oldScale , decimal newScale )
        {
            return total == 0 ? total : value.ScaleNumberValue(oldScale, newScale)/ total;
        }
        public static string FormatScaleNumberValue(this decimal value, decimal oldScale, decimal newScale )
        {
            return String.Format("{0:0.00}", value.ScaleNumberValue(oldScale, newScale));
        }
        public static string FormatScaleNumberValue(this decimal value, decimal total, decimal oldScale , decimal newScale)
        {
            return String.Format("{0:0.00}", value.ScaleNumberValue(total, oldScale, newScale));
        }

        public static decimal? ConvertsFromFormat(this decimal? data, string format = Globals.NumberFormat.N0)
        {
            if (!data.HasValue) return null;
            return (decimal?)decimal.Parse(data.Value.ToString(format));
        }

        public static decimal? ImprovementsDecimalNumber(this decimal? data, decimal? data2, string format = Globals.NumberFormat.N0)
        {
            if (!data.HasValue || !data2.HasValue) return null;
            return ((decimal?)(data.ConvertsFromFormat(format) - data2.ConvertsFromFormat(format))).ConvertsFromFormat(format);
        }
    }
}
