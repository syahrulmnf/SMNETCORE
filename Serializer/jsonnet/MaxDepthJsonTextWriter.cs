using SMNETCORE.Common.Helpers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMNETCORE.Serialize
{
    public static class MaxDepthJsonTextWriterUtility
    {
        public static string ConvertToJson<T>(this T Obj, int? maxDepth = null, bool isObject = true, JsonSerializerSettings setting = null)
        {
            return JSONUtils.ConvertToJson(Obj, maxDepth, isObject, setting);
        }

        public static string ConvertToJson<T>(this T Obj, JsonSerializerSettings setting = null)
        {

            return JSONUtils.ConvertToJson(Obj, setting);

        }
    }
    
}
