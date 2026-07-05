using System.Reflection;
using Newtonsoft.Json.Linq;
using System.Globalization;
using SMNETCORE.Logging;
using System.ComponentModel;
using System.Data;
using System.Text;
using Newtonsoft.Json;
using SMNETCORE.DataType.Exceptions;
using System.Collections.Concurrent;
using SMNETCORE.Common;

namespace SMNETCORE.DataType.Extensions
{


    public static class Objects
    {
        private static object Lock1_Object = new object();
        #region Get/Set Property Value


        public static string NullableToString<T>(this T data, string format = "", string NullableReplaceText = "")
        {
            return Common.Helpers.Utils.NullableToString(data, format, NullableReplaceText);
        }


        public static T ReturnDefaultOverride<T>(this T data)
        {
            if (Common.Helpers.Utils.IsNullable(typeof(T)) && data != null && !data.Equals(Globals.Organisation.NullableReplacement.To<T>().Value)) return data;
            if (!data.Equals(Globals.Organisation.NullableReplacement.To<T>().Value)) return data;

            return default(T);
        }

        /// <summary>
        /// Returns the value of the property specified by the property name. This method returns null if the object is null, the property does not exist or is not readable.
        /// </summary>
        /// <param name="obj">Source object.</param>
        /// <param name="propertyName">Property to evaluate.</param>
        /// <returns>The value of the property or null if the object is null, the property does not exist or is not readable. </returns>
        public static object GetPropertyValue(this object obj, string propertyName)
        {
            var root = obj.GetPropertyRoot(propertyName);
            if (root != null)
            {
                var propertyInfo = root.GetPropertyInfo(propertyName);
                return propertyInfo != null && propertyInfo.CanRead ? propertyInfo.GetValue(root, null) : null;
            }

            return null;
        }

        public static T GetPropertyValue<T>(this object obj, string propertyName)
        {
            return obj.GetPropertyValue(propertyName).To<T>().Value;
        }

        public static MethodInfo GetFunction(this Type data, string funcName)
        {
            List<MethodInfo> methodInfos = data.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static).ToList();
            var m = methodInfos.FirstOrDefault(d => d.Name == funcName);
            return m;
        }

        public static FieldInfo GetFieldInfo(this Type data, string funcName, BindingFlags binding = BindingFlags.Public)
        {
            FieldInfo methodInfos = data.GetFields().FirstOrDefault(d => d.Name == funcName);

            return methodInfos;
        }

        public static PropertyInfo GetPropertyInfo(this Type data, string funcName, BindingFlags binding = BindingFlags.Public)
        {
            PropertyInfo methodInfos = data.GetProperties().FirstOrDefault(d => d.Name == funcName);

            return methodInfos;
        }

        /// <summary>
        /// Sets value of the property specified by propery name and returns an instance of ConversionResult class.
        /// </summary>
        /// <param name="obj">Target object.</param>
        /// <param name="propertyName">Property name to set.</param>
        /// <param name="propertyValue">Property value.</param>
        /// <returns>An instance of ConversionResult class.</returns>
        public static ConversionResult<T> SetPropertyValue<T>(this object obj, string propertyName, T propertyValue)
        {
            var result = new ConversionResult<T> { Value = propertyValue, PropertyType = typeof(T) };
            try
            {
                if (!obj.HasProperty(propertyName))
                {
                    result.Success = false;
                    result.Value = default(T);
                    result.Error = new Exception("Property Not Found: " + propertyName);
                    return result;
                }

                var root = obj.GetPropertyRoot(propertyName);
                var propertyInfo = root.GetPropertyInfo(propertyName);
                if (propertyInfo.CanWrite)
                {
                    propertyInfo.SetValue(root, propertyValue, null);
                    result.Success = true;
                }
                else
                {
                    result.Error = new InvalidOperationException(String.Format("Property {0} is not writable.", propertyName));
                    result.Value = default(T);
                }

                return result;
            }
            catch (Exception e)
            {
                result.Success = false;
                result.Value = default(T);
                result.Error = e;
                return result;
            }
        }



