using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Utilities;

namespace SMNETCORE.Common.Helpers
{
    public class MultiIsoDateTimeStringReadConverter : IsoDateTimeConverter
    {
        public MultiIsoDateTimeStringReadConverter()
        {
        }


        //
        // Summary:
        //     Reads the JSON representation of the object.
        //
        // Parameters:
        //   reader:
        //     The Newtonsoft.Json.JsonReader to read from.
        //
        //   objectType:
        //     Type of the object.
        //
        //   existingValue:
        //     The existing value of object being read.
        //
        //   serializer:
        //     The calling serializer.
        //
        // Returns:
        //     The object value.
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            bool flag = Common.Helpers.Utils.IsNullable(objectType);
            Type type = (flag ? Nullable.GetUnderlyingType(objectType) : objectType);
            if (reader.TokenType == JsonToken.Null)
            {
                if (!Common.Helpers.Utils.IsNullable(objectType))
                {
                    throw new JsonSerializationException("Cannot convert null value to " + objectType.FullName);
                }

                return null;
            }

            if (reader.TokenType == JsonToken.Date)
            {
                if (type == typeof(DateTimeOffset))
                {
                    return new DateTimeOffset((DateTime)reader.Value);
                }

                return reader.Value;
            }

            if (reader.TokenType != JsonToken.String)
            {
                throw new JsonSerializationException("Unexpected token parsing date. Expected String, got " + reader.TokenType.ToString());
            }

            string text = reader.Value.ToString();
            if (string.IsNullOrEmpty(text) && flag)
            {
                return null;
            }

            return Common.Helpers.Utils.ConvertGenericString(text);
        }
    }
}
