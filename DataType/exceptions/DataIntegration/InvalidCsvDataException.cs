using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace SMNETCORE.DataType.Exceptions
{
    public class InvalidCsvDataException : DataIntegrationException
    {
        public InvalidCsvDataException(string message, int? surveyId)
            : base(message, surveyId)
        {
        }

        public InvalidCsvDataException(string message, Exception innerException, int? surveyId)
            : base(message, innerException, surveyId)
        {
        }

        public InvalidCsvDataException(SerializationInfo info, StreamingContext context, int? surveyId)
            : base(info, context, surveyId)
        {
        }
    }
}
