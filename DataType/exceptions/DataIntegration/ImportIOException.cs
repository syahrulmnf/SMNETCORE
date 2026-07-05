using System;
using System.Runtime.Serialization;

namespace SMNETCORE.DataType.Exceptions
{
    public class ImportIOException : DataIntegrationException
    {
        public ImportIOException(string message, int? surveyId) : base(message, surveyId)
        {
        }

        public ImportIOException(string message, Exception innerException, int? surveyId) : base(message, innerException, surveyId)
        {
        }

        public ImportIOException(SerializationInfo info, StreamingContext context, int? surveyId) : base(info, context, surveyId)
        {
        }
    }
}
