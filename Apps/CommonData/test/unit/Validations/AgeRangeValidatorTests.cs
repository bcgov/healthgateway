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
    /// Tests for AgeRangeValidator.
    /// </summary>
    public class AgeRangeValidatorTests
    {
        private static readonly DateTime ReferenceDate = new(2022, 12, 21, 0, 0, 0, DateTimeKind.Utc);

        /// <summary>
        /// Tests for AgeRangeValidatorTests.
        /// </summary>
        /// <param name="dob">Date of birth to test.</param>
        /// <param name="youngerThan">Younger than age.</param>
        /// <param name="shouldBeValid">The validation result to verify.</param>
        [Theory]
        [InlineData("2010-12-20", 12, false)]
        [InlineData("2010-12-21", 12, false)]
        [InlineData("2010-12-22", 12, true)]
        public void ValidateYoungerThan(DateTime dob, int youngerThan, bool shouldBeValid)
        {
            AgeRangeValidator validator = new AgeRangeValidator(youngerThan: youngerThan, referenceDate: ReferenceDate);

            ValidationResult? validationResult = validator.Validate(dob);
            Assert.Equal(shouldBeValid, validationResult.IsValid);
        }

        /// <summary>
        /// Tests for AgeRangeValidatorTests.
        /// </summary>
        /// <param name="dob">Date of birth to test.</param>
        /// <param name="olderThan">Older than age.</param>
        /// <param name="shouldBeValid">The validation result to verify.</param>
        [Theory]
        [InlineData("2010-12-20", 12, true)]
        [InlineData("2010-12-21", 12, true)]
        [InlineData("2010-12-22", 12, false)]
        public void ValidateOlderThan(DateTime dob, int olderThan, bool shouldBeValid)
        {
            AgeRangeValidator validator = new AgeRangeValidator(olderThan, referenceDate: ReferenceDate);

            ValidationResult? validationResult = validator.Validate(dob);
            Assert.Equal(shouldBeValid, validationResult.IsValid);
        }

        /// <summary>
        /// Tests for AgeRangeValidatorTests.
        /// </summary>
        /// <param name="dob">Date of birth to test.</param>
        /// <param name="olderThan">Older than age.</param>
        /// <param name="youngerThan">Younger than age.</param>
        /// <param name="shouldBeValid">The validation result to verify.</param>
        [Theory]
        [InlineData("2008-05-03", 12, 14, false)]
        [InlineData("2008-12-20", 12, 14, false)]
        [InlineData("2008-12-21", 12, 14, false)]
        [InlineData("2008-12-22", 12, 14, true)]
        [InlineData("2010-03-05", 12, 14, true)]
        [InlineData("2010-12-20", 12, 14, true)]
        [InlineData("2010-12-21", 12, 14, true)]
        [InlineData("2010-12-22", 12, 14, false)]
        public void ValidateOlderThanAndYoungerThan(DateTime dob, int olderThan, int youngerThan, bool shouldBeValid)
        {
            AgeRangeValidator validator = new AgeRangeValidator(olderThan, youngerThan, ReferenceDate);

            ValidationResult? validationResult = validator.Validate(dob);
            Assert.Equal(shouldBeValid, validationResult.IsValid);
        }
    }
}
