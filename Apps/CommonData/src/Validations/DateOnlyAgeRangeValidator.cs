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
    /// Validates a minimum and maximum age by date of birth.
    /// </summary>
    public class DateOnlyAgeRangeValidator : AbstractValidator<DateOnly>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DateOnlyAgeRangeValidator"/> class.
        /// </summary>
        /// <param name="olderThan">Must be older than or at this age.</param>
        /// <param name="youngerThan">Must be younger than this age.</param>
        /// <param name="referenceDate">A reference point for age calculation, defaults to UtcNow.</param>
        public DateOnlyAgeRangeValidator(int? olderThan = null, int? youngerThan = null, DateOnly? referenceDate = null)
        {
            referenceDate ??= DateOnly.FromDateTime(DateTime.Today);

            this
                .RuleFor(v => CalculateAge(referenceDate.Value, v))
                .GreaterThanOrEqualTo(olderThan ?? 0)
                .When(_ => olderThan.HasValue, ApplyConditionTo.CurrentValidator)
                .LessThan(youngerThan ?? 0)
                .When(_ => youngerThan.HasValue, ApplyConditionTo.CurrentValidator)
                .OverridePropertyName("Age");
        }

        /// <summary>
        /// Calculates a person's age (in years) at a given date.
        /// </summary>
        /// <param name="referenceDate">Date at which to calculate age.</param>
        /// <param name="dateOfBirth">Date of birth.</param>
        /// <returns>Age in years.</returns>
        public static int CalculateAge(DateOnly referenceDate, DateOnly dateOfBirth)
        {
            // convert to yyyyMMdd integers
            int referenceDateValue = (((referenceDate.Year * 100) + referenceDate.Month) * 100) + referenceDate.Day;
            int dobDateValue = (((dateOfBirth.Year * 100) + dateOfBirth.Month) * 100) + dateOfBirth.Day;

            // calculate age and trim non year values
            return (referenceDateValue - dobDateValue) / 10000;
        }
    }
}
