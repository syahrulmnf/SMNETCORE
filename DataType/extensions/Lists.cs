using SMNETCORE.Common.Enums;
using SMNETCORE.Logging;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Dynamic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace SMNETCORE.DataType.Extensions
{
    public static class Lists
    {
        public static void AddRange<T>(this ConcurrentBag<T> data, IEnumerable<T> addData)
        {
            Serializer.Utils.AddRange<T>(data, addData);
        }


        public static IList<T> Clone<T>(this IList<T> listToClone) where T : ICloneable
        {
            return listToClone.IsValid() ? listToClone.Select(item => (T)item.Clone()).ToList() : new List<T>();
        }

        public static bool IsValid<T>(this IEnumerable<T> obj)
        {
            try
            {
                return Common.Helpers.Utils.IsValid(obj);
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

        public static void AddAndReplaceIfExisted<TKey, TVal>(this IDictionary<TKey, TVal> Model, TKey Key, TVal Val)
        {
            Model = Model ?? new Dictionary<TKey, TVal>();
            if (!Model.Any() || !Model.ContainsKey(Key))
            {
                Model.Add(Key, Val);
            }
            else
            {
                Model[Key] = Val;
            }
        }

        public static void AddRange<T>(this T Model, T fromModel) where T : IList<T>, IEnumerable<T>, ICollection<T>
        {
            Serializer.Utils.AddRange<T>(Model, fromModel);
        }

        public static List<List<KeyValuePair<object, object>>> ToDictionaryPropertiesValueList(this IEnumerable<object> Data, List<string> Props)
        {
            List<List<KeyValuePair<object, object>>> dataValues = new List<List<KeyValuePair<object, object>>>();
            if (Data == null) return dataValues;
            foreach (var d in Data)
            {
                var val = d.ToDictionaryPropertiesValue(Props).Select(dd => new KeyValuePair<object, object>(dd.Key, dd.Value)).EnumToList();
                dataValues.Add(val);
            }
            return dataValues;
        }

        public static IEnumerable<T> CopyListToList<T>(this IEnumerable<T> data)
        {
            return Serializer.Utils.CopyListToList<T>(data);
        }

        public static ConcurrentBag<T> CopyListToList<T>(this ConcurrentBag<T> data)
        {
            return Serializer.Utils.CopyListToList<T>(data);
        }

        public static bool Compares<T>(this IEnumerable<T> data, T compareData, string[] properties)
        {
            if (!data.IsValid() || compareData == null) return false;

            return data.Any(d => d.Compares(compareData, properties));
        }

        public static List<T> CheckContains<T>(this IEnumerable<T> data, IEnumerable<T> checkData, string[] properties)
        {
            if (!data.IsValid() || !checkData.IsValid()) return (data ?? checkData).EnumToList();

            List<T> left = new List<T>();
            foreach (var d in data)
            {
                if (properties == null || !properties.Any())
                {
                    if (!checkData.Contains(d))
                    {
                        left.Add(d);
                    }
                }
                else if (!checkData.Compares(d, properties))
                {
                    left.Add(d);
                }
            }

            List<T> left2 = new List<T>();
            foreach (var d in checkData)
            {
                if (properties == null || !properties.Any())
                {
                    if (!data.Contains(d) && !left.Contains(d))
                    {
                        left2.Add(d);
                    }
                }
                else if (!data.Compares(d, properties) && !left.Compares(d, properties))
                {
                    left2.Add(d);
                }
            }

            left.AddRange(left2);
            return left;
        }

        public static List<T> GetRange<T>(this IEnumerable<T> data, int idx, int range)
        {
            return Serializer.Utils.GetRange<T>(data, idx, range);
        }

        public static IEnumerable<IEnumerable<T>> SplitIEnumerable<T>(this IEnumerable<T> locations, int nSize = 30, object lockData = null)
        {
            return Serializer.Utils.SplitList<T>(locations, nSize, lockData);
        }

        public static IEnumerable<IEnumerable<DataRow>> SplitIEnumerable(this DataTable locations, int nSize = 30, object lockData = null)
        {

            lockData = lockData ?? new object();
            lock (lockData)
            {
                return locations.AsEnumerable().Select((s, i) => new { Value = s, Index = i })
                     .GroupBy(item => item.Index / nSize, item => item.Value).EnumToList();
            }

        }

        public static List<List<T>> SplitList<T>(this IEnumerable<T> locations, int nSize = 30, object lockData = null)
        {
            return Serializer.Utils.SplitList<T>(locations, nSize, lockData);
        }

        public static List<T> MapFromDataRow<T>(this List<T> model, DataTable data) where T : class, new()
        {
            var results = new List<T>();
            if (model != null && model.Any()) results.AddRange(model);
            try
            {
                foreach (DataRow dTRw in data.AsEnumerable())
                {
                    var dModel = new T();
                    dModel.MapFromDataRow(dTRw);
                    results.Add(dModel);
                }
            }
            catch (Exception exc)
            {
                Logger.LogError(exc, LogCategoryType.Common);
            }
            return results;
        }

        public static T DictionaryToObject<T>(this Dictionary<String, Object> dictionary) where T : class
        {
            return ToDictionaryToObject(dictionary) as T;
        }

        private static dynamic ToDictionaryToObject(Dictionary<String, Object> dictionary)
        {
            var expandoObj = new ExpandoObject();
            var expandoObjCollection = (ICollection<KeyValuePair<String, Object>>)expandoObj;

            foreach (var keyValuePair in dictionary)
            {
                expandoObjCollection.Add(keyValuePair);
            }
            dynamic eoDynamic = expandoObj;
            return eoDynamic;
        }

        public static Dictionary<K, V> ToDictionary<K, V>(this List<KeyValuePair<K, V>> data)
        {
            return data.GroupBy(d => d.Key).ToDictionary(dk => dk.Key, dv => dv.First().Value);
        }

        //public static Dictionary<K, V> ToDictionary<K, V>(this IEnumerable<KeyValuePair<K, V>> data)
        //{
        //    return data.EnumToList().ToDictionary();
        //}

        public static string ToStringUrl<tKey, tVal>(this Dictionary<tKey, tVal> data)
        {
            if (data == null || !data.Any()) return string.Empty;
            return String.Join("&", data.Select(dataStr => String.Format("{0}={1}", dataStr.Key.NullableToString(), dataStr.Value.NullableToString())));
        }

        public static bool IsNull<T, TU>(this KeyValuePair<T, TU> pair)
        {
            return pair.Equals(new KeyValuePair<T, TU>());
        }

        public static IDictionary<TKey, TVal> MergeDictionaries<TKey, TVal>(this IDictionary<TKey, TVal> Model, IDictionary<TKey, TVal> fromModel, bool OverwriteValueOnKeyExists = false)
        {
            foreach (var data in fromModel)
            {
                if (!Model.ContainsKey(data.Key)) Model.Add(data);
                else if (OverwriteValueOnKeyExists && data.Value.NullableToString() != string.Empty)
                {
                    Model[data.Key] = data.Value;
                }
            }
            return Model;
        }

        public static IDictionary<K, V> AddRange<K, V>(this IDictionary<K, V> data, IEnumerable<KeyValuePair<K, V>> additional)
        {
            additional.ForEachEnumerable(d =>
            {
                if (data.ContainsKey(d.Key)) data[d.Key] = d.Value;
                else data.Add(d.Key, d.Value);
            });
            return data;
        }

        public static ConcurrentDictionary<K, V> ToConcurrentDictionary<K, V>(this IEnumerable<KeyValuePair<K, V>> data)
        {
            return new ConcurrentDictionary<K, V>
            (data.EnumToList().ToDictionary());
        }

        public static ConcurrentDictionary<K, V> ToConcurrentDictionary<K, V>(this List<KeyValuePair<K, V>> data)
        {
            return new ConcurrentDictionary<K, V>
            (data.GroupBy(d => d.Key).ToDictionary(dk => dk.Key, dv => dv.First().Value));
        }

        public static ConcurrentDictionary<TKey, TElement> ToConcurrentDictionary<TSource, TKey, TElement>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector)
        {
            return new ConcurrentDictionary<TKey, TElement>
            (source.ToDictionary(keySelector, elementSelector));
        }

        public static ConcurrentDictionary<TKey, TSource> ToConcurrentDictionary<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
        {
            return new ConcurrentDictionary<TKey, TSource>
            (source.ToDictionary(keySelector, d => d));
        }
        public static ConcurrentDictionary<TKey, TSource> ToConcurrentDictionary<TKey, TSource>(this NameValueCollection nvc)
        {
            return new ConcurrentDictionary<TKey, TSource>
            (nvc.AllKeys.Where(d => d.HasStringValue()).ToDictionary(k => k.To<TKey>().Value, k => nvc[k].To<TSource>(default(TSource)).Value));
        }

        public static Dictionary<string, string> ToDictionary(this NameValueCollection nvc)
        {
            return nvc.AllKeys.Where(d => d.HasStringValue()).ToDictionary(k => k, k => nvc[k]);
        }
        public static ConcurrentDictionary<string, string> ToConcurrentDictionary(this NameValueCollection nvc)
        {
            return new ConcurrentDictionary<string, string>(nvc.ToDictionary());
        }

        public static List<TVal> ToListModel<TKey, TVal>(this IDictionary<TKey, TVal> Model)
        {
            List<TVal> data = new List<TVal>();
            foreach (var dataModel in Model)
            {
                if (!data.Contains(dataModel.Value)) data.Add(dataModel.Value);
            }
            return data;
        }

        public static List<TVal> ToListModel<TKey, TVal>(this IDictionary<TKey, List<TVal>> Model)
        {
            List<TVal> data = new List<TVal>();
            foreach (var dataModel in Model)
            {
                foreach (var dataVal in dataModel.Value)
                {
                    if (!data.Contains(dataVal)) data.Add(dataVal);
                }
            }
            return data;
        }

        #region Linq
        //public static IEnumerable<TSource> DistinctBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
        //{
        //    return source.GroupBy(keySelector).Select(x => x.First());
        //}

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T">Model type</typeparam>
        /// <param name="sequence"></param>
        /// <param name="action"></param>
        public static void ForEachIndex<T>(this IEnumerable<T> sequence, Action<int, T> action)
        {
            if (!sequence.IsValid()) sequence = new List<T>();
            // argument null checking omitted
            int i = 0;
            foreach (T item in sequence)
            {
                action(i, item);
                i++;
            }
        }

        public static void ForEachEnumerable<T>(this IEnumerable<T> sequence, Action<T> action)
        {
            if (!sequence.IsValid()) sequence = new List<T>();
            // argument null checking omitted
            foreach (T item in sequence)
            {
                action(item);
            }
        }

        public static void ForEachEnumerable<T>(this IEnumerable<T> sequence, Action<T, int> action)
        {
            if (!sequence.IsValid()) sequence = new List<T>();
            // argument null checking omitted
            int idx = 0;
            foreach (T item in sequence)
            {
                action(item, idx++);
            }
        }

        public static void ForEachEnumerable<T>(this ICollection sequence, Action<T> action)
        {
            if (sequence == null) sequence = new List<T>();
            // argument null checking omitted
            foreach (T item in sequence)
            {
                action(item);
            }
        }

        public static void ForEachEnumerable<T>(this ICollection sequence, Action<T, int> action)
        {
            if (sequence == null) sequence = new List<T>();
            // argument null checking omitted
            int idx = 0;
            foreach (T item in sequence)
            {
                action(item, idx++);
            }
        }

        public static void ForEachEnumerable<T>(this ICollection<T> sequence, Action<T> action)
        {
            if (sequence == null) sequence = new List<T>();
            // argument null checking omitted
            foreach (T item in sequence)
            {
                action(item);
            }
        }

        public static void ForEachEnumerable<T>(this ICollection<T> sequence, Action<T, int> action)
        {
            if (sequence == null) sequence = new List<T>();
            // argument null checking omitted
            int idx = 0;
            foreach (T item in sequence)
            {
                action(item, idx++);
            }
        }

        public static List<KeyValuePair<TKey, TElement>> ToKeyValuePairs<TSource, TKey, TElement>(this IEnumerable<TSource> sequence,
            Func<TSource, TKey> keySelector,
            Func<TSource, TElement> elementSelector)
        {
            if (!sequence.IsValid()) return new List<KeyValuePair<TKey, TElement>>();
            sequence = sequence.DistinctBy(keySelector);
            var dictionary = sequence.ToDictionary(keySelector, elementSelector);
            var results = dictionary.EnumToList();
            return results;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="source">IEnumerable<Object></param>
        /// <param name="propName">Property Name</param>
        /// <param name="dir">Directions -1 Desc, 1 Asc, Else Desc</param>
        /// <returns></returns>
        public static IEnumerable<TSource> OrderByPropertyName<TSource>(this IEnumerable<TSource> source, string propName, int dir = -1, bool mustHaveValue = false)
        {
            if (String.IsNullOrEmpty(propName) || source == null || !source.Any() || !source.FirstOrDefault().HasProperty(propName)) return source;

            if (mustHaveValue)
            {
                source = source.Where(data => !data.GetPropertyValue(propName).IsNull());
            }

            if (dir == -1)
            {
                source = source.OrderByDescending(data => data.GetPropertyValue(propName));
            }
            else
            {
                source = source.OrderBy(data => data.GetPropertyValue(propName));
            }

            return source;
        }

        public static IEnumerable<TSource> OrderByPropertyName<TSource>(this IEnumerable<TSource> source, KeyValuePair<string, SortMethod> listOrder,
          bool mustHaveValue = false, bool orderedByMustHaveValue = false)
        {
            return OrderByPropertyName<TSource>(source, new List<KeyValuePair<string, SortMethod>>() { listOrder },
            mustHaveValue, orderedByMustHaveValue);
        }

        /// <summary>
        /// list Order key value pair of column name and order type -1 desc, 1 asc
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <param name="source"></param>
        /// <param name="listOrder"></param>
        /// <returns></returns>
        /// 
        public static IEnumerable<TSource> OrderByPropertyName<TSource>(this IEnumerable<TSource> source, List<KeyValuePair<string, SortMethod>> listOrder,
           bool mustHaveValue = false, bool orderedByMustHaveValue = false)
        {
            return OrderByPropertyName<TSource>(source, listOrder.Select(d => new KeyValuePair<string, int>(d.Key, (int)d.Value)).EnumToList(),
                mustHaveValue, orderedByMustHaveValue);
        }

        public static IEnumerable<TSource> OrderByPropertyName<TSource>(this IEnumerable<TSource> source, KeyValuePair<string, int> listOrder,
            bool mustHaveValue = false, bool orderedByMustHaveValue = false)
        {
            return OrderByPropertyName<TSource>(source, new List<KeyValuePair<string, int>>() { listOrder },
            mustHaveValue, orderedByMustHaveValue);
        }

        public static IEnumerable<TSource> OrderByPropertyName<TSource>(this IEnumerable<TSource> source, List<KeyValuePair<string, int>> listOrder, 
            bool mustHaveValue = false, bool orderedByMustHaveValue = false)
        {
            bool isOrderd = false;
            if (listOrder == null || !listOrder.Any() || source == null || !source.Any()) return source;
            IOrderedEnumerable<TSource> data = null;
            if (mustHaveValue)
            {
                ConcurrentBag<TSource> fltered = new ConcurrentBag<TSource>();
                var tmp = source.EnumToConcurrentBag();
                Parallel.ForEach(tmp, d =>
                {
                    var tmpData = listOrder.Select(o => d.GetPropertyValue(o.Key));
                    var dataValues = listOrder.Select(o => d.GetPropertyValue(o.Key)).Any(op => op.IsNull());
                    if (!dataValues) fltered.Add(d);
                });
                source = fltered.EnumToList();
            }

            if (orderedByMustHaveValue)
            {

                foreach (KeyValuePair<string, int> order in listOrder)
                {
                    if (!source.FirstOrDefault().HasProperty(order.Key)) continue;
                    if (!isOrderd && (data == null || !data.Any()))
                    {
                        data = source.OrderByDescending(dataResult => !dataResult.GetPropertyValue(order.Key).IsNull());
                    }
                    else
                    {
                        data = data.ThenByDescending(dataResult => !dataResult.GetPropertyValue(order.Key).IsNull());
                    }
                    isOrderd = true;
                }
            }

            foreach (KeyValuePair<string, int> order in listOrder)
            {
                if (!source.FirstOrDefault().HasProperty(order.Key)) continue;

                if ((SortMethod)order.Value == SortMethod.Descending)
                {
                    if (!isOrderd && (data == null || !data.Any()))
                    {
                        data = source.OrderByDescending(dataResult => dataResult.GetPropertyValue(order.Key));
                    }
                    else
                    {
                        data = data.ThenByDescending(dataResult => dataResult.GetPropertyValue(order.Key));
                    }
                }
                else
                {
                    if (!isOrderd && (data == null || !data.Any()))
                    {
                        data = source.OrderBy(dataResult => dataResult.GetPropertyValue(order.Key));
                    }
                    else
                    {
                        data = data.ThenBy(dataResult => dataResult.GetPropertyValue(order.Key));
                    }
                }
                isOrderd = true;
            }
            return data;
        }

        public static IEnumerable<IGrouping<TKey, TSource>> GroupBy<TSource, TKey>(this IEnumerable<TSource> source, string namegroup)
        {
            return source.GroupBy(d => d.GetPropertyValue(namegroup).To<TKey>().Value);
        }

        public static IEnumerable<IGrouping<string, TSource>> GroupBy<TSource>(this IEnumerable<TSource> source, List<string> groupLists)
        {
            return source.GroupBy(d => groupLists.Select(k => d.GetPropertyValue(k).ToString()).JoinText());
        }


        #endregion

        /// <summary>
        /// Safely converts enumeration to Lists with nullable and empty checking
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="model"></param>
        /// <param name="nullable"></param>
        /// <returns></returns>
        public static List<T> EnumToList<T>(this IEnumerable<T> model, bool nullable = false)
        {
            try
            {
                return Common.Helpers.Utils.EnumToList<T>(model, nullable);
            }
            catch (ArgumentNullException exc)
            {
                Logger.LogError("EnumToList Error - Return Default", exc, LogCategoryType.Common);
                return nullable ? null : new List<T>();
            }
            catch (Exception exc)
            {
                Logger.LogError("EnumToList Error - ", exc, LogCategoryType.Common);
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
        public static List<T> EnumToList<T>(this ConcurrentBag<T> model, bool nullable = false)
        {
            try
            {
                return Common.Helpers.Utils.EnumToList<T>(model, nullable);
            }
            catch (ArgumentNullException exc)
            {
                Logger.LogError("EnumToList Error - Return Default", exc, LogCategoryType.Common);
                return nullable ? null : new List<T>();
            }
            catch (Exception exc)
            {
                Logger.LogError("EnumToList Error - ", exc, LogCategoryType.Common);
                throw exc;
            }
            return nullable ? null : new List<T>();
        }

        public static List<T> EnumToList<T>(this IQueryable<T> model, bool nullable = false)
        {
            try
            {
                return Common.Helpers.Utils.EnumToList<T>(model, nullable);
            }
            catch (ArgumentNullException exc)
            {
                Logger.LogError("EnumToList Error - Return Default", exc, LogCategoryType.Common);
                return new List<T>();
            }
            catch (Exception exc)
            {
                Logger.LogError("EnumToList Error - ", exc, LogCategoryType.Common);
                throw exc;
            }
            return nullable ? null : new List<T>();
        }
        public static ConcurrentBag<T> EnumToConcurrentBag<T>(this IEnumerable<T> data)
        {
            var _data = new ConcurrentBag<T>();
            if (data.IsValid()) _data.AddRange(data);
            return _data;
        }
        public static ConcurrentBag<T> EnumToConcurrentBag<T>(this IList<T> data)
        {
            var _data = new ConcurrentBag<T>();
            if (data.IsValid()) _data.AddRange(data);
            return _data;
        }

        public static ConcurrentBag<T> EnumToConcurrentBag<T>(this IQueryable<T> data)
        {
            var _data = new ConcurrentBag<T>();
            if (data.IsValid()) _data.AddRange(data);
            return _data;
        }

        public static T Last<T>(this IEnumerable<T> data, int lastIndexOf)
        {
            if (!data.IsValid()) return default(T);
            if (data.Count() >= lastIndexOf) return default(T);
            var paramT = data.EnumToList();
            return paramT[data.Count() - lastIndexOf];
        }

        public static IEnumerable<T> SkipLast<T>(this IEnumerable<T> data, int lastIndexOf)
        {
            if (!data.IsValid()) return null;
            if (data.Count() >= lastIndexOf) return null;
            var numberOfData = data.Count();
            if (numberOfData <= lastIndexOf) return null;

            numberOfData -= lastIndexOf;

            return data.Where((d, i) => i < numberOfData).Select(d => d).EnumToList();


        }

        public static IEnumerable<T> TakeLast<T>(this IEnumerable<T> data, int lastIndexOf)
        {
            if (!data.IsValid()) return null;
            if (data.Count() >= lastIndexOf) return null;
            var numberOfData = data.Count();
            if (numberOfData <= lastIndexOf) return data;

            numberOfData -= lastIndexOf;

            return data.Where((d, i) => i >= numberOfData).Select(d => d).EnumToList();
        }

        public static List<T> ReverseList<T>(this IEnumerable<T> data)
        {
            return data.Reverse().EnumToList();
        }
    }
}
