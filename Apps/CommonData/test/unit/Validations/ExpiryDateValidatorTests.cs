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
namespace HealthGateway.Common.Data.Tests.Validations
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using HealthGateway.Common.Data.Utils;
    using HealthGateway.Common.Data.Validations;
    using Microsoft.Extensions.Configuration;
    using Xunit;

    /// <summary>
    /// Validates Expiry Date.
    /// </summary>
    public class ExpiryDateValidatorTests
    {
        /// <summary>
        /// Tests for expiry date validator.
        /// </summary>
        /// <param name="daysToAdd">Number of days to add to create expiry date from utc now.</param>
        /// <param name="success">The value indicates whether the test should succeed or not.</param>
        [Theory]
        [InlineData(1, true)]
        [InlineData(0, true)]
        [InlineData(-1, false)]
        public void ValidateExpiryDate(int daysToAdd, bool success)
        {
            TimeZoneInfo localTimezone = DateFormatter.GetLocalTimeZone(GetConfiguration());
            DateOnly referenceDate = DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, localTimezone));
            DateOnly expiryDate = referenceDate.AddDays(daysToAdd);
            bool actual = ExpiryDateValidator.IsValid(expiryDate, referenceDate);
            Assert.True(actual == success);
        }

        private static IConfigurationRoot GetConfiguration()
        {
            Dictionary<string, string?> configDictionary = new()
            {
                { "TimeZone:UnixTimeZoneId", "America/Vancouver" },
                { "TimeZone:WindowsTimeZoneId", "Pacific Standard Time" },
            };

            return new ConfigurationBuilder()
                .AddInMemoryCollection(configDictionary.ToList())
                .Build();
        }
    }
}