        /// <summary>
        /// Sets value of the property specified by propery name and returns an instance of ConversionResult class.
        /// </summary>
        /// <param name="obj">Target object.</param>
        /// <param name="values">Property name to set, Property value.</param>
        /// <returns>An instance of ConversionResult class.</returns>
        public static ConversionResult<T> SetPropertyValue<T>(this T obj, List<KeyValuePair<string, object>> values)
        {
            var result = new ConversionResult<T> { Value = obj, PropertyType = typeof(T) };
            try
            {
                foreach (var prop in values)
                {
                    if (!obj.HasProperty(prop.Key)) continue;

                    var root = obj.GetPropertyRoot(prop.Key);
                    var propertyInfo = root.GetPropertyInfo(prop.Key);
                    if (propertyInfo.CanWrite)
                    {
                        var destinationType = Nullable.GetUnderlyingType(propertyInfo.PropertyType) ?? propertyInfo.PropertyType;
                        if (prop.Value != null)
                        {
                            var val = Convert.ChangeType(prop.Value, destinationType);
                            propertyInfo.SetValue(root, val, null);
                        }
                    }
                }
                return result;
            }
            catch (Exception e)
            {
                Logger.LogError(e, LogCategoryType.Common);
                result.Success = false;
                result.Value = obj == null ? default(T) : obj;
                result.Error = e;
                return result;
            }
        }

        /// <summary>
        /// Sets value of the property specified by propery name and returns an instance of ConversionResult class.
        /// </summary>
        /// <param name="obj">Target object.</param>
        /// <param name="values">Property name to set, Property value.</param>
        /// <returns>An instance of ConversionResult class.</returns>
        public static ConversionResult<T> SetPropertyValue<T>(this T obj, Dictionary<string, object> values)
        {
            return obj.SetPropertyValue(values.ToList());
        }


        /// <summary>
        /// Sets the property value of the object specified by the property name parameter and returns an instance of ConversionResult class.
        /// </summary>
        /// <param name="obj">Target Object.</param>
        /// <param name="propertyName">Property name.</param>
        /// <param name="propertyValue">Property value.</param>
        /// <param name="setDefaultOnFailure">If true, the method will set the property value to default value of the property if conversion failed.</param>
        /// <returns>An instance of ConversionResult class.</returns>
        public static bool TrySetPropertyValue<O>(this O obj, string propertyName, string propertyValue, bool setDefaultOnFailure, bool allowEmptyString)
        {
            if (!obj.HasProperty(propertyName) || !propertyValue.HasStringValue(allowEmptyString)) return false;
            try
            {
                var root = obj.GetPropertyRoot(propertyName);

                var propertyInfo = root.GetPropertyInfo(propertyName);
                if (!propertyInfo.CanWrite) return false;

                var destinationType = Common.Helpers.Utils.GetType(propertyInfo.PropertyType);

                if(destinationType.Equals(typeof(DateTime)))
                {
                    DateTime date;
                    if (propertyValue.TryConvertGenericString(out date))
                    {

                        propertyInfo.SetValue(root, date, null);
                        return true;
                    }
                }



                try
                {
                    //Note: The following line is because Convert.ChangeType() fails for nullable types.
                    var convertedValue = Convert.ChangeType(propertyValue, destinationType);
                    propertyInfo.SetValue(root, convertedValue, null);
                    return true;
                }
                catch (Exception e)
                {

                    try
                    {
                        if (setDefaultOnFailure)
                        {
                            var defaultValue = Common.Helpers.Utils.GetDefaultValue(propertyInfo.PropertyType);
                            propertyInfo.SetValue(root, defaultValue, null);
                        }
                    }
                    catch (Exception innerException)
                    {
                        throw innerException;
                    }

                    throw e;
                }


            }
            catch (Exception e)
            {
                Logger.LogError(e, LogCategoryType.Common);
                return false;
            }
        }

