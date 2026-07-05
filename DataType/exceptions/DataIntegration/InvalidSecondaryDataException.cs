using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace SMNETCORE.DataType.Exceptions
{
    public class InvalidSecondaryDataException : DataIntegrationException
    {
        public InvalidSecondaryDataException(string message, int? surveyId)
            : base(message, surveyId)
        {
        }

        public InvalidSecondaryDataException(string message, Exception innerException, int? surveyId)
            : base(message, innerException, surveyId)
        {
        }

        public InvalidSecondaryDataException(SerializationInfo info, StreamingContext context, int? surveyId)
            : base(info, context, surveyId)
        {
        }
    }
}
