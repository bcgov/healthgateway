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
namespace HealthGateway.GatewayApiTests.Validations
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using FluentValidation.Results;
    using HealthGateway.Common.Data.Constants;
    using HealthGateway.Common.Data.Utils;
    using HealthGateway.GatewayApi.Models;
    using HealthGateway.GatewayApi.Validations;
    using Microsoft.Extensions.Configuration;
    using Xunit;

    /// <summary>
    /// CreateDelegateInvitationValidator unit tests.
    /// </summary>
    public class CreateDelegateInvitationValidatorTests
    {
        private const string ValidEmail = "delegator@gateway.ca";
        private const string InvalidEmail = "delegator@gateway";
        private const string ValidNickname = "12345678901234567890"; // 20 characters
        private const string InvalidNickname = "123456789012345678901"; // 21 characters
        private static readonly HashSet<DataSource> ValidDataSources = new()
        {
            DataSource.Immunization,
            DataSource.Medication,
        };

        private static readonly HashSet<DataSource> InvalidDataSources = new();

        /// <summary>
        /// Returns Enumerable containing the test cases as arrays of objects.
        /// </summary>
        /// <returns>Test cases as arrays of objects.</returns>
        public static IEnumerable<object[]> TestCases()
        {
            yield return new object[] { ValidEmail, ValidNickname, 1, ValidDataSources, true };
            yield return new object[] { ValidEmail, ValidNickname, 0, ValidDataSources, true };
            yield return new object[] { InvalidEmail, ValidNickname, 1, ValidDataSources, false };
            yield return new object[] { ValidEmail, InvalidNickname, 1, ValidDataSources, false };
            yield return new object[] { ValidEmail, ValidNickname, -1, ValidDataSources, false };
            yield return new object[] { ValidEmail, ValidNickname, 1, InvalidDataSources, false };
        }

        /// <summary>
        /// Validate create delegate invitation requests.
        /// </summary>
        /// <param name="email">Email to validate.</param>
        /// <param name="nickname">Nickname to validate.</param>
        /// <param name="daysToAdd">Number of days to add to create expiry date from utc now.</param>
        /// <param name="dataSources">Data sources to validate.</param>
        /// <param name="success">The value indicates whether the test should succeed or not.</param>
        [Theory]
        [MemberData(nameof(TestCases))]
        public void CreateDelegateInvitationRequestValidator(string email, string nickname, int daysToAdd, HashSet<DataSource> dataSources, bool success)
        {
            TimeZoneInfo localTimezone = DateFormatter.GetLocalTimeZone(GetConfiguration());
            DateOnly referenceDate = DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, localTimezone));

            CreateDelegateInvitationRequest request = new()
            {
                Email = email,
                Nickname = nickname,
                ExpiryDate = DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, localTimezone)).AddDays(daysToAdd),
                DataSources = dataSources,
            };
            ValidationResult result = new CreateDelegateInvitationRequestValidator(referenceDate).Validate(request);
            Assert.True(result.IsValid == success);
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
