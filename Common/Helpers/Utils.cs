using Newtonsoft.Json.Converters;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

namespace SMNETCORE.Common.Helpers
{
    public class Utils
    {
        public static bool TryConvertGenericStringToDate(string stringDate, out DateTime result)
        {
            result = DateTime.MinValue;
            if (string.IsNullOrEmpty(stringDate)) return false;
            stringDate = stringDate.Trim();
            stringDate = stringDate.Replace("  ", " ");
            if (string.IsNullOrEmpty(stringDate)) return false;

            long ticks;
            if (long.TryParse(stringDate, out ticks))
            {
                DateTime start = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                DateTime date = start.AddMilliseconds(ticks).ToLocalTime();
                result = date;
                return true;
            }

            string[] formats = Globals.AvailableFormatDates;

            DateTime dateValue;

            foreach (string dateStringFormat in formats)
            {
                if (DateTime.TryParseExact(stringDate, dateStringFormat,
                                           CultureInfo.InvariantCulture,
                                           DateTimeStyles.None,
                                           out dateValue))
                {
                    result = dateValue;
                    return true;
                }
            }

            return false;
        }


        public static DateTime ConvertGenericString(string stringDate)
        {
            return StringToDate(stringDate);
        }

        public static DateTime StringToDate(string stringDate)
        {
            if (string.IsNullOrEmpty(stringDate)) return DateTime.MaxValue;
            stringDate = stringDate.Trim();
            stringDate = stringDate.Replace("  ", " ");
            if (string.IsNullOrEmpty(stringDate)) return DateTime.MaxValue;

            long ticks;
            if (long.TryParse(stringDate, out ticks))
            {
                DateTime start = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                DateTime date = start.AddMilliseconds(ticks).ToLocalTime();
                return date;
            }

            string[] formats = Globals.AvailableFormatDates;

            DateTime dateValue;

            foreach (string dateStringFormat in formats)
            {
                if (DateTime.TryParseExact(stringDate, dateStringFormat,
                                           CultureInfo.InvariantCulture,
                                           DateTimeStyles.None,
                                           out dateValue))
                    return dateValue;
            }

            throw new Exception("Invalid Date Format Supplied : " + stringDate + ", Available Format: " + string.Join("||", formats));
        }

        public static DateTimeOffset SteringToDatetimeOffset(string stringDate)
        {
            if (string.IsNullOrEmpty(stringDate)) return DateTime.MaxValue;
            stringDate = stringDate.Trim();
            stringDate = stringDate.Replace("  ", " ");
            if (string.IsNullOrEmpty(stringDate)) return DateTime.MaxValue;

            long ticks;
            if (long.TryParse(stringDate, out ticks))
            {
                DateTimeOffset start = new DateTimeOffset(1970, 1, 1, 0, 0, 0, new TimeSpan(0));
                DateTimeOffset date = start.AddMilliseconds(ticks).ToLocalTime();
                return date;
            }

            string[] formats = Globals.AvailableFormatDates;

            DateTimeOffset dateValue;

            foreach (string dateStringFormat in formats)
            {
                if (DateTimeOffset.TryParseExact(stringDate, dateStringFormat,
                                           CultureInfo.InvariantCulture,
                                           DateTimeStyles.None,
                                           out dateValue))
                    return dateValue;
            }

            throw new Exception("Invalid DateTimeOffset Format Supplied : " + stringDate + ", Available Format: " + string.Join("||", formats));
        }

        /// <summary>
        /// Converts the string to the specified type and returns the converted value if successful, otherwise returns the specified default value.
        /// </summary>
        /// <typeparam name="T">Type to convert the string to.</typeparam>
        /// <param name="value">String to convert.</param>
        /// <param name="defaultValue">Default value to return if conversion was unsuccessful.</param>
        /// <returns>Converted value or the specified deafult value.</returns>
        public static T To<T, S>(S value)
        {
            return To<T, S>(value, (T)GetDefaultValue(typeof(T)));
        }