        /// <summary>
        /// Sets the property value of the object specified by the property name parameter and returns an instance of ConversionResult class.
        /// </summary>
        /// <param name="obj">Target Object.</param>
        /// <param name="propertyName">Property name.</param>
        /// <param name="propertyValue">Property value.</param>
        /// <param name="setDefaultOnFailure">If true, the method will set the property value to default value of the property if conversion failed.</param>
        /// <returns>An instance of ConversionResult class.</returns>
        public static ConversionResult<object> SetPropertyValue(this object obj, string propertyName, object propertyValue, bool setDefaultOnFailure)
        {
            if (!obj.HasProperty(propertyName)) return new ConversionResult<object> { Value = propertyValue, Success = false, Error = new Exception("Property " + propertyName + " was not found in " + obj.ToString()) };
            var result = new ConversionResult<object> { Value = propertyValue };
            try
            {
                var root = obj.GetPropertyRoot(propertyName);

                var propertyInfo = root.GetPropertyInfo(propertyName);
                var propertyType = propertyInfo.PropertyType;
                result.PropertyType = propertyType;

                if (propertyType.Equals(typeof(DateTime)) && (obj.GetType().Equals(typeof(string)) || obj.GetType().Equals(typeof(String))))
                {
                    DateTime date;
                    if (propertyValue.NullableToString().TryConvertGenericString(out date))
                    {

                        propertyInfo.SetValue(root, date, null);
                        result.Success = true;
                        return result;
                    }
                }

                if (propertyInfo.CanWrite)
                {
                    try
                    {
                        //Note: The following line is because Convert.ChangeType() fails for nullable types.
                        var destinationType = Nullable.GetUnderlyingType(propertyInfo.PropertyType) ?? propertyInfo.PropertyType;
                        var convertedValue = Convert.ChangeType(propertyValue, destinationType);
                        propertyInfo.SetValue(root, convertedValue, null);
                        result.Success = true;
                    }
                    catch (Exception e)
                    {
                        result.Success = false;
                        result.Error = e;

                        try
                        {
                            if (setDefaultOnFailure)
                            {
                                var defaultValue = Common.Helpers.Utils.GetDefaultValue(propertyInfo.PropertyType);
                                result.Value = defaultValue;
                                propertyInfo.SetValue(root, defaultValue, null);
                            }
                        }
                        catch (Exception innerException)
                        {
                            result.Error = innerException;
                        }
                    }
                }
                else
                    result.Error = new InvalidOperationException(String.Format("Property {0} is not writable.", propertyName));

                return result;
            }
            catch (Exception e)
            {
                Logger.LogError(e, LogCategoryType.Common);
                return new ConversionResult<object> { Value = propertyValue, Success = false, Error = new Exception("Property " + propertyName + " was not found in " + obj.ToString()) };
            }

        }

        /// <summary>
        /// Sets the property value of the object specified by the property name parameter and returns an instance of ConversionResult class.
        /// </summary>
        /// <param name="obj">Target Object.</param>
        /// <param name="propertyName">Property name.</param>
        /// <param name="propertyValue">Property value.</param>
        /// <param name="setDefaultOnFailure">If true, the method will set the property value to default value of the property if conversion failed.</param>
        /// <returns>An instance of ConversionResult class.</returns>
        public static ConversionResult<object> SetPropertyValue(this object obj, PropertyInfo propertyInfo, object propertyValue, bool setDefaultOnFailure)
        {
            var result = new ConversionResult<object> { Value = propertyValue };
            try
            {

                result.PropertyType = propertyInfo.PropertyType;

                if (propertyInfo.CanWrite)
                {
                    var defaultValue = Common.Helpers.Utils.GetDefaultValue(propertyInfo.PropertyType);

                    try
                    {
                        //Note: The following line is because Convert.ChangeType() fails for nullable types.
                        var destinationType = Nullable.GetUnderlyingType(propertyInfo.PropertyType) ?? propertyInfo.PropertyType;
                        if (propertyValue == null && !Common.Helpers.Utils.IsNullable(destinationType))
                        {
                            result.Success = false;
                            result.Error = new Exception("Non Nullable but been set null value");
                            return result;
                        }

                        var convertedValue = Convert.ChangeType(propertyValue, destinationType);
                        propertyInfo.SetValue(obj, convertedValue, null);
                        result.Value = convertedValue;
                        result.Success = true;
                    }
                    catch (Exception e)
                    {
                        result.Success = false;
                        result.Error = e;
                        Logger.LogError(e, LogCategoryType.Common);

                        try
                        {
                            if (setDefaultOnFailure)
                            {

                                result.Value = defaultValue;
                                propertyInfo.SetValue(obj, defaultValue, null);
                            }
                        }
                        catch (Exception innerException)
                        {
                            Logger.LogError(innerException, LogCategoryType.Common);
                            result.Error = innerException;
                        }
                    }
                }
                else
                    result.Error = new InvalidOperationException(String.Format("Property {0} is not writable.", propertyInfo.Name));

                return result;
            }
            catch (Exception e)
            {
                Logger.LogError(e, LogCategoryType.Common);
                result.Error = e;
                result.Success = false;
                return result;
            }

        }

