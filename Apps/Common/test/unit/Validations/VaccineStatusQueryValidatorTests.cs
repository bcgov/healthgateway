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

namespace HealthGateway.CommonTests.Validations
{
    using System;
    using HealthGateway.Common.Models.PHSA;
    using HealthGateway.Common.Validations;
    using Xunit;

    /// <summary>
    /// VaccineStatusQueryValidatorTests unit tests
    /// </summary>
    public class VaccineStatusQueryValidatorTests
    {
        /// <summary>
        /// Assert invalid states.
        /// </summary>
        /// <param name="dob">Date of Birth.</param>
        /// <param name="dov">Date of Vaccine.</param>
        /// <param name="phn">PHN.</param>
        /// <param name="reason">Reason for being invalid.</param>
        [Theory]
        [InlineData("2000-01-01", "2021-01-01", "9735353314", "PHN is invalid")]
        [InlineData("2000-01-01", "2021-01-01", "", "PHN is empty")]
        public void ShouldNotBeValid(DateTime dob, DateTime dov, string phn, string reason)
        {
            var validator = new VaccineStatusQueryValidator();
            var validationResult = validator.Validate(new VaccineStatusQuery
            {
                DateOfBirth = dob,
                DateOfVaccine = dov,
                PersonalHealthNumber = phn,
            });

            Assert.False(validationResult.IsValid, reason);
        }

        /// <summary>
        /// Assert invalid states.
        /// </summary>
        /// <param name="dob">Date of Birth.</param>
        /// <param name="dov">Date of Vaccine.</param>
        /// <param name="phn">PHN.</param>
        [Theory]
        [InlineData("2000-01-01", "2021-01-01", "9735353315")]
        public void ShouldBeValid(DateTime dob, DateTime dov, string phn)
        {
            var validator = new VaccineStatusQueryValidator();
            var validationResult = validator.Validate(new VaccineStatusQuery
            {
                DateOfBirth = dob,
                DateOfVaccine = dov,
                PersonalHealthNumber = phn,
            });

            Assert.True(validationResult.IsValid);
        }
    }
}
