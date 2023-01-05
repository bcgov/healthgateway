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

namespace HealthGateway.Common.Data.Validations
{
    using System;
    using FluentValidation;

    /// <summary>
    /// Validates a dependent's age against a maximum value using date of birth.
    /// </summary>
    public class DependentAgeValidator : AbstractValidator<DateTime>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DependentAgeValidator"/> class.
        /// </summary>
        /// <param name="now">DateTime reflecting the relative date to check the age against.</param>
        /// <param name="maxDependentAge">The maximum age of the dependent.</param>
        public DependentAgeValidator(DateTime? now = null, int maxDependentAge = 12)
        {
            this.RuleFor(v => v).SetValidator(new AgeRangeValidator(youngerThan: maxDependentAge, referenceDate: now)).WithMessage($"Dependent age exceeds the maximum limit of {maxDependentAge}");
        }

        /// <summary>
        /// Validates a date of birth against a maximum age.
        /// </summary>
        /// <param name="dateOfBirth">date of birth to validate.</param>
        /// <param name="maxDependentAge">optional maximum age, defaults to 12.</param>
        /// <returns>true if valid, false id not.</returns>
        public static bool IsValid(DateTime dateOfBirth, int maxDependentAge = 12)
        {
            return new DependentAgeValidator(maxDependentAge: maxDependentAge).Validate(dateOfBirth).IsValid;
        }
    }
}
