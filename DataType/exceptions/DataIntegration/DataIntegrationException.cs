using System;
using System.Runtime.Serialization;

namespace SMNETCORE.DataType.Exceptions
{
    public class DataIntegrationException : CustomException
    {
        public int? SurveyId { get; protected set; }
        protected DataIntegrationException(string message, int? surveyId)
            : base(message)
        {
            SurveyId = surveyId;
        }

        protected DataIntegrationException(string message, Exception innerException, int? surveyId)
            : base(message, innerException)
        {
            SurveyId = surveyId;
        }

        protected DataIntegrationException(SerializationInfo info, StreamingContext context, int? surveyId)
            : base(info, context)
        {
            SurveyId = surveyId;
        }

        public DataIntegrationException()
        {
        }
    }
}
