using SMNETCORE.Common;
using SMNETCORE.Common.Enums;
using SMNETCORE.DataType.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMNETCORE.Cache.Enums
{
    public enum CacheCollectionType
    {
        Common=-1,
        Web = 0,
        Generic = 1,
        API = 2,
        DTO = 3,
        Services = 4,
        Auth = 5,
        OrganisationVariableSettingServices = 6,
        SessionManagerExclusive = 7,
        ReportVariableType = 8,
        PDFFileReportType = 9,
        Tenants = 10,
    }

    public static class CacheCollectionTypeUtils
    {
        public static string GetKey(this CacheCollectionType key, string tenantName)
        {
            if (!String.IsNullOrEmpty(tenantName)) return string.Format("{0}:{1}:{2}", tenantName, key.NullableToString(), ((int)key).NullableToString());
            return string.Format("{0}:{1}:{2}", Globals.RouteSettings.AdminClient, key.NullableToString(), ((int)key).NullableToString());
        }

    }
}
