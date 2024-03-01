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
    using FluentValidation.Results;
    using HealthGateway.Common.Data.Models;
    using HealthGateway.Common.Data.Validations;
    using Xunit;

    /// <summary>
    /// Tests for BroadcastValidatorTests.
    /// </summary>
    public class BroadcastValidatorTests
    {
        private static readonly DateTime Today = new(2022, 12, 21, 0, 0, 0, DateTimeKind.Utc);

        /// <summary>
        /// Gets parameters for broadcast unit test(s).
        /// </summary>
        public static TheoryData<Broadcast, bool> BroadcastTheoryData =>
            new()
            {
                {
                    new Broadcast { ScheduledDateUtc = Today, ExpirationDateUtc = Today },
                    false
                },
                {
                    new Broadcast { ScheduledDateUtc = Today, ExpirationDateUtc = Today.AddDays(-1) },
                    false
                },
                {
                    new Broadcast { ScheduledDateUtc = Today, ExpirationDateUtc = Today.AddDays(1) },
                    true
                },
            };

        /// <summary>
        /// Tests for BroadcastValidatorTests.
        /// </summary>
        /// <param name="broadcast">Broadcast object to test.</param>
        /// <param name="shouldBeValid">The validation result to verify.</param>
        [Theory]
        [MemberData(nameof(BroadcastTheoryData))]
        public void ValidateBroadcast(Broadcast broadcast, bool shouldBeValid)
        {
            BroadcastValidator validator = new();
            ValidationResult? validationResult = validator.Validate(broadcast);
            Assert.Equal(shouldBeValid, validationResult.IsValid);
        }
    }
}
