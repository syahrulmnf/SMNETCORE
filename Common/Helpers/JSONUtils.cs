using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMNETCORE.Common.Helpers
{
    public class MaxDepthJsonTextWriter : JsonTextWriter
    {
        public int? MaxDepth { get; set; }
        public int MaxObservedDepth { get; private set; }

        public MaxDepthJsonTextWriter(TextWriter writer, JsonSerializerSettings settings)
            : base(writer)
        {
            this.MaxDepth = (settings == null ? null : settings.MaxDepth);
            this.MaxObservedDepth = 0;
        }

        public MaxDepthJsonTextWriter(TextWriter writer, int? maxDepth)
            : base(writer)
        {
            this.MaxDepth = maxDepth;
        }

        public override void WriteStartArray()
        {
            base.WriteStartArray();
            CheckDepth();
        }

        public override void WriteStartConstructor(string name)
        {
            base.WriteStartConstructor(name);
            CheckDepth();
        }

        public override void WriteStartObject()
        {
            base.WriteStartObject();
            CheckDepth();
        }

        private void CheckDepth()
        {
            MaxObservedDepth = Math.Max(MaxObservedDepth, Top);
            if (Top > MaxDepth)
                throw new JsonSerializationException(string.Format("Depth {0} Exceeds MaxDepth {1} at path \"{2}\"", Top, MaxDepth, Path));
        }
    }

    public class JSONUtils
    {
        public static string ConvertToJson<T>(T Obj, JsonSerializerSettings setting = null)
        {
            setting = setting ?? new JsonSerializerSettings() { MaxDepth = 10, NullValueHandling = NullValueHandling.Ignore, TypeNameHandling = TypeNameHandling.Objects };
            string json;
            try
            {
                using (var writer = new StringWriter())
                {
                    using (var jsonWriter = new MaxDepthJsonTextWriter(writer, setting))
                    {
                        JsonSerializer.Create(setting).Serialize(jsonWriter, Obj);
                        // Log the MaxObservedDepth here, if you want to.
                    }
                    json = writer.ToString();
                }
                return json;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static string ConvertToJson<T>(T Obj, int? maxDepth = null, bool isObject = true, JsonSerializerSettings setting = null)
        {
            setting = setting ?? new JsonSerializerSettings() { MaxDepth = maxDepth ?? 10, NullValueHandling = NullValueHandling.Ignore };
            if (isObject) setting.TypeNameHandling = TypeNameHandling.Objects;
            if(maxDepth.HasValue) setting.MaxDepth = maxDepth.Value;

            return ConvertToJson(Obj, setting);
        }
    }
}
