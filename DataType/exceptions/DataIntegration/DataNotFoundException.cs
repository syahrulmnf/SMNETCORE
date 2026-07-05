using System;
using System.Runtime.Serialization;

namespace SMNETCORE.DataType.Exceptions
{
    public class DataNotFoundException : DataIntegrationException
    {
        public DataNotFoundException(string message, int? surveyId)
            : base(message, surveyId)
        {
        }

        public DataNotFoundException(string message, Exception innerException, int? surveyId)
            : base(message, innerException, surveyId)
        {
        }

        public DataNotFoundException(SerializationInfo info, StreamingContext context, int? surveyId)
            : base(info, context, surveyId)
        {
        }
    }
}
