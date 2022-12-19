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

/// <summary>
/// Formats dates and times for display.
/// </summary>
[ExcludeFromCodeCoverage]
public static class DateFormatter
{
    /// <summary>
    /// CultureInfo corresponding to the "en-CA" culture.
    /// </summary>
    public static readonly CultureInfo CanadianCulture = CultureInfo.CreateSpecificCulture("en-CA");

    /// <summary>
    /// Converts the supplied date to a string formatted as YYYY-MM-DD (2022-01-01).
    /// </summary>
    /// <param name="date">The date to format.</param>
    /// <returns>The date in YYYY-MM-DD format.</returns>
    public static string ToShortDate(DateOnly date)
    {
        return date.ToString("d", CanadianCulture);
    }

    /// <summary>
    /// Converts the supplied dateTime to a string formatted as YYYY-MM-DD (2022-01-01).
    /// </summary>
    /// <param name="dateTime">The dateTime to format.</param>
    /// <returns>The dateTime in YYYY-MM-DD format.</returns>
    public static string ToShortDate(DateTime dateTime)
    {
        return dateTime.ToString("d", CanadianCulture);
    }

    /// <summary>
    /// Converts the supplied date to a string formatted as MMMM dd, YYYY (January 1, 2022).
    /// </summary>
    /// <param name="date">The date to format.</param>
    /// <returns>The date in MMMM dd, YYYY format.</returns>
    public static string ToLongDate(DateOnly date)
    {
        return date.ToString("D", CanadianCulture);
    }

    /// <summary>
    /// Converts the supplied dateTime to a string formatted as MMMM dd, YYYY (January 1, 2022).
    /// </summary>
    /// <param name="dateTime">The dateTime to format.</param>
    /// <returns>The dateTime in MMMM dd, YYYY format.</returns>
    public static string ToLongDate(DateTime dateTime)
    {
        return dateTime.ToString("D", CanadianCulture);
    }

    /// <summary>
    /// Converts the supplied time to a string formatted as HH:mm (5:00 PM).
    /// </summary>
    /// <param name="time">The time to format.</param>
    /// <returns>The time in HH:mm format.</returns>
    public static string ToShortTime(TimeOnly time)
    {
        return time.ToString("t", CanadianCulture);
    }

    /// <summary>
    /// Converts the supplied dateTime to a string formatted as HH:mm (5:00 PM).
    /// </summary>
    /// <param name="dateTime">The dateTime to format.</param>
    /// <returns>The dateTime in HH:mm format.</returns>
    public static string ToShortTime(DateTime dateTime)
    {
        return dateTime.ToString("t", CanadianCulture);
    }

    /// <summary>
    /// Converts the supplied dateTime to a string formatted as YYYY-MM-DD HH:mm (2022-01-01 5:00 PM).
    /// </summary>
    /// <param name="dateTime">The dateTime to format.</param>
    /// <returns>The dateTime in HH:mm format.</returns>
    public static string ToShortDateAndTime(DateTime dateTime)
    {
        return dateTime.ToString("g", CanadianCulture);
    }

    /// <summary>
    /// Converts the supplied dateTime to a string formatted as MMMM dd, YYYY HH:mm (January 1, 2022 5:00 PM).
    /// </summary>
    /// <param name="dateTime">The dateTime to format.</param>
    /// <returns>The dateTime in HH:mm format.</returns>
    public static string ToLongDateAndTime(DateTime dateTime)
    {
        return dateTime.ToString("f", CanadianCulture);
    }

    /// <summary>
    /// Tries to parse a string to a DateTime using the format supplied.
    /// </summary>
    /// <param name="dateTime">string to parse.</param>
    /// <param name="format">format to parse into.</param>
    /// <param name="parsedDateTime">the parsed DateTime is successful.</param>
    /// <returns>false if failed to parse, true is succeeded.</returns>
    public static bool TryParse(string dateTime, string format, out DateTime parsedDateTime) =>
        DateTime.TryParseExact(dateTime, format, CanadianCulture, DateTimeStyles.None, out parsedDateTime);
}
