using System.ComponentModel;

namespace SMNETCORE.Common.Enums
{
    public enum ComparisonPeriods
    {
        [Description("Last 7 vs Previous 7 Days")]
        Last7DaysVsPrevious7Days = Globals.ComparisonPeriod.Last7DaysVsPrevious7Days,
        [Description("Last 14 vs Previous 14 Days")]
        Last14DaysVsPrevious14Days = Globals.ComparisonPeriod.Last14DaysVsPrevious14Days,
        [Description("Month To Date vs Last Month")]
        ThisMonthVsLastMonth = Globals.ComparisonPeriod.ThisMonthVsLastMonth,
        [Description("Last 3 Months vs Previous 3 Months")]
        Last3MonthsVsPrevious3Months = Globals.ComparisonPeriod.Last3MonthsVsPrevious3Months,
        [Description("Last 3 Months vs Same 3 Months Last Year")]
        Last3MonthsVsLastYear = Globals.ComparisonPeriod.Last3MonthsVsLastYear,
        [Description("Last Month vs Same Month Last Year")]
        LastMonthVsSameMonthLastYear = Globals.ComparisonPeriod.LastMonthsVsSameMonthLastYear,
        //[Description("This Month vs. First Month of Program")]
        //ThisMonthVsFirstMonthOfProgram = 3,
        //[Description("Last 3 Months vs. First 3 Months of Program")]
        //Last3MonthsVsFirst3MonthsOfProgram = 4,
        [Description("Last 12 Months vs Previous 12 Months")]
        Last12MonthsVsPrevious12Months = Globals.ComparisonPeriod.Last12MonthsVsPrevious12Months,
        [Description("Last Month vs Previous Month")]
        LastMonthVsPreviousMonth = Globals.ComparisonPeriod.LastMonthVsPreviousMonth,
        [Description("Year To Date vs Previous Year To Date")]
        YearToDate = Globals.ComparisonPeriod.YearToDate,
        [Description("Custom Range")]
        Custom = Globals.ComparisonPeriod.Custom,
        [Description("Custom Dates")]
        CustomDates = Globals.ComparisonPeriod.CustomDates,
        [Description("Current Vs Last Week")]
        CurrentVsLastWeek = Globals.ComparisonPeriod.CurrentVsLastWeek,
        [Description("Last Vs Previous Week")]
        LastVsPrevWeek = Globals.ComparisonPeriod.LastVsPrevWeek,
    }
}