        #endregion



        #region General Methods

        /// <summary>
        /// Returns a value indicating whether the object is null or equal to default value of its type.
        /// </summary>
        /// <param name="obj">Object to evaluate.</param>
        /// <returns>True if the object is null or equal to default value of its type, otherwise false</returns>
        public static bool IsNullOrDefault(this object obj)
        {
            return Common.Helpers.Utils.IsNullOrDefault(obj);
        }

        public static bool EqualObject<T>(this T obj, T compare)
        {
            return EqualityComparer<T>.Default.Equals(obj, compare);
        }

        /// <summary>
        /// Returns a value indicating whether the object is null. This method returns false for value types.
        /// </summary>
        /// <param name="obj">Object to evaluate.</param>
        /// <returns>True if the object is null, otherwise false.</returns>
        public static bool IsNull(this object obj)
        {
            return obj == null;
        }


        #endregion

        #region Private Methods

        public static PropertyInfo GetPropertyInfo(this object root, string propertyName, bool iterateOnce = false)
        {
            try
            {
                var sourceType = root.GetType();
                var path = propertyName.Split('.').Last();
                var propertyInfo = iterateOnce ? sourceType.GetProperties().FirstOrDefault(d => d.Name.Equals(propertyName)) : sourceType.GetProperty(path);

                if (propertyInfo == null)
                    throw new PropertyNotFoundException(String.Format("The object does not have a property named {0}.", propertyName));

                return propertyInfo;
            }
            catch (AmbiguousMatchException ambExc)
            {
                if (!iterateOnce)
                {
                    Logger.LogError(ambExc, LogCategoryType.Common);
                    return GetPropertyInfo(root, propertyName, true);
                }
                throw ambExc;
            }
            catch (Exception exc)
            {
                throw exc;
            }
        }

        public static bool HasProperty(this object obj, string propertyName)
        {
            if (obj == null || propertyName == null) return false;
            var sourceType = obj.GetType();
            if (propertyName.Contains("."))
            {
                var propertyInfo = sourceType.GetProperty(propertyName.Split('.')[0]);

                if (propertyInfo == null)
                    return false;
                List<string> properties = propertyName.Split('.').ToList();
                properties.RemoveAt(0);
                return propertyInfo.GetValue(obj).HasProperty(string.Join(".", properties));
            }
            else
            {
                var propertyInfo = sourceType.GetProperty(propertyName);

                if (propertyInfo == null)
                    return false;
                return true;
            }

        }

        private static object GetPropertyRoot(this object obj, string propertyName)
        {
            if (obj == null)
                throw new NullReferenceException("Source object is null.");

            var parts = propertyName.Split('.');
            var root = obj;

            if (parts.Length > 1)
            {
                parts = parts.TakeWhile((p, i) => i < parts.Length - 1).ToArray();
                var nextLevelPath = String.Join(".", parts);
                root = obj.GetPropertyValue(nextLevelPath);
            }

            return root;
        }

        #endregion


