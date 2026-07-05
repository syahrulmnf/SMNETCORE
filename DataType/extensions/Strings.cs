using System;
using System.IO;
using System.Text.RegularExpressions;
using SMNETCORE.Common.Helpers;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Linq;
using SMNETCORE.Logging;
using SMNETCORE.Common.Enums;
using Newtonsoft.Json;
using SMNETCORE.Common;
using NeoSmart.Utils;

namespace SMNETCORE.DataType.Extensions
{
    public static class Strings
    {
        public static string Base64Encode(this string plainText)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return UrlBase64.Encode(plainTextBytes, PaddingPolicy.Discard);
        }

        public static string Base64Decode(this string base64EncodedData)
        {
            var base64EncodedBytes = UrlBase64.Decode(base64EncodedData);
            return Encoding.UTF8.GetString(base64EncodedBytes);
        }

        private static readonly Regex NameExpression = new Regex("([A-Z]+(?=$|[A-Z][a-z])|[A-Z]?[a-z]+)", RegexOptions.Compiled);
        /// <summary>
        /// Compares the strings base on "CaseSetsitiveSearch" application settings. This function returns true for comparison of null and empty strings.
        /// </summary>
        /// <param name="lhs">Source string.</param>
        /// <param name="rhs">String to compare.</param>
        /// <returns>True if they are equal, otherwise false.</returns>
        public static bool IsEqualTo(this string lhs, string rhs)
        {
            lhs = lhs ?? String.Empty;
            rhs = rhs ?? String.Empty;

            return rhs.Equals(lhs, AppSettings.CaseSetsitiveSearch ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Compares the strings base on "CaseSetsitiveSearch" application settings. This method returns false if the input parameters are null or empty.
        /// </summary>
        /// <param name="lhs">Source string.</param>
        /// <param name="rhs">String to compare.</param>
        /// <returns>True if they are equal, otherwise false.This method returns false if the input parameters are null or empty.</returns>
        public static bool IsNotEmptyAndEqualTo(this string lhs, string rhs)
        {
            return !String.IsNullOrEmpty(lhs) && !String.IsNullOrEmpty(rhs) && lhs.Equals(rhs, AppSettings.CaseSetsitiveSearch ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Returns true if the string is matched with the specified pattern.
        /// </summary>
        /// <param name="value">String to evaluate.</param>
        /// <param name="regularExpressionPattern">Regular expression pattern.</param>
        /// <returns>True if the string is matched with the specified pattern, otherwise false.</returns>
        public static bool IsMatch(this string value, string regularExpressionPattern)
        {
            var regex = new Regex(regularExpressionPattern);
            return regex.IsMatch(value);
        }

        /// <summary>
        /// Returns a value indicating whether the string is a relative path.
        /// </summary>
        /// <param name="path">Path to evaluate.</param>
        /// <returns>True if the path is relative, otherwise False.</returns>
        public static bool IsRelativePath(this string path)
        {
            return !path.IsMatch("^[a-zA-Z]{1}:\\.*");
        }

        /// <summary>
        /// Converts the path to an absolute path if it's relative, otherwise returns the path itself.
        /// </summary>
        /// <param name="path">The path to convert.</param>
        /// <returns>Absolute path, based on the execution path of the application.</returns>
        public static string ToAbsolutePath(this string path)
        {
            return path.IsRelativePath()
                ? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, path)
                : path;
        }

        /// <summary>
        /// Chech if the value exists
        /// </summary>
        /// <returns>True or False</returns>
        public static bool HasValue(this string value)
        {
            return Common.Helpers.Utils.HasValue(value);
        }

        /// <summary>
        /// Chech if the value exists
        /// </summary>
        /// <returns>True or False</returns>
        public static bool HasStringValue(this string value)
        {
            return Serializer.Utils.HasStringValue(value);
        }

        public static bool HasStringValue(this string value,bool allowEmptyString)
        {
            return Serializer.Utils.HasStringValue(value, allowEmptyString);
        }

        /// <summary>
        /// Get Default if empty, Default strig empty
        /// </summary>
        /// <returns>Empty or Default</returns>
        public static string GetDefaultIfEmpty(this string value, string defaultValue = "")
        {
            if (defaultValue == null) defaultValue = string.Empty;
            return !string.IsNullOrEmpty(value) ? value : defaultValue;
        }

        /// <summary>
        /// Returns a value indicating whether the specified path is a directory.
        /// </summary>
        /// <param name="path">Path to evaluate.</param>
        /// <returns>True if the path is directory, otherwise false.</returns>
        public static bool IsDirectory(this string path, bool isNetworkDrive)
        {
            var attr = File.GetAttributes(path);
            return (attr & FileAttributes.Directory) == FileAttributes.Directory;
        }

        /// <summary>
        /// Replaces single quotes from a string with double quoutes.
        /// </summary>
        /// <param name="input">Input string.</param>
        /// <returns></returns>
        public static string EscapeQuotes(this string input)
        {
            const string QUOTE = "\"";
            const string ESCAPED_QUOTE = "\"\"";

            if (String.IsNullOrEmpty(input))
                return QUOTE + QUOTE;

            if (input.Contains(QUOTE))
                input = input.Replace(QUOTE, ESCAPED_QUOTE);

            return QUOTE + input + QUOTE;
        }

        public static string FormatFromDictionary(this string formatString, Dictionary<string, object> ValueDict)
        {
            if (string.IsNullOrEmpty(formatString)) return formatString;

            int i = 0;
            StringBuilder newFormatString = new StringBuilder(formatString);
            Dictionary<string, int> keyToInt = new Dictionary<string, int>();
            foreach (var tuple in ValueDict)
            {
                newFormatString = newFormatString.Replace("{" + tuple.Key + "}", "{" + i.ToString() + "}");
                keyToInt.Add(tuple.Key, i);
                i++;
            }
            return String.Format(newFormatString.ToString(), ValueDict.OrderBy(x => keyToInt[x.Key]).Select(x => x.Value.NullableToString()).ToArray());
        }
        public static string FormatFromDictionary(this string formatString, Dictionary<string, string> ValueDict)
        {
            if (string.IsNullOrEmpty(formatString)) return formatString;
            var dc = ValueDict.ToDictionary(dk => dk.Key, dv => (object)dv.Value);
            return FormatFromDictionary(formatString, dc);
        }

        public static string FormatFromModel(this string formatString, object model)
        {
            if (string.IsNullOrEmpty(formatString)) return formatString;

            StringBuilder newFormatString = new StringBuilder(formatString);
            Dictionary<int, string> ValueDict = new Dictionary<int, string>();
            Regex r = new Regex(@"{(.+?)}");
            MatchCollection mc = r.Matches(formatString);
            for (int index = 0; index < mc.Count; index++)
            {
                newFormatString = newFormatString.Replace(mc[index].Groups[1].Value, index.ToString());
                ValueDict.Add(index, model.GetPropertyValue(mc[index].Groups[1].Value).ToString());
            }
            string formatStringNew = newFormatString.ToString();
            string[] dataMap = ValueDict.OrderBy(data => data.Key).Select(x => x.Value).ToArray();
            string result = String.Format(formatStringNew, dataMap);
            return result;
        }

        public static int Count(this string formatString, params char[] separator)
        {
            if (string.IsNullOrEmpty(formatString)) return -1;
            return formatString.Split(separator).Count();
        }

        public static string DecodeUTF8toUnicode(this string utf8String)
        {
            // read the string as UTF-8 bytes.
            byte[] encodedBytes = Encoding.UTF8.GetBytes(utf8String);

            // convert them into unicode bytes.
            byte[] unicodeBytes = Encoding.Convert(Encoding.UTF8, Encoding.Unicode, encodedBytes);

            // builds the converted string.
            return Encoding.Unicode.GetString(encodedBytes);
        }

        public static string DecodeUnicodeToUTF8(this string unicodeString)
        {
            // read the string as UTF-8 bytes.
            byte[] encodedBytes = Encoding.Unicode.GetBytes(unicodeString);

            // convert them into unicode bytes.
            byte[] unicodeBytes = Encoding.Convert(Encoding.Unicode, Encoding.UTF8, encodedBytes);

            // builds the converted string.
            return Encoding.UTF8.GetString(encodedBytes);
        }

        public static string DecodeUTF8toUnicode(this char[] utf8String)
        {
            // read the string as UTF-8 bytes.
            byte[] encodedBytes = Encoding.UTF8.GetBytes(utf8String);

            // convert them into unicode bytes.
            byte[] unicodeBytes = Encoding.Convert(Encoding.UTF8, Encoding.Unicode, encodedBytes);

            // builds the converted string.
            return Encoding.Unicode.GetString(encodedBytes);
        }

        public static string DecodeUnicodeToUTF8(this char[] unicodeString)
        {
            // read the string as UTF-8 bytes.
            byte[] encodedBytes = Encoding.Unicode.GetBytes(unicodeString);

            // convert them into unicode bytes.
            byte[] unicodeBytes = Encoding.Convert(Encoding.Unicode, Encoding.UTF8, encodedBytes);

            // builds the converted string.
            return Encoding.UTF8.GetString(encodedBytes);
        }

        public static string AsTitle(this string value)
        {
            if (value == null)
            {
                return string.Empty;
            }

            int lastIndex = value.LastIndexOf(".", StringComparison.Ordinal);

            if (lastIndex > -1)
            {
                value = value.Substring(lastIndex + 1);
            }

            return value.SplitPascalCase();
        }

        public static string SplitPascalCase(this string value)
        {
            return NameExpression.Replace(value, " $1").Trim();
        }

        public static string TrimString(this string value)
        {
            return !value.HasValue() ? string.Empty : value.Trim();
        }

        public static string GetValueEmptyIfNull(this string data)
        {
            return string.IsNullOrEmpty(data) ? string.Empty : data;
        }

        public static string GetFixedString(this string value, int number)
        {
            if (string.IsNullOrEmpty(value)) return string.Empty;

            int realCount = value.ToCharArray().Count();
            if (realCount <= number) return value;
            return value.Substring(0, number);
        }

        public static string GetFileName(this string name)
        {
            return Logging.Utils.GetFileName(name);
        }

        public static List<T> SplitGroupsCombinedTo<T>(this string data, char[] combinedSparator, char[] groupSparator)
        {
            try
            {
                if (combinedSparator == null || combinedSparator.Count() == 0) combinedSparator = Globals.GroupCombinedSparator;
                if (groupSparator == null || groupSparator.Count() == 0) groupSparator = Globals.GroupSparator;
                var dataSplit = data.Split(combinedSparator).EnumToList();
                var dataGroup = dataSplit.SelectMany(d => d.Split(groupSparator).EnumToList());
                var dataConvert = dataGroup.Select(d => d.To<T>().Value).EnumToList();
                return dataConvert;
            }
            catch (Exception exc)
            {
                Logger.LogError(exc, LogCategoryType.Common);
                throw exc;
            }
        }

    
        public static List<T> SplitGroupsTo<T>(this string data, params char[] groupSparator)
        {
            try
            {
                if (data == null || data == "") return new List<T>();
                if (groupSparator == null || groupSparator.Count() == 0) groupSparator = Globals.GroupSparator;
                var dataSplit = data.Split(groupSparator).EnumToList();
                var dataConvert = dataSplit.Select(d => d.To<T>().Value).EnumToList();
                return dataConvert;
            }
            catch (Exception exc)
            {
                Logger.LogError(exc, LogCategoryType.Common);
                throw exc;
            }
        }

        public static Dictionary<string, string> FormDictionaryURL(this string dataString)
        {
            try
            {
                if (!dataString.HasValue() || !dataString.Contains("=")) return new Dictionary<string, string>();
                if (dataString.Contains("?")) dataString = dataString.Split('?')[1];
                
                var dataKey = dataString.Split('&')
                    .Select(data => new KeyValuePair<string, string>(data.Split('=')[0], data.Split('=')[1]))
                    .GroupBy(data => data.Key)
                    .Where(d => d.Key.HasValue())
                    .DistinctBy(d => d.Key)
                    .Select(dataDist => new KeyValuePair<string, string>(dataDist.Key, dataDist.FirstOrDefault().Value)).ToList();
                var stringDiction = dataKey.ToDictionary(dk => dk.Key, dv => dv.Value ?? string.Empty);
                return stringDiction;
            }
            catch (Exception exc)
            {
                Logger.LogError(exc, LogCategoryType.Common);

            }
            return new Dictionary<string, string>();
        }

        public static Dictionary<string, object> DictionaryFromURL(this string dataString)
        {
            return DictionaryFromURL(dataString, 0, 3);
        }

        public static Dictionary<string, object> DictionaryFromURL(this string dataString, int idxStart, int maxDepth)
        {
            try
            {
                if (!dataString.HasValue() || !dataString.Contains("=")) return new Dictionary<string, object>();
                if (dataString.Contains("?")) dataString = dataString.Split('?')[1];

                var dataKey = dataString.Split('&')
                    .Select(data => new KeyValuePair<string, string>(data.Split('=')[0], data.Split('=')[1]))
                    .GroupBy(data => data.Key)
                    .Where(d => d.Key.HasValue())
                    .DistinctBy(d => d.Key)
                    .Select(dataDist => new KeyValuePair<string, object>(dataDist.Key, dataDist.FirstOrDefault().Value)).EnumToList();

                dataKey = dataKey.Where(d => d.Value.NullableToString().HasStringValue()).EnumToList();
                var dataKeyObject = dataKey.Where(d => d.Value.NullableToString().Contains("=") && d.Value.NullableToString().StartsWith("\"") && d.Value.NullableToString().EndsWith("\"")
                    && d.Value.NullableToString().StartsWith("'") && d.Value.NullableToString().EndsWith("'")).EnumToList();

                if (dataKeyObject.IsValid() && idxStart < maxDepth)
                {
                    var keyString = dataKeyObject.Select(d => d.Key).EnumToList();
                    dataKey = dataKey.Where(d => !keyString.Contains(d.Key)).EnumToList();
                    var newDataKey = dataKeyObject.Select(d => new KeyValuePair<string, object>(d.Key, d.Value.NullableToString().DictionaryFromURL(idxStart++, maxDepth))).EnumToList();
                    if (newDataKey.IsValid()) dataKey.AddRange(newDataKey);
                }
                var stringDiction = dataKey.ToDictionary(dk => dk.Key, dv => dv.Value);
                return stringDiction;
            }
            catch (Exception exc)
            {
                Logger.LogError(exc, LogCategoryType.Common);

            }
            return new Dictionary<string, object>();
        }


        public static List<string> SplitEmail(this string address)
        {
            try
            {
                if (!address.HasValue()) return new List<string>();
                var result = address.Split(Globals.EmailSeparator)
                    .Where(d => d.HasValue())
                    .Select(d => d.Trim()).EnumToList();
                return result;
            }
            catch(Exception exc)
            {
                Logger.LogError(exc, LogCategoryType.Common);
                throw exc;
            }
        }

       
        public static dynamic Deserealized(this string Results)
        {
            var res = JsonConvert.DeserializeObject<dynamic>(Results);

            return res;
        }

        public static string RemoveInvalidChars(this string filename, string replacement = "-")
        {
            return string.Join(replacement, filename.Split(Path.GetInvalidFileNameChars()));
        }

        public static string JoinText<T>(this IEnumerable<T> data, string delimiter = ",", string format = "", string nullReplacement = "")
        {
            if (!data.IsValid()) return string.Empty;
            if (data.Count() == 1) return data.FirstOrDefault().NullableToString();
            var dataStr = data.Select(d => d.NullableToString(format, nullReplacement));
            return string.Join(delimiter, dataStr);
        }
        
    }
}
