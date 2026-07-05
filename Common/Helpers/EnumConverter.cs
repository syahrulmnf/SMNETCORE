
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SMNETCORE.Common.Helpers
{
    public class EnumUtils
    {
        public static string GetDescription<T>( T enumValue) where T : Enum
        {
            FieldInfo _fieldInfo = enumValue.GetType().GetField(enumValue.ToString());

            if (_fieldInfo != null)
            {
                object[] _attributes = _fieldInfo.GetCustomAttributes(typeof(DescriptionAttribute), true);
                if (_attributes != null && _attributes.Length > 0)
                {
                    return ((DescriptionAttribute)_attributes[0]).Description;
                }
            }

            return string.Empty;
        }

        public static IEnumerable<T> GetValues<T, E>( E data) where E : Enum
        {
            return Enum.GetValues(typeof(E)).Cast<T>();
        }

        public static IEnumerable<T> GetValues<T>( T data) where T : Enum
        {
            return Enum.GetValues(typeof(T)).Cast<T>();
        }

        public static IEnumerable<T> GetValues<T>() where T : Enum
        {
            return Enum.GetValues(typeof(T)).Cast<T>();
        }
        public static IEnumerable<T> GetValues<T>(string enumName) where T : Enum
        {
            var type = EnumUtils.GetEnumType(enumName);
            return Enum.GetValues(type).Cast<T>();
        }

        public static IEnumerable<KeyValuePair<int, string>> GetValuesAndString<T>() where T : Enum
        {
            var type = typeof(T);
            var datas = Enum.GetValues(type).Cast<T>();
            var results = new List<KeyValuePair<int, string>>();
            
            foreach (var item in datas)
            {
                if (Enum.IsDefined(type, item))
                {
                    var value = (T)Enum.Parse(type, item.ToString(), true);
                    results.Add(new KeyValuePair<int, string>(ToInt32(value), ToText(value)));
                }
            }
            return results;
        }

        /// <summary>
        /// Returns Description attribute's value if exists, otherwise returns standard ToString() result.
        /// </summary>
        /// <param name="value">Enum</param>
        /// <returns>Description attribute's value if exists, otherwise standard ToString() result.</returns>
        public static string ToText(Enum value)
        {
            try
            {
                if (value == null) return string.Empty;
                var fi = value.GetType().GetField(value.ToString());
                if (fi == null) return string.Empty;

                var attributes = (DescriptionAttribute[])fi.GetCustomAttributes(typeof(DescriptionAttribute), false);
                return Common.Helpers.Utils.IsValid(attributes) && attributes.Length > 0 ? attributes[0].Description : value.ToString();
            }
            catch (Exception exc)
            {
                throw exc;
            }
        }

        public static Int32 ToInt32( Enum value)
        {
            if (!value.GetTypeCode().Equals(TypeCode.Int32)) return 0;
            return (Int32)Convert.ChangeType(value, value.GetTypeCode());
        }

        public static Int64 ToInt64( Enum value)
        {
            if (!value.GetTypeCode().Equals(TypeCode.Int64)) return 0;
            return (Int64)Convert.ChangeType(value, value.GetTypeCode());
        }

        public static Int16 ToInt16( Enum value)
        {
            if (!value.GetTypeCode().Equals(TypeCode.Int16)) return 0;
            return (Int16)Convert.ChangeType(value, value.GetTypeCode());
        }

        public static Type GetEnumType(string enumName)
        {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                var type = assembly.GetType(enumName);
                if (type == null)
                    continue;
                if (type.IsEnum)
                    return type;
            }
            return null;
        }



        public static T GetEnumFromString<T>( string str) where T : struct, IConvertible
        {
            try
            {
                T res = default(T);
                if (TryGetEnumFromString(str, out res)) return res;
                return res;
            }
            catch (Exception exc)
            {
                throw exc;
            }
        }

        public static bool TryGetEnumFromString<T>( string str, out T data)
        {
            data = default(T);
            try
            {

                T res = (T)Enum.Parse(typeof(T), str);
                if (!Enum.IsDefined(typeof(T), res)) return false;
                data = res;
            }
            catch (Exception exc)
            {
                throw exc;
            }
            return false;
        }

        public static T GetEnumFromInt<T>( int str) where T : struct, IConvertible
        {
            try
            {

                if (Enum.IsDefined(typeof(T), str))
                {
                    return (T)Enum.Parse(typeof(T), str.ToString(), true);
                }
                throw new Exception(str.ToString() + " is not member of " + typeof(T).FullName);
            }
            catch (Exception exc)
            {
                throw exc;
            }
        }
    }
    public class EnumConverter<TEnum> where TEnum : struct, IConvertible
    {
        Func<int, TEnum> _ConvertInt;
        public Func<int, TEnum> ConvertInt
        {
            get
            {
                _ConvertInt = _ConvertInt ?? GenerateIntConverter();
                return _ConvertInt;
            }
        }

        Func<string, TEnum> _ConvertStr;
        public Func<string, TEnum> ConvertStr
        {
            get
            {
                _ConvertStr = _ConvertStr ?? GenerateStringConverter();
                return _ConvertStr;
            }
        }

        private Func<int, TEnum> GenerateIntConverter()
        {
            var parameter = Expression.Parameter(typeof(int));
            var dynamicMethod = Expression.Lambda<Func<int, TEnum>>(
                Expression.Convert(parameter, typeof(TEnum)),
                parameter);
            return dynamicMethod.Compile();
        }

        private Func<string, TEnum> GenerateStringConverter()
        {
            var parameter = Expression.Parameter(typeof(string));
            var dynamicMethod = Expression.Lambda<Func<string, TEnum>>(
                Expression.Convert(parameter, typeof(TEnum)),
                parameter);
            return dynamicMethod.Compile();
        }

    }
}