        public static T To<T, S>(S value, T defaultValue)
        {
            if (string.IsNullOrEmpty(NullableToString(value))) return defaultValue;
            
            var sourceType = GetType(typeof(S));
            var destinationType = GetType(typeof(T));
            if(destinationType == typeof(object)) return (T)(object)value;
            if(destinationType == typeof(String) || destinationType == typeof(string))
            {
               
                var rstString = NullableToString<S>(value);
                return string.IsNullOrEmpty(rstString) ? defaultValue : (T)Convert.ChangeType(rstString, destinationType);
            }

            var originalSourceType = GetOriginalType(sourceType);
            var originalDestinationType = GetOriginalType(destinationType);

            try
            {
                T result = defaultValue;
                if (sourceType == destinationType && (originalSourceType == originalDestinationType || originalDestinationType == typeof(object)))
                {
                    return (T)Convert.ChangeType(value, destinationType);
                }
                else
                {
                    if (NullableToString(value) == string.Empty)
                    {
                        return defaultValue;
                    }

                    if (sourceType == typeof(string) || sourceType == typeof(String))
                    {
                        value = (S)Convert.ChangeType(value, typeof(String));
                        if (destinationType == typeof(DateTime))
                            return (T)Convert.ChangeType(ConvertGenericString(NullableToString(value)), destinationType);
                        else if (destinationType == typeof(DateTimeOffset))
                            return (T)Convert.ChangeType(SteringToDatetimeOffset(NullableToString(value)), destinationType);
                        else if (destinationType.IsEnum && EnumUtils.TryGetEnumFromString<T>(NullableToString(value), out result))
                        {
                            return result;
                        }
                        else if (IsSimple(destinationType))
                        {
                            return (T)Convert.ChangeType(value, destinationType);
                        }
                        else return JsonConvertTo<T>(NullableToString(value));
                    }
                    else return JsonConvertTo<T, S>(value);
                }
            }
            catch (Exception e)
            {

                throw e;
            }
        }

        public static bool IsSimple(Type typeData)
        {
            typeData = GetType(typeData);
            return typeData.IsPrimitive
              || typeData.IsEnum
              || typeData.Equals(typeof(string))
              || typeData.Equals(typeof(decimal))
              || typeData.Equals(typeof(Decimal))
              || typeData.Equals(typeof(String))
              || typeData.Equals(typeof(BigInteger));
        }

        public static bool IsAbleConvertFromString(Type type) => TypeDescriptor.GetConverter(type).CanConvertFrom(typeof(string));

        public static T JsonConvertTo<T>(String value, JsonSerializerSettings settings = null)
        {
            var Settings = settings ?? new JsonSerializerSettings() { MaxDepth = 100, NullValueHandling = NullValueHandling.Include, TypeNameHandling = TypeNameHandling.None };
            Settings.Converters.Add(new MultiIsoDateTimeStringReadConverter());
            return JsonConvert.DeserializeObject<T>(value, Settings);
        }

        public static T JsonConvertTo<T, S>(S value)
        {
            var valueType = Common.Helpers.Utils.GetType(value.GetType());
            if(valueType == typeof(string) || valueType == typeof(String))
                return JsonConvertTo<T>(NullableToString(value));

            var Settings = new JsonSerializerSettings() { MaxDepth = 100, NullValueHandling = NullValueHandling.Include, TypeNameHandling = TypeNameHandling.None };
            Settings.Converters.Add(new MultiIsoDateTimeStringReadConverter());
            var strJson = JSONUtils.ConvertToJson(value, Settings);
            return JsonConvert.DeserializeObject<T>(strJson, Settings);
        }

        public static T JsonConvertTo<T>(object value)
        {
            return JsonConvertTo<T, object>(value);
        }


        public static IEnumerable<T> JsonConvertTo<T, S>(List<S> value)
        {
            if(!(value != null && value.Any())) return new List<T>();
            var tasks = value.Select(d => Task.Run<T>(() => { return Task.FromResult<T>(JsonConvertTo<T, S>(d)); })).ToArray();
            Task.WaitAll(tasks);
            return tasks.Select(d => d.Result).ToList();

        }

        public static IEnumerable<T> JsonConvertTo<T>(List<object> value)
        {
            return JsonConvertTo<T, object>(value);

        }

        /// <summary>
        /// Chech if the value exists
        /// </summary>
        /// <returns>True or False</returns>
        public static bool HasValue(string value)
        {
            return !string.IsNullOrEmpty(value) && !string.IsNullOrWhiteSpace(value);
        }

        public static bool IsValid<T>(IEnumerable<T> obj)
        {
            try
            {
                return obj != null && obj.Any();
            }
            catch (ArgumentNullException exc)
            {
                return false;
            }
            catch (Exception exc)
            {
                return false;
            }
        }

        public static string DateToString(DateTime date, string format = "")
        {
            if (!HasValue(format)) format = Globals.DefaultLongDateFormat;
            return date.ToString(format, CultureInfo.InvariantCulture);
        }

        public static bool IsNullable(Type type)
        {
            if (!type.IsValueType)
                return true;

            return Nullable.GetUnderlyingType(type) != null;
        }

        public static Type GetOriginalType(Type type)
        {
            if (!type.IsValueType)
                return type;

            return IsGenericList(type) ? GetGenericListType(type) : GetType(type);
        }

        public static Type GetType(Type type)
        {
            if (!type.IsValueType)
                return type;

            return IsNullable(type) ? Nullable.GetUnderlyingType(type) : type;
        }

