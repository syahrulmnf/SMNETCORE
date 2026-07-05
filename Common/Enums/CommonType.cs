using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMNETCORE.Common.Enums
{
    public class CommonType
    {
        public enum Months
        {
            January = 1,
            February = 2,
            March = 3,
            April = 4,
            May = 5,
            June = 6,
            July = 7,
            August = 8,
            September = 9,
            October = 10,
            November = 11,
            December = 12
        }

        public enum ShortMonths
        {
            [Description("January")]
            Jan = 1,
            [Description("February")]
            Feb = 2,
            [Description("March")]
            Mar = 3,
            [Description("April")]
            Apr = 4,
            [Description("May")]
            May = 5,
            [Description("June")]
            Jun = 6,
            [Description("July")]
            Jul = 7,
            [Description("August")]
            Aug = 8,
            [Description("September")]
            Sept = 9,
            [Description("October")]
            Oct = 10,
            [Description("November")]
            Nov = 11,
            [Description("December")]
            Dec = 12
        }

        public enum WeekDayType
        {
            Monday = 0,
            Tuesday = 1,
            Wednesday=2,
            Thursday=3,
            Friday =4,
            Saturday=5,
            Sunday=6
        }

        public enum EmailInterval
        {
            Daily=1,
            Weekly=2,
            Monthly=3,
            Quarterly=4
        }


     
    }
}
