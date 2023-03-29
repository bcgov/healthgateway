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
namespace HealthGateway.Admin.Tests.Validations
{
    using System;
    using FluentValidation.Results;
    using HealthGateway.Admin.Server.Validations;
    using HealthGateway.Common.Models;
    using Xunit;

    /// <summary>
    /// DelegatePatientValidator unit tests.
    /// </summary>
    public class DelegatePatientValidatorTests
    {
        private const int MinDelegateAge = 12;

        /// <summary>
        /// Test range of valid birth dates.
        /// </summary>
        /// <param name="daysToAdd">The number of days(+/-) from utc now minus minimum age(years) used to create a birthdate.</param>
        [Theory]
        [InlineData(0)]
        [InlineData(-100)]
        [InlineData(-365)]
        public void ShouldBeValid(int daysToAdd)
        {
            PatientModel patient = GetPatient(daysToAdd);
            ValidationResult validationResult = new DelegatePatientValidator(MinDelegateAge).Validate(patient);
            Assert.True(validationResult.IsValid);
        }

        /// <summary>
        /// Test range of invalid birth dates.
        /// </summary>
        /// <param name="daysToAdd">The number of days(+/-) from utc now minus minimum age(years) used to create a birthdate.</param>
        [Theory]
        [InlineData(1)]
        [InlineData(100)]
        [InlineData(365)]
        public void ShouldNotBeValid(int daysToAdd)
        {
            PatientModel patient = GetPatient(daysToAdd);
            ValidationResult validationResult = new DelegatePatientValidator(MinDelegateAge).Validate(patient);
            Assert.False(validationResult.IsValid);
        }

        private static PatientModel GetPatient(int daysToAdd)
        {
            // Creating dynamic birthdate where minimum delegate age (years) is minus from current utc date and then days are either added to or subtracted from
            // to create the date.
            DateTime birthDate = DateTime.UtcNow.AddYears(-MinDelegateAge).AddDays(daysToAdd);

            return new()
            {
                Birthdate = birthDate,
            };
        }
    }
}
