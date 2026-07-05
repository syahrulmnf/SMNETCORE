using SMNETCORE.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMNETCORE.DataType.Extensions
{
    public static class Conversion
    {
        public static ConversionResult<T> To<T, S>(this S value, T defaultValue)
        {
            var result = new ConversionResult<T> { PropertyType = typeof(T) };
            try
            {
                result.Value = Common.Helpers.Utils.To(value, defaultValue);
                result.Success = true;
            }
            catch (Exception e)
            {
                Logger.LogError(e, LogCategoryType.Common);
                Logger.Log("Error converting value of : " + value.GetType().FullName + " To : " + typeof(T).FullName + " - Value : " + value.ToString(), LogCategoryType.Common, LogLevelType.Error);
                result.Success = false;
                result.Value = defaultValue;
                result.Error = e;
            }

            return result;
        }

        public static ConversionResult<T> To<T, S>(this S value)
        {
            var result = new ConversionResult<T> { PropertyType = typeof(T) };
            try
            {
                result.Value = Common.Helpers.Utils.To<T, S>(value);
                result.Success = true;
            }
            catch (Exception e)
            {
                Logger.LogError(e, LogCategoryType.Common);
                Logger.Log("Error converting value of : " + value.GetType().FullName + " To : " + typeof(T).FullName + " - Value : " + value.ToString(), LogCategoryType.Common, LogLevelType.Error);
                result.Success = false;
                result.Value = default(T);
                result.Error = e;
            }

            return result;
        }


        public static ConversionResult<T> To<T>(this object value, T defaultValue)
        {
            if (value == null) return To<T, object>(value, defaultValue);
            var valueType = Common.Helpers.Utils.GetType(value.GetType());
            return valueType == typeof(string) || valueType == typeof(String) ? To<T, String>(value.NullableToString()) : To<T, object>(value, defaultValue);
        }

        public static ConversionResult<List<T>> ListTo<T, S>(this List<S> value)
        {
            var result = new ConversionResult<List<T>> { PropertyType = typeof(T) };
            try
            {
                result.Value = Common.Helpers.Utils.JsonConvertTo<T, S>(value).EnumToList();
                result.Success = true;
            }
            catch (Exception e)
            {
                Logger.LogError(e, LogCategoryType.Common);
                Logger.Log("Error converting value of : " + value.GetType().FullName + " To : " + typeof(T).FullName + " - Value : " + value.ToString(), LogCategoryType.Common, LogLevelType.Error);
                result.Success = false;
                result.Error = e;
            }

            return result;
        }

        public static ConversionResult<List<T>> ListTo<T>(this List<object> value)
        {
            return ListTo<T, object>(value);
        }

        public static ConversionResult<IEnumerable<T>> ListTo<T, S>(this IEnumerable<S> value)
        {
            var result = new ConversionResult<IEnumerable<T>> { PropertyType = typeof(T) };
            var lstResult = ListTo<T, S>(value.EnumToList());
            result.Value = lstResult.Value;
            result.Success = lstResult.Success;
            
            return result;
        }

        public static ConversionResult<IList<T>> ListTo<T, S>(this IList<S> value)
        {
            var result = new ConversionResult<IList<T>> { PropertyType = typeof(T) };
            var lstResult = ListTo<T, S>(value.EnumToList());
            result.Value = lstResult.Value;
            result.Success = lstResult.Success;

            return result;
        }

        public static ConversionResult<IList<T>> ListTo<T>(this IList<object> value)
        {
            return ListTo<T, object>(value);
        }

        public static ConversionResult<IEnumerable<T>> ListTo<T>(this IEnumerable<object> value)
        {
            return ListTo<T, object>(value);
        }
        /// <summary>
        /// Converts the string to the specified type and returns the converted value if successful, otherwise returns the type's default value.
        /// </summary>
        /// <typeparam name="T">Type to convert the string to.</typeparam>
        /// <param name="value">String to convert.</param>
        /// <returns>Converted value or deafult value.</returns>
        public static ConversionResult<T> To<T>(this object value)
        {
            var valueType = Common.Helpers.Utils.GetType(value.GetType());
            return valueType == typeof(string) || valueType == typeof(String) ? To<T, String>(value.NullableToString()) :  value.To<T>(default(T));
        }

    }
}
