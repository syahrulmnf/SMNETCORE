using SMNETCORE.Common.Helpers;
using SMNETCORE.Logging;

namespace SMNETCORE.DataType.Extensions
{
    public static class Enums
    {
        public static string GetDescription<T>(this T enumValue) where T : Enum
        {
           return EnumUtils.GetDescription(enumValue);
        }

        public static IEnumerable<T> GetValues<T, E>(this E data) where E : Enum
        {
            return EnumUtils.GetValues<T, E>(data);
        }

        public static IEnumerable<T> GetValues<T>(this T data) where T : Enum
        {
            return EnumUtils.GetValues<T>(data);
        }

        public static IEnumerable<T> GetValues<T>() where T : Enum
        {
            return EnumUtils.GetValues<T>();
        }



        /// <summary>
        /// Returns Description attribute's value if exists, otherwise returns standard ToString() result.
        /// </summary>
        /// <param name="value">Enum</param>
        /// <returns>Description attribute's value if exists, otherwise standard ToString() result.</returns>
        public static string ToText(this Enum value)
        {
            try
            {
                return EnumUtils.ToText(value);
            }
            catch (Exception exc)
            {
                Logger.LogError(exc, LogCategoryType.Common);
                return string.Empty;
            }
        }

        public static Int32 ToInt32(this Enum value)
        {
            return EnumUtils.ToInt32(value);
        }

        public static Int64 ToInt64(this Enum value)
        {
            return EnumUtils.ToInt64(value);
        }

        public static Int16 ToInt16(this Enum value)
        {
            return EnumUtils.ToInt16(value);
        }

        public static Type GetEnumType(string enumName)
        {
            return EnumUtils.GetEnumType(enumName);
        }

       

        public static T GetEnumFromString<T>(this string str) where T : struct, IConvertible
        {
            try
            {
                return EnumUtils.GetEnumFromString<T>(str);
            }
            catch
            {
                return default(T);
            }
        }

        public static bool TryGetEnumFromString<T>(this string str, out T data) where T : struct, IConvertible
        {
            data = default(T);
            try
            {

                return EnumUtils.TryGetEnumFromString<T>(str, out data);
            }
            catch
            {
               
            }
            return false;
        }

        public static T GetEnumFromInt<T>(this int str) where T : struct, IConvertible
        {
            try
            {

                return EnumUtils.GetEnumFromInt<T>(str);
            }
            catch(Exception exc)
            {
                throw exc;
            }
        }
       


    }

    
}
