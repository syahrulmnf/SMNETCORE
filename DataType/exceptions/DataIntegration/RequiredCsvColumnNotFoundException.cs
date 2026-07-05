using System;
using System.Runtime.Serialization;

namespace SMNETCORE.DataType.Exceptions
{
    public class RequiredCsvColumnNotFoundException : DataIntegrationException
    {
        public RequiredCsvColumnNotFoundException(string message, int? surveyId)
            : base(message, surveyId)
        {
        }

        public RequiredCsvColumnNotFoundException(string message, Exception innerException, int? surveyId)
            : base(message, innerException, surveyId)
        {
        }

        public RequiredCsvColumnNotFoundException(SerializationInfo info, StreamingContext context, int? surveyId)
            : base(info, context, surveyId)
        {
        }
    }
}
