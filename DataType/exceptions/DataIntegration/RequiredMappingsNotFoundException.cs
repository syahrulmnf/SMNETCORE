using System;
using System.Runtime.Serialization;

namespace SMNETCORE.DataType.Exceptions
{
    public class RequiredMappingsNotFoundException : DataIntegrationException
    {
        public RequiredMappingsNotFoundException(string message, int? surveyId)
            : base(message, surveyId)
        {
        }

        public RequiredMappingsNotFoundException(string message, Exception innerException, int? surveyId)
            : base(message, innerException, surveyId)
        {
        }

        public RequiredMappingsNotFoundException(SerializationInfo info, StreamingContext context, int? surveyId)
            : base(info, context, surveyId)
        {
        }
    }
}
