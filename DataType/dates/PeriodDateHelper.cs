using SMNETCORE.Common;
using SMNETCORE.Common.Enums;
using SMNETCORE.DataType.Extensions;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMNETCORE.DataType.Dates
{
    public class PeriodDateHelper
    {
        [ThreadStatic]
        private static Lazy<PeriodDateHelper> _helper;
        public static PeriodDateHelper Instance
        {
            get
            {
                if (_helper == null) _helper = new Lazy<PeriodDateHelper>(() => new PeriodDateHelper(), isThreadSafe: true);
                return _helper.Value;
            }
        }

        public CommonType.ShortMonths DateFromMonthStr(int DateFromMonth) => (CommonType.ShortMonths)DateFromMonth;
        public CommonType.ShortMonths DateToMonthStr(int DateToMonth) => (CommonType.ShortMonths)DateToMonth;
        public CommonType.Months DateFromMonthLongStr(int DateFromMonth) => (CommonType.Months)DateFromMonth;
        public CommonType.Months DateToMonthLongStr(int DateToMonth) => (CommonType.Months)DateToMonth;
        public string PeriodMonthRangeName(DateTime StartDate, DateTime EndDate, int DateFromMonth, int DateFromYear, int DateToMonth, int DateToYear, bool IsLast24Hr)
        {
            if (IsLast24Hr) return string.Format("{0}-{1}", StartDate.NullableToString(Globals.GlobalDateFormat), EndDate.NullableToString(Globals.GlobalDateFormat));
            if (DateFromYear != DateToYear) return string.Format("{0}({1})-{2}({3})", DateFromMonthStr(DateFromMonth).ToString(), DateFromYear, DateToMonthStr(DateToMonth).ToString(), DateToYear);
            return string.Format("{0}-{1}", DateFromMonthStr(DateFromMonth).ToString(), DateToMonthStr(DateToMonth).ToString());
        }

        public string FormattedDateFrom(DateTime StartDate, int DateFromMonth, int DateFromYear, bool IsLast24Hr)
        {
            if (DateFromMonth == 0)
            {
                return string.Empty;
            }

            return IsLast24Hr ? StartDate.NullableToString(Globals.GlobalDateFormat) :
                String.Format("{0} {1}", CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(DateFromMonth).Substring(0, 3), DateFromYear);
            
        }

        public string DatePeriodMonthRangeName(DateTime StartDate, DateTime EndDate)
        {
                return string.Format("{0} - 2", StartDate.NullableToString(Globals.GlobalDateFormat), EndDate.NullableToString(Globals.GlobalDateFormat));
            
        }

        public string ToString(PeriodDTOModelType Id, DateTime StartDate, DateTime EndDate, int DateFromMonth, int DateFromYear, int DateToMonth, int DateToYear, bool IsLast24Hr)
        {
            if (Id.Equals(PeriodDTOModelType.Custom))
            {
                return PeriodDTOModelType.Custom.ToText();
            }

            if (IsLast24Hr)
            {
                return string.Format("{0} - {1}", StartDate.ToString(Globals.GlobalDateFormat), EndDate.ToString(Globals.GlobalDateFormat));
            }

            string name = string.Empty;
            name = String.Format("{0} {1}", CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(DateFromMonth).Substring(0, 3), DateFromYear);

            if (DateFromYear != DateToYear || DateFromMonth != DateToMonth)
                name += String.Format(" - {0} {1}", CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(DateToMonth).Substring(0, 3), DateToYear);

            return name;
        }
    }
}
