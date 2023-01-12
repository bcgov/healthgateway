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
namespace HealthGateway.Admin.Server.Converters
{
    using System;
    using System.Globalization;
    using CsvHelper;
    using CsvHelper.Configuration;
    using CsvHelper.TypeConversion;
    using HealthGateway.Common.Data.Utils;

    /// <summary>
    /// String Date Time Converter.
    /// </summary>
    public class DateOutputConverter : ITypeConverter
    {
        /// <inheritdoc/>
        public object? ConvertFromString(
            string? text,
            IReaderRow? row,
            MemberMapData? memberMapData)
        {
            return text != null ? ParseDate(text) : null;
        }

        /// <inheritdoc/>
        public string ConvertToString(
            object? value,
            IWriterRow? row,
            MemberMapData? memberMapData)
        {
            return value != null ? DateFormatter.ToShortDateAndTime((DateTime)value) : string.Empty;
        }

        private static DateTime ParseDate(string text)
        {
            return DateTime.Parse(text, CultureInfo.InvariantCulture, DateTimeStyles.None);
        }
    }
}
