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
namespace HealthGateway.Admin.Client.Services
{
    using System;
    using HealthGateway.Common.Data.Utils;
    using Microsoft.Extensions.Configuration;

    /// <summary>
    /// Injectable service to provide data conversion methods.
    /// </summary>
    public class DateConversionService : IDateConversionService
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DateConversionService"/> class.
        /// </summary>
        /// <param name="configuration">Injected application settings</param>
        public DateConversionService(IConfiguration configuration)
        {
            this.Configuration = configuration;
        }

        private IConfiguration Configuration { get; }

        /// <inheritdoc/>
        public DateTime ConvertFromUtc(DateTime utcDateTime)
        {
            return TimeZoneInfo.ConvertTimeFromUtc(
                utcDateTime,
                DateFormatter.GetLocalTimeZone(this.Configuration));
        }

        /// <inheritdoc/>
        public DateTime? ConvertFromUtc(DateTime? utcDateTime, bool returnNowIfNull = false)
        {
            if (utcDateTime != null)
            {
                return this.ConvertFromUtc(utcDateTime.Value);
            }

            if (returnNowIfNull)
            {
                return this.ConvertFromUtc(DateTime.UtcNow);
            }

            return null;
        }

        /// <inheritdoc/>
        public string ConvertToShortFormatFromUtc(DateTime? utcDateTime, string fallbackString = "-")
        {
            return utcDateTime == null ? fallbackString : DateFormatter.ToShortDateAndTime(this.ConvertFromUtc(utcDateTime.Value));
        }

        /// <inheritdoc/>
        public DateTime ConvertTime(DateTime dateTime)
        {
            return TimeZoneInfo.ConvertTime(
                dateTime,
                DateFormatter.GetLocalTimeZone(this.Configuration));
        }
    }
}
