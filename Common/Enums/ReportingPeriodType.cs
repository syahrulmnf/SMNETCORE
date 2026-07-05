using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMNETCORE.Common.Enums
{
    public enum WeeklyPeriodModeEnum
    {
        Default = 0,
        Last7DaysMode = 1,
    }

    public enum PeriodDTOModelType
    {
        //[Description("All")]
        //All = Globals.PeriodDTOModel.Undefined,
        [Description("Month To Date")]
        MonthToDate = Globals.PeriodDTOModel.MonthToDate,
        [Description("Last Month")]
        LastMonth = Globals.PeriodDTOModel.LastMonth,
        [Description("Last 3 Months")]
        Last3Months = Globals.PeriodDTOModel.Last3Months,
        [Description("Year To Date")]
        YearToDate = Globals.PeriodDTOModel.YearToDate,
        [Description("Last Week")]
        LastWeek = Globals.PeriodDTOModel.LastWeek,
        [Description("Last 24 Hours")]
        Last24Hours = Globals.PeriodDTOModel.Last24Hours,
        [Description("Last 12 Months")]
        Last12Months = Globals.PeriodDTOModel.Last12Months,
        [Description("Previous 3 Months")]
        Previous3Months = Globals.PeriodDTOModel.Previous3Months,
        [Description("First Month Program")]
        FirstMonthOfProgram = Globals.PeriodDTOModel.FirstMonthOfProgram,
        [Description("Firt 3 Months of Program")]
        First3MonthsOfProgram = Globals.PeriodDTOModel.First3MonthsOfProgram,
        [Description("Last 6 Months")]
        Last6Months = Globals.PeriodDTOModel.Last6Months,
        [Description("Previous Month")]
        PreviousMonth = Globals.PeriodDTOModel.PreviousMonth,
        [Description("Previous 12 Months")]
        Previous12Months = Globals.PeriodDTOModel.Previous12Months,
        [Description("Custom Range")]
        Custom = Globals.PeriodDTOModel.Custom,
        [Description("Prev. Week")]
        PrevWeek = Globals.PeriodDTOModel.PrevWeek,
        [Description("To Day")]
        ToDay = Globals.PeriodDTOModel.ToDay,
        [Description("Yesterday")]
        LastDay = Globals.PeriodDTOModel.LastDay,
        [Description("Prev. 6 Months")]
        Prev6Months = Globals.PeriodDTOModel.Prev6Months,
        [Description("Prev. 12 Months")]
        Prev12Months = Globals.PeriodDTOModel.Prev12Months,
        [Description("Prev. YTD")]
        PrevYTD = Globals.PeriodDTOModel.PrevYTD,
        [Description("Prev. Custom")]
        PrevCustom = Globals.PeriodDTOModel.PrevCustom,
        [Description("Custom Date")]
        CustomDates = Globals.PeriodDTOModel.CustomDate,
        [Description("Prev. Custom Date")]
        PrevCustomDates = Globals.PeriodDTOModel.PrevCustomDate,
        [Description("Yearly")]
        Yearly = Globals.PeriodDTOModel.Yearly,
        [Description("Last 3 Months This Year")]
        Last3MonthThisYear = Globals.PeriodDTOModel.Last3MonthThisYear,
        [Description("Last 3 Months Prev. Year")]
        Last3MonthPrevYear = Globals.PeriodDTOModel.Last3MonthPrevYear,
        [Description("Last Month This Year")]
        LastMonthThisYear = Globals.PeriodDTOModel.LastMonthThisYear,
        [Description("Last Month Prev. Year")]
        LastMonthPrevYear = Globals.PeriodDTOModel.LastMonthPrevYear,
        [Description("Last 24 Months")]
        Last24Months = Globals.PeriodDTOModel.Last24Months,
        [Description("Last 3 Days")]
        Last3Days = Globals.PeriodDTOModel.Last3Days,
        [Description("Custom Month")]
        SingleCustomMonth = Globals.PeriodDTOModel.SingleCustomMonth,
        [Description("Custom Prev. Month")]
        SinglePrevCustomMonth = Globals.PeriodDTOModel.SinglePrevCustomMonth,
        [Description("Quarter 1")]
        Quarter1 = Globals.PeriodDTOModel.Quarter1,
        [Description("Quarter 2")]
        Quarter2 = Globals.PeriodDTOModel.Quarter2,
        [Description("Quarter 3")]
        Quarter3 = Globals.PeriodDTOModel.Quarter3,
        [Description("Current Quarter")]
        CurrentQuarter = Globals.PeriodDTOModel.CurrentQuarter,
        [Description("Quarter 1 Last year")]
        LastYearQuarter1 = Globals.PeriodDTOModel.LastYearQuarter1,
        [Description("Quarter 2 Last Year")]
        LastYearQuarter2 = Globals.PeriodDTOModel.LastYearQuarter2,
        [Description("Quarter 3 Last Year")]
        LastYearQuarter3 = Globals.PeriodDTOModel.LastYearQuarter3,
        [Description("Last Quarter")]
        LastQuarter = Globals.PeriodDTOModel.LastQuarter,
        [Description("Last 7 Days")]
        Last7Days = Globals.PeriodDTOModel.Last7Days,
        [Description("Prev. 7 Days")]
        Prev7Days = Globals.PeriodDTOModel.Prev7Days,
        [Description("Last 14 Days")]
        Last14Days = Globals.PeriodDTOModel.Last14Days,
        [Description("Prev 14 Days")]
        Prev14Days = Globals.PeriodDTOModel.Prev14Days,
        [Description("Current Week")]
        CurrentWeek = Globals.PeriodDTOModel.CurrentWeek,
        [Description("Last 6 Month Prev. Year")]
        Last6MonthPrevYear = Globals.PeriodDTOModel.Last6MonthPrevYear,
        [Description("Last 6 Month This Year")]
        Last6MonthThisYear = Globals.PeriodDTOModel.Last6MonthThisYear,
    }
}
