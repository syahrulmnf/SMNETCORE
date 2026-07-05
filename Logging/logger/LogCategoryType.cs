using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMNETCORE.Logging
{
    public enum LogCategoryType
    {
        Common = 0,
        Service = 1,
        Website = 2,
        FileImport = 3,
        ReportSend = 4,
        Login = 5,
        DataRepository = 6,
        MobileAPI = 7,
        RedSeedTrainingService = 8,
        IntegratedService = 9,
        JWTSecurityDal = 10,
        JWTSecurityService = 11,
        JWTSecurityWebAPI = 12,
        WebHook = 13,
        RedisManager = 24
    }
}
