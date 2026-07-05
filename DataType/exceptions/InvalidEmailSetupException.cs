using System;
using System.Runtime.Serialization;

namespace SMNETCORE.DataType.Exceptions
{
    public class InvalidEmailSetupException : CustomException
    {
        public InvalidEmailSetupException(string message)
            : base(message)
        {
        }

        public InvalidEmailSetupException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        public InvalidEmailSetupException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