        public static string NullableToString<T>(T data, string format = "", string NullableReplaceText = "")
        {

            if (IsNullable(typeof(T)) && data == null)
            {
                 return NullableReplaceText;
            }

            if (typeof(DateTime) == data.GetType())
            {
                format = string.IsNullOrEmpty(format) ? Globals.DefaultDateFormat : format;
                return DateToString((DateTime)Convert.ChangeType(data, typeof(DateTime)), format);
            }

            if (typeof(Decimal) == data.GetType() || typeof(decimal) == data.GetType())
            {
                format = string.IsNullOrEmpty(format) ? Globals.NoDecimalPointFormat : format;
                return ((Decimal)Convert.ChangeType(data, typeof(Decimal))).ToString(format);
            }


            return data.ToString();
        }

        public static Type GetObjectType(Expression<Func<object>> expr)
        {
            if (expr.Body is MemberExpression)
            {
                var obj = ((MemberExpression)expr.Body).Expression;
                return (Type)((PropertyInfo)obj.GetType()
                .GetProperty("Type", BindingFlags.Instance | BindingFlags.Public)).GetValue(obj);
            }
            else
            {

                var obj = ((UnaryExpression)expr.Body).Operand;
                return (Type)((PropertyInfo)obj.GetType()
                .GetProperty("Type", BindingFlags.Instance | BindingFlags.Public)).GetValue(obj);
            }
        }

        public static bool IsGenericList(object o)
        {
            try
            {
                var oType = GetObjectType(() => o);
                return IsGenericList(oType);
            }
            catch (Exception ex)
            {
                return false;
            }
        }


        public static bool IsGenericList(Type oType)
        {
            return (oType.IsGenericType && (oType.GetGenericTypeDefinition() == typeof(List<>)));
        }

        public static bool IsGenericDictionary(object o)
        {
            var oType = o.GetType();
            return (oType.IsGenericType && (oType.GetGenericTypeDefinition() == typeof(Dictionary<,>)));
        }

        public static bool IsGenericDictionary(Type oType)
        {
            return (oType.IsGenericType && (oType.GetGenericTypeDefinition() == typeof(Dictionary<,>)));
        }

        public static Type GetGenericListType(object abc)
        {
            Type type = abc.GetType().GetGenericArguments()[0];
            return type;
        }

        public static Type GetGenericListType(Type abc)
        {
            Type type = abc.GetGenericArguments()[0];
            return type;
        }

        public static object GetDefaultValue(Type type)
        {
            if (Common.Helpers.Utils.IsNullable(type))
                return null;

            return type.IsValueType ? Activator.CreateInstance(type) : null;
        }

        /// <summary>
        /// Returns a value indicating whether the object is null or equal to default value of its type.
        /// </summary>
        /// <param name="obj">Object to evaluate.</param>
        /// <returns>True if the object is null or equal to default value of its type, otherwise false</returns>
        public static bool IsNullOrDefault(object obj)
        {
            return obj == null || obj.Equals(GetDefaultValue(obj.GetType()));
        }

        /// <summary>
        /// Safely converts enumeration to Lists with nullable and empty checking
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="model"></param>
        /// <param name="nullable"></param>
        /// <returns></returns>
        public static List<T> EnumToList<T>(IEnumerable<T> model, bool nullable = false)
        {
            try
            {
                if (IsNullOrDefault(model)) return nullable ? null : new List<T>();
                if (model == null) return new List<T>();
                return model.ToList();
            }
            catch (ArgumentNullException exc)
            {
                throw exc;
            }
            catch (Exception exc)
            {
                throw exc;
            }
            return nullable ? null : new List<T>();
        }

        /// <summary>
        /// Safely converts enumeration to Lists with nullable and empty checking
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="model"></param>
        /// <param name="nullable"></param>
        /// <returns></returns>
        public static List<T> EnumToList<T>(ConcurrentBag<T> model, bool nullable = false)
        {
            try
            {
                if (IsNullOrDefault(model)) return nullable ? null : new List<T>();
                if (model == null) return new List<T>();
                return model.ToList();
            }
            catch (ArgumentNullException exc)
            {
                throw exc;
            }
            catch (Exception exc)
            {
                throw exc;
            }
            return nullable ? null : new List<T>();
        }

        public static List<T> EnumToList<T>(IQueryable<T> model, bool nullable = false)
        {
            try
            {
                if (IsNullOrDefault(model.AsEnumerable())) return nullable ? null : new List<T>();
                return model.ToList();
            }
            catch (ArgumentNullException exc)
            {
                throw exc;
            }
            catch (Exception exc)
            {
                throw exc;
            }
            return nullable ? null : new List<T>();
        }

        
    }
}
