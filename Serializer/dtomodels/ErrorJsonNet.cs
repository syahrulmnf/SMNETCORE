using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMNETCORE.Serializer.DTOModels
{
    [Serializable]
    public class ErrorClassJson
    {
        public ErrorClassJson()
        {

        }
        public string Message { get; set; }
        public string ErrorMessage { get; set; }
        public string StackTrace { get; set; }

        public string Source { get; set; }
    }

    [Serializable]
    public class ErrorClassJsonResponse : ErrorClassJson
    {
        public ErrorClassJsonResponse()
        {
            InnerMessage = new List<ErrorClassJson>();
        }

        public List<ErrorClassJson> InnerMessage { get; set; }
    }
}
