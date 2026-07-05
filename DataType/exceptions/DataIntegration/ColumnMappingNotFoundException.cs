using System;
using System.Runtime.Serialization;

namespace SMNETCORE.DataType.Exceptions
{
    public class ColumnMappingNotFoundException : DataIntegrationException
    {
        public ColumnMappingNotFoundException(string message, int? surveyId)
            : base(message, surveyId)
        {
        }

        public ColumnMappingNotFoundException(string message, Exception innerException, int? surveyId)
            : base(message, innerException, surveyId)
        {
        }

        public ColumnMappingNotFoundException(SerializationInfo info, StreamingContext context, int? surveyId)
            : base(info, context, surveyId)
        {
        }
    }
}
