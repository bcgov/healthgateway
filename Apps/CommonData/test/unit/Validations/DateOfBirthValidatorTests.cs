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
    using HealthGateway.Common.Data.Validations;
    using Xunit;

    /// <summary>
    /// Tests for DateOfBirthValidator.
    /// </summary>
    public class DateOfBirthValidatorTests
    {
        private static readonly DateTime ReferenceDate = new(2022, 12, 21, 0, 0, 0, DateTimeKind.Utc);

        /// <summary>
        /// Tests for DateOfBirthValidatorTests.
        /// </summary>
        /// <param name="dob">Date of birth to test.</param>
        /// <param name="shouldBeValid">The validation result to verify.</param>
        [Theory]
        [InlineData("2022-12-20", true)]
        [InlineData("2022-12-21", true)]
        [InlineData("2022-12-22", false)]
        public void ValidateNotFutureDate(DateTime dob, bool shouldBeValid)
        {
            DateOfBirthValidator validator = new(referenceDate: ReferenceDate);

            ValidationResult? validationResult = validator.Validate(dob);
            Assert.Equal(shouldBeValid, validationResult.IsValid);
        }
    }
}