        public static int GetCount<TSource>(this IEnumerable<TSource> source)
        {
            if (!source.IsValid()) return -1;
            return source.Count();
        }



        public static bool TryUpdate<K, V>(this ConcurrentDictionary<K, V> data, K key, V updatedData)
        {
            V existing;
            data.TryGetValue(key, out existing);
            return data.TryUpdate(key, updatedData, existing);
        }

        public static T CopyObject<T>(this T obj) where T : new()
        {
            var newObj = SMNETCORE.Common.Helpers.Utils.JsonConvertTo<T, T>(obj);
            return newObj;
        }

        public static Type GetPropertyType(this Type type)
        {
            return SMNETCORE.Common.Helpers.Utils.GetType(type);
        }

        public static T MergeModel<T>(this T ToModel, object FromModel)
        {
            if (ToModel == null || FromModel == null) return ToModel;
            try
            {
                Type fromModelType = FromModel.GetType();
                Type ToModelType = ToModel.GetType();

                List<PropertyInfo> fromProperties = fromModelType.GetProperties().Where(d => d.CanWrite && d.CanRead).DistinctBy(d => d.Name).ToList();
                var propertyNames = fromProperties.Select(d => d.Name).EnumToList();
                List<PropertyInfo> toProperties = ToModelType.GetProperties().Where(d => propertyNames.Contains(d.Name) && d.CanWrite && d.CanRead).ToList();

                foreach (PropertyInfo fromProp in fromProperties)
                {
                    var toSources = toProperties.Where(d => d.Name == fromProp.Name && d.PropertyType.GetPropertyType() == fromProp.PropertyType.GetPropertyType()).EnumToList();
                    if (toSources.IsValid())
                    {
                        var valueProp = fromProp.GetValue(FromModel, null);
                        foreach (var ppSource in toSources)
                        {
                            if (ppSource.CanWrite && ppSource.CanRead && (ppSource.IsNull() || ppSource.PropertyType == fromProp.PropertyType || (!ppSource.IsNull() && valueProp != null)))
                            {
                                try
                                {
                                    ppSource.SetValue(ToModel, valueProp);
                                }
                                catch (Exception exc)
                                {
                                    Logger.LogError(exc, LogCategoryType.Common);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception exc)
            {
                Logger.LogError(exc, LogCategoryType.Common);
            }
            return ToModel;
        }

        public static Tvalue TryGetValueData<Tkey, Tvalue>(this Dictionary<Tkey, Tvalue> data, Tkey key)
        {
            Tvalue value = default(Tvalue);
            if (data.TryGetValue(key, out value)) return value;
            return default(Tvalue);
        }
        public static bool TryGetValueData<Tkey, Tvalue>(this Dictionary<Tkey, Tvalue> data, Tkey key, out Tvalue tvalue)
        {
            tvalue = default(Tvalue);
            if (data.TryGetValue(key, out tvalue))
            {
                return true;
            }
            return false;
        }


        public static IDictionary<string, string> PropertiesToDictionary(this object metaToken)
        {
            if (metaToken == null)
            {
                return null;
            }

            JToken token = metaToken as JToken;
            if (token == null)
            {
                return PropertiesToDictionary(JObject.FromObject(metaToken));
            }

            if (token.HasValues)
            {
                var contentData = new Dictionary<string, string>();
                foreach (var child in token.Children().ToList())
                {

                    var childContent = child.PropertiesToDictionary();
                    if (childContent != null)
                    {
                        contentData = contentData.Concat(childContent)
                                                 .ToDictionary(k => k.Key, v => v.Value);
                    }
                }

                return contentData;
            }

            var jValue = token as JValue;
            if (jValue == null || jValue.Value == null)
            {
                return null;
            }

            var value = jValue.Type == JTokenType.Date ?
                            jValue.ToString(Globals.TimeZone.StandardDateTimeFormat, CultureInfo.InvariantCulture) :
                            jValue.ToString(CultureInfo.InvariantCulture);

            return new Dictionary<string, string> { { token.Path, value } };
        }

        public static byte[] ReadFully(this Stream input, long? size = null)
        {
            byte[] buffer = new byte[size ?? 16 * 1024];
            try
            {
                if (input == null || input.Length == 0) return null;

                using (MemoryStream ms = new MemoryStream())
                {
                    int read;
                    input.Position = 0;

                    while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        ms.Write(buffer, 0, read);
                    }
                    return ms.ToArray();
                }
            }
            catch (Exception exc)
            {
                Logger.LogError(exc, LogCategoryType.Common);
            }
            return buffer;
        }

        public static Dictionary<string, object> AnonymousObjectToDictionary(this object obj)
        {
            return TypeDescriptor.GetProperties(obj)
                .OfType<PropertyDescriptor>()
                .Where(prop => !prop.Attributes.OfType<JsonIgnoreAttribute>().Any())
                .ToDictionary(
                    prop => prop.Name,
                    prop => prop.GetValue(obj)
                );
        }

        public static void MapFromDataRow<T>(this T model, DataRow data) where T : class, new()
        {
            try
            {
                var mdDictionary = model.AnonymousObjectToDictionary();
                model = model ?? new T();
                var dtDict = data.Table.Columns
                              .Cast<DataColumn>()
                              .ToDictionary(c => c.ColumnName, c => data[c]);
                model.SetPropertyValue(dtDict);
            }
            catch (Exception exc)
            {
                Logger.LogError(exc, LogCategoryType.Common);
            }
        }


        public static StringBuilder[] SplitString(this StringBuilder input, char separator)
        {
            List<StringBuilder> results = new List<StringBuilder>();

            StringBuilder current = new StringBuilder();
            for (int i = 0; i < input.Length; ++i)
            {
                if (input[i] == separator)
                {
                    results.Add(current);
                    current = new StringBuilder();
                }
                else
                    current.Append(input[i]);
            }

            if (current.Length > 0)
                results.Add(current);

            return results.ToArray();
        }

        public static StringBuilder[] SplitString(this StringBuilder input, int line = 1500)
        {
            List<StringBuilder> results = new List<StringBuilder>();
            if (input == null || input.Length == 0) return null;

            List<List<String>> str = input.ToString().Split('\n').EnumToList().SplitList(line);

            StringBuilder current = new StringBuilder();

            foreach (var dataStr in str)
            {

                dataStr.ForEach(dt =>
                {
                    current.Append(dt);
                });
                results.Add(current);
                current = new StringBuilder();
            }

            if (current.Length > 0)
                results.Add(current);

            return results.ToArray();
        }

        public static KeyValuePair<string, MemoryStream>[] GetAttachment(this StringBuilder input, int line = 1500)
        {
            List<KeyValuePair<string, MemoryStream>> results = new List<KeyValuePair<string, MemoryStream>>();
            if (input == null || input.Length == 0) return null;

            var dSplittedErrors = input.SplitString(line);
            int i = 1;
            foreach (var dErrorsData in dSplittedErrors)
            {
                var dErrorSplitter = dErrorsData.SplitString();
                foreach (var dErrors in dErrorSplitter)
                {
                    var historyStream = new MemoryStream();
                    var historyWriter = new StreamWriter(historyStream);

                    historyWriter.WriteLine(dErrors.ToString());
                    historyWriter.Flush();

                    if (historyStream.Length > 0)
                    {
                        historyStream.Position = 0;
                        results.Add(new KeyValuePair<string, MemoryStream>("ERROR-" + i + ".txt", historyStream));
                        i++;
                    }
                }

            }

            return results.ToArray();
        }




        public static String Extract<T>(this T data, string messageSr = "", bool needFullReport = true) where T : Exception, new()
        {
            return Serializer.Utils.Extract<T>(data, messageSr, needFullReport);
        }

        public static bool IsGenericList(this object o)
        {
            return Common.Helpers.Utils.IsGenericList(o);
        }

        public static bool IsGenericList(this Type oType)
        {
            return Common.Helpers.Utils.IsGenericList(oType);
        }

        public static bool IsGenericDictionary(this object o)
        {
            return Common.Helpers.Utils.IsGenericDictionary(o);
        }

        public static bool IsGenericDictionary(this Type oType)
        {
            return Common.Helpers.Utils.IsGenericDictionary(oType);
        }

        public static Type GetGenericListType(this object abc)
        {
            return Common.Helpers.Utils.GetGenericListType(abc);
        }

        public static Type GetGenericListType(this Type abc)
        {
            return Common.Helpers.Utils.GetGenericListType(abc);
        }


        public static KeyValuePair<Type, Type> GetGenericDictionaryType(this object abc)
        {
            Type type = abc.GetType().GetGenericArguments()[0];
            Type type2 = abc.GetType().GetGenericArguments()[1];
            return new KeyValuePair<Type, Type>(type, type2);
        }

        public static KeyValuePair<Type, Type> GetGenericDictionaryType(this Type abc)
        {
            Type type = abc.GetGenericArguments()[0];
            Type type2 = abc.GetGenericArguments()[1];
            return new KeyValuePair<Type, Type>(type, type2);
        }

        public static MethodInfo GetGeneridMethodInfo(this Type ttData, string methodName, Type dataType)
        {
            var mi = ttData.GetMethod(methodName);
            var fooRef = mi.MakeGenericMethod(dataType);
            return fooRef;
        }

        public static bool Compares<T>(this T data, T compareData, string prop)
        {
            if (!data.HasProperty(prop) || !compareData.HasProperty(prop)) return false;
            return data.GetPropertyValue(prop).ToString() == compareData.GetPropertyValue(prop).ToString();
        }

        public static bool Compares<T>(this T data, T compareData, string[] properties)
        {
            if (data == null || compareData == null) return false;

            var propertiesCheck = properties.Where(d => data.Compares(compareData, d)).Select(d => d);
            return propertiesCheck != null && properties.Count() == propertiesCheck.Count();
        }



        public static List<KeyValuePair<string, object>> ToDictionaryPropertiesValue<T>(this T Data, List<string> Props)
        {
            List<KeyValuePair<string, object>> dataValues = new List<KeyValuePair<string, object>>();
            if (Data == null) return dataValues;
            if (!Props.IsValid()) Props = typeof(T).GetProperties().Select(d => d.Name).EnumToList();
            var propDuplicate = Props.GroupBy(d => d).Where(d => d.Count() > 1).EnumToList();
            if (propDuplicate.IsValid()) Logger.LogError("Duplicate Property Name ToDictionaryPropertiesValue" + propDuplicate.Distinct().JoinText(), LogCategoryType.Common);
            foreach (string prop in Props)
            {
                if (Data.HasProperty(prop))
                {
                    try
                    {
                        var val = Data.GetPropertyValue(prop);
                        dataValues.Add(new KeyValuePair<string, object>(prop, val));
                    }
                    catch (Exception exc)
                    {
                        Logger.LogError(exc, LogCategoryType.Common);
                        dataValues.Add(new KeyValuePair<string, object>(prop, string.Empty));
                    }
                }
                else
                {
                    dataValues.Add(new KeyValuePair<string, object>(prop, string.Empty));
                }
            }
            return dataValues;
        }

        public static void Add<Tkey, Tvalue>(this List<KeyValuePair<Tkey, Tvalue>> data, Tkey key, Tvalue value)
        {
            data = data ?? new List<KeyValuePair<Tkey, Tvalue>>();
            data.Add(new KeyValuePair<Tkey, Tvalue>(key, value));
        }

        public static T[] Merge<T>(this T[] data, T[] newData)
        {
            var tmp = data.EnumToList() ?? new List<T>();
            if(newData.IsValid()) tmp = tmp.Merge(newData.EnumToList());
            return tmp.ToArray();
        }

        public static List<T> Merge<T>(this List<T> data, List<T> newData)
        {
            data= data ?? new List<T>();
            if(newData.IsValid())
                data.AddRange(newData);
            return data;
        }

    }
}
