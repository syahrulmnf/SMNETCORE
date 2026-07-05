using SMNETCORE.Common;
using SMNETCORE.Common.Enums;
using SMNETCORE.Serializer.DTOModels;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMNETCORE.Serializer
{
    public class Utils
    {
        /// <summary>
        /// Chech if the value exists
        /// </summary>
        /// <returns>True or False</returns>
        public static bool HasStringValue(string value)
        {
            return !string.IsNullOrEmpty(value) && !string.IsNullOrWhiteSpace(value);
        }

        public static bool HasStringValue(string value,bool allowEmptyString)
        {
            if (!allowEmptyString) return HasStringValue(value);
            
            return value!= null;
        }

        public static string ToText(Enum value)
        {
            try
            {
                return Common.Helpers.EnumUtils.ToText(value);
            }
            catch (Exception exc)
            {
                return string.Empty;
            }
        }

        private static object GetRange_lock = new object();
        public static List<T> GetRange<T>(IEnumerable<T> data, int idx, int range)
        {
            lock (GetRange_lock)
            {
                return Common.Helpers.Utils.EnumToList(data).GetRange(idx, range);
            }
        }

        public static List<List<T>> SplitList<T>(IEnumerable<T> locations, int nSize = 30, object lockData = null)
        {
            
            var list = new List<List<T>>();
            if (!Common.Helpers.Utils.IsValid(locations)) return list;

            lockData = lockData ?? new object();
            lock (lockData)
            {
                for (int i = 0; i < locations.Count(); i += nSize)
                {
                    list.Add(GetRange(locations, i, Math.Min(nSize, locations.Count() - i)));
                }
            }

            return list;
        }

        public static void AddRange<T>(ConcurrentBag<T> data, IEnumerable<T> addData)
        {
            try
            {
                if (!Common.Helpers.Utils.IsValid(addData)) return;
                foreach (var d in addData)
                {
                    data.Add(d);
                }
            }
            catch (Exception exc)
            {

            }
        }

        public static void AddRange<T>(T Model, T fromModel) where T : IList<T>, IEnumerable<T>, ICollection<T>
        {
            foreach (var data in fromModel)
            {
                Model.Add(data);
            }
        }

        public static IEnumerable<T> CopyListToList<T>(IEnumerable<T> data)
        {
            if (!Common.Helpers.Utils.IsValid(data)) return new List<T>();
            var dtArray = new T[data.Count()];
            Common.Helpers.Utils.EnumToList(data).CopyTo(dtArray);
            var lstArray = Common.Helpers.Utils.EnumToList(dtArray.Select(d => d));
            return Common.Helpers.Utils.EnumToList(lstArray);
        }

        public static ConcurrentBag<T> CopyListToList<T>(ConcurrentBag<T> data)
        {
            if (!Common.Helpers.Utils.IsValid(data)) return new ConcurrentBag<T>();
            var dtArray = new T[data.Count()];
            data.CopyTo(dtArray, 0);
            var lstArray = new ConcurrentBag<T>();
            AddRange(lstArray, dtArray);
            return lstArray;
        }

        public static String Extract<T>(T data, string messageSr = "", bool needFullReport = true) where T : Exception, new()
        {
            
            ErrorClassJsonResponse messageClass = new ErrorClassJsonResponse();
            var setting = Globals.GenericHelper.JSONConvertsSetting(isObject: false);

            if (!string.IsNullOrEmpty(messageSr))
            {
                messageClass.Message = messageSr;
            }

            if (!String.IsNullOrEmpty(data.Message))
            {
                messageClass.ErrorMessage = data.Message;
            }

            if (needFullReport)
            {
                if (!String.IsNullOrEmpty(data.StackTrace))
                {
                    messageClass.StackTrace = data.StackTrace;
                }
                if (!String.IsNullOrEmpty(data.Source))
                {
                    messageClass.Source = data.Source;
                }
            }

            Exception inner = data.InnerException;
            if (inner != null && !Common.Helpers.Utils.IsValid(messageClass.InnerMessage)) messageClass.InnerMessage = new List<ErrorClassJson>();
            while (inner != null)
            {
                var msgTemp = new ErrorClassJson();
                if (!String.IsNullOrEmpty(data.Message))
                {
                    msgTemp.ErrorMessage = inner.Message;
                }

                if (needFullReport)
                {
                    if (!String.IsNullOrEmpty(data.StackTrace))
                    {
                        msgTemp.StackTrace = inner.StackTrace;
                    }
                    if (!String.IsNullOrEmpty(data.Source))
                    {
                        msgTemp.Source = inner.Source;
                    }
                }

                messageClass.InnerMessage.Add(msgTemp);

                inner = inner.InnerException;
            }
            var results = JsonConvert.SerializeObject(messageClass, setting);
            return results;
        }
    }
}
