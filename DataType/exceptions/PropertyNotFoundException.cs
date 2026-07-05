using System;
using System.Runtime.Serialization;

namespace SMNETCORE.DataType.Exceptions
{
    public class PropertyNotFoundException : CustomException
    {
        public PropertyNotFoundException(string message)
            : base(message)
        {
        }

        public PropertyNotFoundException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        public PropertyNotFoundException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
