using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Serialization;
using System.Collections.Generic;
using SMNETCORE.Common.Enums;
using log4net;
using Newtonsoft.Json.Linq;
using System.Globalization;
using System.Dynamic;
using System.ComponentModel;
using System.Data;
using System.Text;
using Newtonsoft.Json;
using System.Linq.Expressions;
using SMNETCORE.DataType.Extensions;
using SMNETCORE.Logging;

namespace SMNETCORE.Common.Extensions
{
    public static class Json
    {
        public static T DeserealizeJsonObject<T>(this String Data, T Result, Dictionary<string, string> Properties) where T : new()
        {
            Result = Result == null ? new T() : Result;
            try
            {
                if (!Data.HasStringValue()) return Result;
                var jsonConvert = JObject.Parse(Data);

                foreach(var prop in Properties)
                {
                    var value = GetJOBJECTDataProperty(jsonConvert, prop.Key);
                    Result.SetPropertyValue(prop.Value, value);
                }
            }
            catch(Exception exc)
            {
                Logger.LogError(exc, LogCategoryType.Common);
            }

            return Result;
        }

        public static List<Dictionary<string, object>> DeserealizeJsonDictionaryList(this String Data, Dictionary<string, string> Properties)
        {
           var Result = new List<Dictionary<string, object>>();
            try
            {
                if (!Data.HasStringValue()) return Result;
                var jsonConvert = JObject.Parse(Data);
                int idx = 0;

                while(true)
                {
                    JToken jtokenData;
                    jsonConvert.TryGetValue(idx.NullableToString(), out jtokenData);
                    if (jtokenData == null || !jtokenData.HasValues) break;

                    var tempResult = new Dictionary<string, object>();
                    foreach (var prop in Properties)
                    {
                        var value = GetJOBJECTDataProperty(jtokenData, prop.Key);
                        tempResult.Add(prop.Value, value);
                    }

                    Result.Add(tempResult);
                    idx++;
                }
            }
            catch (Exception exc)
            {
                Logger.LogError(exc, LogCategoryType.Common);
            }

            return Result;
        }

        public static Dictionary<string, object> DeserealizeJsonDictionary(this String Data, Dictionary<string, object> Result, Dictionary<string, string> Properties)
        {
            Result = Result == null ? new Dictionary<string, object>() : Result;
            try
            {
                if (!Data.HasStringValue()) return Result;
                var jsonConvert = JObject.Parse(Data);

                foreach (var prop in Properties)
                {
                    var value = GetJOBJECTDataProperty(jsonConvert, prop.Key);
                    Result.Add(prop.Value, value);
                }
            }
            catch (Exception exc)
            {
                Logger.LogError(exc, LogCategoryType.Common);
            }

            return Result;
        }

        public static object GetJOBJECTDataProperty(JObject data, string property)
        {
            var propertiesLists = property.Split('.');
            JToken jtokenData = null;
            if (propertiesLists.IsValid() && propertiesLists.Count() > 1)
            {
                var firstProp = propertiesLists[0];
               
                data.TryGetValue(firstProp, out jtokenData);
                if(jtokenData != null && jtokenData.HasValues)
                {
                    return GetJOBJECTDataProperty(jtokenData, string.Join(",", propertiesLists.GetRange(1, propertiesLists.Count() - 1)));
                }
                return null;
            }
            else
            {
                data.TryGetValue(property, out jtokenData);
                return GetJOBJECTDataProperty(jtokenData, property);
            }
        }

        public static object GetJOBJECTDataProperty(JToken data, string property)
        {
            try
            {
                var propertiesLists = property.Split('.');
                if (propertiesLists.IsValid() && propertiesLists.Count() > 1)
                {
                    var firstProp = propertiesLists[0];
                    JToken jtokenData = data[firstProp];
                    if (jtokenData != null)
                    {
                        return GetJOBJECTDataProperty(jtokenData, string.Join(",", propertiesLists.GetRange(1, propertiesLists.Count() - 1)));
                    }
                    return null;
                }
                else
                {
                    var jtokenDataValue = data.Value<object>(property);
                    return jtokenDataValue;
                }
            }
            catch(Exception exc)
            {
                Logger.LogError(exc, LogCategoryType.Common);
            }
            return null;
        }
    }
}
