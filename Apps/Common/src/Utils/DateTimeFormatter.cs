//-------------------------------------------------------------------------
// Copyright © 2019 Province of British Columbia
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//-------------------------------------------------------------------------
using System;
using System.Globalization;

namespace HealthGateway.Common.Utils
{

    /// <summary>
    /// Utilities for formatting Dates and Times for Health Gateway.
    /// </summary>
    public static class DateTimeFormatter
    {
        /// <summary>
        /// Formats the supplied datetime as a date string.
        /// </summary>
        /// <param name="datetime">The datetime to format.</param>
        /// <returns>A formatted string or empty if input is null.</returns>
        public static string FormatDate(DateTime datetime)
        {
            return datetime != null ? datetime.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) : string.Empty;
        }
    }
}
