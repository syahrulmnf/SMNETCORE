using SMNETCORE.Common;
using SMNETCORE.Common.Enums;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Text;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace SMNETCORE.DataType.Extensions
{
    public static class DateTimeExtensions
    {
        /// <summary>
        /// Gets the datetime representing the start of a week.
        /// </summary>
        /// <param name="dateTime">A date time within the week.</param>
        /// <returns></returns>
        public static DateTime GetStartOfWeek(this DateTime dateTime)
        {
            return dateTime.AddDays(-1d * ((int)dateTime.DayOfWeek)).Date;
        }

        /// <summary>
        /// Gets the datetime representing the start of a month.
        /// </summary>
        /// <param name="dateTime">A date time within the month.</param>
        /// <returns></returns>
        public static DateTime GetStartOfMonth(this DateTime dateTime)
        {
            return new DateTime(dateTime.Year, dateTime.Month, 1);
        }

        /// <summary>
        /// Gets the datetime representing the start of a quarter.
        /// </summary>
        /// <param name="dateTime">A date time within the quarter.</param>
        /// <returns></returns>
        public static DateTime GetStartOfQuarter(this DateTime dateTime)
        {
            var quarter = (dateTime.Month - 1) / 3;
            var firstMonthOfQuarter = (quarter * 3) + 1;
            return new DateTime(dateTime.Year, firstMonthOfQuarter, 1);
        }

        /// <summary>
        /// Converts the time in a specified time zone to Coordinated Universal Time (UTC), safely handling ambiguous and invalid
        /// local date/times.
        /// </summary>
        /// <remarks>
        /// If the local time is ambiguous, the earliest of the ambiguous values will be returned.
        /// If the local time is invalid, the earliest valid local time that is greater than the passed in time will be converted and returned.
        /// </remarks>
        /// <param name="dateTime">The date time.</param>
        /// <param name="sourceTimeZone">The source time zone.</param>
        /// <returns></returns>
        public static DateTime SafeConvertTimeToUtc(this DateTime dateTime, TimeZoneInfo sourceTimeZone)
        {
            if (sourceTimeZone.IsAmbiguousTime(dateTime))
            {
                var possibilities = sourceTimeZone.GetAmbiguousTimeOffsets(dateTime);
                return dateTime - (possibilities.Max());
            }
            
            if (sourceTimeZone.IsInvalidTime(dateTime))
            {
                // start with start of next minute
                var checkTime = new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, dateTime.Hour, dateTime.Minute, 0);
                checkTime = checkTime.AddMinutes(1d);

                // advance one minute at a time until we are no longer invalid
                while (sourceTimeZone.IsInvalidTime(checkTime))
                {
                    checkTime = checkTime.AddMinutes(1d);
                }

                return TimeZoneInfo.ConvertTimeToUtc(checkTime, sourceTimeZone);
            }

            return TimeZoneInfo.ConvertTimeToUtc(dateTime, sourceTimeZone);
        }

        public static DateTime AMonthBefore(this DateTime datetime, short? month, short? year, short? startDate)
        {
            if (!month.HasValue) month = (short) datetime.Month;
            if (!year.HasValue) year = (short)datetime.Year;
            if (month == 1)
            {
                month = (short)12;
                year--;
            }
            else
            {
                month--;
            }
            if (!startDate.HasValue) startDate = (short)datetime.Day;
            return new DateTime((int)year, (int)month, (int)startDate);

        }

        public static DateTime NextMonths(this DateTime datetime, short? month, short? year, short? startDate, int nextMonthNumber)
        {
            if (!month.HasValue) month = (short)datetime.Month;
            if (!year.HasValue) year = (short)datetime.Year;
            if (!startDate.HasValue) startDate = (short)datetime.Day;
            DateTime date = new DateTime((int)year, (int)month, (int)startDate);
            DateTime nextDate = date.AddMonths(nextMonthNumber);
            return nextDate;

        }

        public static string DateToString(this DateTime date, string format = "")
        {
            return Common.Helpers.Utils.DateToString(date, format);
        }

        public static string DateToString(this DateTime? date)
        {
            return date.Value.DateToString();
        }

        public static DateTime ConvertGenericString(this string stringDate, string[] formatspar = null, bool strictLimitToCurrentDate = false)
        {
            stringDate = stringDate.TrimString();
            stringDate = stringDate.Replace("  ", " ");
            if (!stringDate.HasStringValue()) return DateTime.MaxValue;

            long ticks;
            if (long.TryParse(stringDate, out ticks))
            {
                DateTime start = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                DateTime date = start.AddMilliseconds(ticks).ToLocalTime();
                return date;
            }

            var formats = formatspar ?? Globals.AvailableFormatDates;
            if(formats.Count() > 10)
            {

            }
            DateTime dateValue;

            foreach (string dateStringFormat in formats)
            {
                if (DateTime.TryParseExact(stringDate, dateStringFormat,
                                           CultureInfo.InvariantCulture,
                                           DateTimeStyles.None,
                                           out dateValue))
                {
                    if(!strictLimitToCurrentDate || (strictLimitToCurrentDate && dateValue <= DateTime.Now)) return dateValue;
                }
            }

            throw new Exception("Invalid Date Format Supplied : " + stringDate + ", Available Format: " + string.Join("||", formats));
        }

        public static bool TryConvertGenericString(this string stringDate, out DateTime result)
        {
            string[] formats = Globals.AvailableFormatDates;
            stringDate = stringDate.TrimString();
            stringDate = stringDate.Replace("  ", " ");
       
            DateTime dateValue;
            result = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            long startDateSeconds;
            if (long.TryParse(stringDate, out startDateSeconds))
            {
                //value is a unix timestamp
                var StartDate = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
                StartDate = StartDate.AddSeconds(startDateSeconds);
                result = StartDate;
                return true;
            }
            foreach (string dateStringFormat in formats)
            {
                var resultConv = DateTime.TryParseExact(stringDate, dateStringFormat,
                                           CultureInfo.InvariantCulture,
                                           DateTimeStyles.None,
                                           out dateValue);
                if (resultConv)
                {
                    result = dateValue;
                    return resultConv;
                }
            }

            
            return false;
        }

        public static bool ConvertGenericStringFormat(this string stringDate, string formatString, out DateTime result)
        {
            long startDateSeconds;
            if (long.TryParse(stringDate, out startDateSeconds))
            {
                //value is a unix timestamp
                var PurchaseDate = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
                PurchaseDate = PurchaseDate.AddSeconds(startDateSeconds);

                result = PurchaseDate;
                return true;
            }

            stringDate = stringDate.TrimString();
            stringDate = stringDate.Replace("  ", " ");
            return DateTime.TryParseExact(stringDate, formatString, System.Globalization.CultureInfo.InvariantCulture, DateTimeStyles.None, out result);
            
        }

        public static int GetIso8601WeekOfYear(this DateTime time)
        {
            // Seriously cheat.  If its Monday, Tuesday or Wednesday, then it'll 
            // be the same week# as whatever Thursday, Friday or Saturday are,
            // and we always get those right
            DayOfWeek day = CultureInfo.InvariantCulture.Calendar.GetDayOfWeek(time);
            if (day >= DayOfWeek.Monday && day <= DayOfWeek.Wednesday)
            {
                time = time.AddDays(3);
            }

            // Return the week of our adjusted day
            return CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(time, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
        }

        public static Int64 ToUnixTimeStamps(this DateTime data)
        {
            return (Int64)(data - new DateTime(1970, 1, 1)).TotalMilliseconds;
        }

        public static DateTime ToDate(this Int64 data)
        {
            return new DateTime(1970, 1, 1).Date.AddMilliseconds(data);
        }

        public static DateTime StartOfDay(this DateTime dates)
        {
            return (dates.Date + Globals.EarlyDayHours);
        }

        public static DateTime EndOfDay(this DateTime dates)
        {
            return (dates.Date + Globals.TwentyFourHours);
        }

        public static DateTime UtcStartOfDay(this DateTime dates)
        {
            return (dates.ToUniversalTime().Date + Globals.EarlyDayHours);
        }

        public static DateTime UtcEndOfDay(this DateTime dates)
        {
            return (dates.ToUniversalTime().Date + Globals.TwentyFourHours);
        }

        public static bool InBetween(this DateTime now, DateTime startDate, DateTime endDate, bool isEqual)
        {
            if(isEqual)
            {
                return startDate <= now && now <= endDate;
            }
            return startDate < now && now < endDate;
        }
        
        public static DateTime RoundToTicks(this DateTime target, long ticks) => new DateTime((target.Ticks + ticks / 2) / ticks * ticks, target.Kind);
        public static DateTime RoundUpToTicks(this DateTime target, long ticks) => new DateTime((target.Ticks + ticks - 1) / ticks * ticks, target.Kind);
        public static DateTime RoundDownToTicks(this DateTime target, long ticks) => new DateTime(target.Ticks / ticks * ticks, target.Kind);

        public static DateTime Round(this DateTime target, TimeSpan round) => RoundToTicks(target, round.Ticks);
        public static DateTime RoundUp(this DateTime target, TimeSpan round) => RoundUpToTicks(target, round.Ticks);
        public static DateTime RoundDown(this DateTime target, TimeSpan round) => RoundDownToTicks(target, round.Ticks);

        public static DateTime RoundToMinutes(this DateTime target, int minutes = 1) => RoundToTicks(target, minutes * TimeSpan.TicksPerMinute);
        public static DateTime RoundUpToMinutes(this DateTime target, int minutes = 1) => RoundUpToTicks(target, minutes * TimeSpan.TicksPerMinute);
        public static DateTime RoundDownToMinutes(this DateTime target, int minutes = 1) => RoundDownToTicks(target, minutes * TimeSpan.TicksPerMinute);

        public static DateTime RoundToSeconds(this DateTime target, int seconds = 1) => RoundToTicks(target, seconds * TimeSpan.TicksPerSecond);
        public static DateTime RoundUpToSeconds(this DateTime target, int seconds = 1) => RoundUpToTicks(target, seconds * TimeSpan.TicksPerSecond);
        public static DateTime RoundDownToSeconds(this DateTime target, int seconds = 1) => RoundDownToTicks(target, seconds * TimeSpan.TicksPerSecond);

        public static DateTime RoundToMiliSeconds(this DateTime target, int mseconds = 1) => RoundToTicks(target, mseconds * TimeSpan.TicksPerMillisecond);
        public static DateTime RoundUpToMiliSeconds(this DateTime target, int mseconds = 1) => RoundUpToTicks(target, mseconds * TimeSpan.TicksPerMillisecond);
        public static DateTime RoundDownToMiliSeconds(this DateTime target, int mseconds = 1) => RoundDownToTicks(target, mseconds * TimeSpan.TicksPerMillisecond);

        public static DateTime RoundToHours(this DateTime target, int hours = 1) => RoundToTicks(target, hours * TimeSpan.TicksPerHour);
        public static DateTime RoundUpToHours(this DateTime target, int hours = 1) => RoundUpToTicks(target, hours * TimeSpan.TicksPerHour);
        public static DateTime RoundDownToHours(this DateTime target, int hours = 1) => RoundDownToTicks(target, hours * TimeSpan.TicksPerHour);

        public static DateTime RoundToDays(this DateTime target, int days = 1) => RoundToTicks(target, days * TimeSpan.TicksPerDay);
        public static DateTime RoundUpToDays(this DateTime target, int days = 1) => RoundUpToTicks(target, days * TimeSpan.TicksPerDay);
        public static DateTime RoundDownToDays(this DateTime target, int days = 1) => RoundDownToTicks(target, days * TimeSpan.TicksPerDay);

        public static DateOnly ToDateOnly(this DateTime date)
        {
            return DateOnly.FromDateTime(date);
        }
    }
}
