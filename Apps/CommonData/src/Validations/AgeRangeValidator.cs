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
    public class AgeRangeValidator : AbstractValidator<DateTime>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AgeRangeValidator"/> class.
        /// </summary>
        /// <param name="olderThan">Must be older than or at this age.</param>
        /// <param name="youngerThan">Must be younger than this age.</param>
        /// <param name="referenceDate">A reference point for age calculation, defaults to UtcNow.</param>
        public AgeRangeValidator(int? olderThan = null, int? youngerThan = null, DateTime? referenceDate = null)
        {
            if (referenceDate == null)
            {
                referenceDate = DateTime.UtcNow;
            }

            this
                .Transform(v => v, v => CalculateAge(referenceDate.Value.Date, v.Date))
                .GreaterThanOrEqualTo(olderThan ?? 0).When(_ => olderThan.HasValue, ApplyConditionTo.CurrentValidator)
                .LessThan(youngerThan ?? 0).When(_ => youngerThan.HasValue, ApplyConditionTo.CurrentValidator)
                ;
        }

        /// <summary>
        /// Validate a date of birth.
        /// </summary>
        /// <param name="dateOfBirth">date of birth to validate.</param>
        /// <param name="olderThan">optional older than age.</param>
        /// <param name="youngerThan">optional younger than age.</param>
        /// <returns>true if valid, false if not.</returns>
        public static bool IsValid(DateTime dateOfBirth, int? olderThan = null, int? youngerThan = null) => new AgeRangeValidator(olderThan, youngerThan).Validate(dateOfBirth).IsValid;

        private static int CalculateAge(DateTime reference, DateTime dateOfBirth)
        {
            if (reference.Kind == DateTimeKind.Unspecified)
            {
                reference = DateTime.SpecifyKind(reference, DateTimeKind.Utc);
            }

            if (dateOfBirth.Kind == DateTimeKind.Unspecified)
            {
                dateOfBirth = DateTime.SpecifyKind(dateOfBirth, DateTimeKind.Utc);
            }

            // convert to yyyyMMdd integers
            var referenceDateValue = (((reference.Year * 100) + reference.Month) * 100) + reference.Day;
            var dobDateValue = (((dateOfBirth.Year * 100) + dateOfBirth.Month) * 100) + dateOfBirth.Day;

            // calculate age and trim non year values
            return (referenceDateValue - dobDateValue) / 10000;
        }
    }
}
