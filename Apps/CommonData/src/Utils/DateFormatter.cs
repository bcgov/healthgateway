// -------------------------------------------------------------------------
//  Copyright © 2019 Province of British Columbia
//
//  Licensed under the Apache License, Version 2.0 (the "License");
//  you may not use this file except in compliance with the License.
//  You may obtain a copy of the License at
//
//  http://www.apache.org/licenses/LICENSE-2.0
//
//  Unless required by applicable law or agreed to in writing, software
//  distributed under the License is distributed on an "AS IS" BASIS,
//  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  See the License for the specific language governing permissions and
//  limitations under the License.
// -------------------------------------------------------------------------
namespace HealthGateway.Common.Data.Utils;

using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.InteropServices;
using Microsoft.Extensions.Configuration;

/// <summary>
/// Formats dates and times for display.
/// </summary>
public static class DateFormatter
{
    private const string UnixTzKey = "TimeZone:UnixTimeZoneId";
    private const string WindowsTzKey = "TimeZone:WindowsTimeZoneId";

    /// <summary>
    /// Converts the supplied date to a string formatted as YYYY-MM-DD (2022-01-01).
    /// </summary>
    /// <param name="date">The date to format.</param>
    /// <returns>The date in YYYY-MM-DD format.</returns>
    public static string ToShortDate(DateOnly date)
    {
        return date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// Converts the supplied dateTime to a string formatted as YYYY-MM-DD (2022-01-01).
    /// </summary>
    /// <param name="dateTime">The dateTime to format.</param>
    /// <returns>The dateTime in YYYY-MM-DD format.</returns>
    public static string ToShortDate(DateTime dateTime)
    {
        return dateTime.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// Converts the supplied date to a string formatted as MMMM dd, YYYY (January 1, 2022).
    /// </summary>
    /// <param name="date">The date to format.</param>
    /// <returns>The date in MMMM dd, YYYY format.</returns>
    public static string ToLongDate(DateOnly date)
    {
        return date.ToString("MMMM dd, yyyy", CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// Converts the supplied dateTime to a string formatted as MMMM dd, YYYY (January 1, 2022).
    /// </summary>
    /// <param name="dateTime">The dateTime to format.</param>
    /// <returns>The dateTime in MMMM dd, YYYY format.</returns>
    public static string ToLongDate(DateTime dateTime)
    {
        return dateTime.ToString("MMMM dd, yyyy", CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// Converts the supplied time to a string formatted as HH:mm (5:00 PM).
    /// </summary>
    /// <param name="time">The time to format.</param>
    /// <returns>The time in HH:mm format.</returns>
    public static string ToShortTime(TimeOnly time)
    {
        return time.ToString("h:mm tt", CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// Converts the supplied dateTime to a string formatted as HH:mm (5:00 PM).
    /// </summary>
    /// <param name="dateTime">The dateTime to format.</param>
    /// <returns>The dateTime in HH:mm format.</returns>
    public static string ToShortTime(DateTime dateTime)
    {
        return dateTime.ToString("h:mm tt", CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// Converts the supplied dateTime to a string formatted as YYYY-MM-DD HH:mm (2022-01-01 5:00 PM).
    /// </summary>
    /// <param name="dateTime">The dateTime to format.</param>
    /// <returns>The dateTime in HH:mm format.</returns>
    public static string ToShortDateAndTime(DateTime? dateTime)
    {
        return dateTime == null
            ? string.Empty
            : dateTime.Value.ToString("yyyy-MM-dd h:mm tt", CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// Converts the supplied dateTime to a string formatted as MMMM dd, YYYY HH:mm (January 1, 2022 5:00 PM).
    /// </summary>
    /// <param name="dateTime">The dateTime to format.</param>
    /// <returns>The dateTime in HH:mm format.</returns>
    public static string ToLongDateAndTime(DateTime dateTime)
    {
        return dateTime.ToString("MMMM dd, yyyy h:mm tt", CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// Sets the kind of a supplied DateTime to UTC if it is currently unspecified.
    /// </summary>
    /// <param name="dateTime">The DateTime to modify.</param>
    /// <returns>The supplied DateTime, no longer with an unspecified kind.</returns>
    [return: NotNullIfNotNull(nameof(dateTime))]
    public static DateTime? SpecifyUtc(DateTime? dateTime)
    {
        return dateTime?.Kind != DateTimeKind.Unspecified ? dateTime : DateTime.SpecifyKind(dateTime.Value, DateTimeKind.Utc);
    }

    /// <summary>
    /// Tries to parse a string to a DateTime using the format supplied.
    /// </summary>
    /// <param name="dateTime">string to parse.</param>
    /// <param name="format">format to parse into.</param>
    /// <param name="parsedDateTime">the parsed DateTime is successful.</param>
    /// <returns>false if failed to parse, true is succeeded.</returns>
    public static bool TryParse(string dateTime, string format, out DateTime parsedDateTime)
    {
        return DateTime.TryParseExact(dateTime, format, CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedDateTime);
    }

    /// <summary>
    /// Gets local timezone.
    /// </summary>
    /// <param name="configuration">The configuration to use.</param>
    /// <returns>TimeZoneInfo object representing local timezone.</returns>
    public static TimeZoneInfo GetLocalTimeZone(IConfiguration configuration)
    {
        return TimeZoneInfo.FindSystemTimeZoneById(
            RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                ? GetConfigurationValue<string>(configuration, WindowsTzKey)
                : GetConfigurationValue<string>(configuration, UnixTzKey));
    }

    /// <summary>
    /// Gets local time offset from utc.
    /// </summary>
    /// <param name="configuration">The configuration to use.</param>
    /// <param name="utcDateTime">The UTC datetime to derive offset from.</param>
    /// <returns>TimeSpan object representing the difference between local time and UTC.</returns>
    public static TimeSpan GetLocalTimeOffset(IConfiguration configuration, DateTime utcDateTime)
    {
        TimeZoneInfo localTimeZone = GetLocalTimeZone(configuration);
        return localTimeZone.GetUtcOffset(utcDateTime);
    }

    private static T GetConfigurationValue<T>(IConfiguration cfg, string key)
    {
        return cfg.GetValue<T>(key)!;
    }
}
