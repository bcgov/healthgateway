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
    /// Validates date of birth.
    /// </summary>
    public class DateOfBirthValidator : AbstractValidator<DateTime>
    {
        private readonly DateTime referenceDate;

        /// <summary>
        /// Initializes a new instance of the <see cref="DateOfBirthValidator"/> class.
        /// </summary>
        /// <param name="referenceDate">A reference point to validate against, defaults to UtcNow.</param>
        public DateOfBirthValidator(DateTime? referenceDate = null)
        {
            referenceDate ??= DateTime.UtcNow;
            this.referenceDate = referenceDate.Value;

            this.RuleFor(v => v).NotEmpty().Must(this.IsValidInternal).WithMessage("Invalid date");
        }

        /// <summary>
        /// Validates a date of birth is not a future date.
        /// </summary>
        /// <param name="dateOfBirth">Date of birth to validate.</param>
        /// <returns>True if valid, false if not.</returns>
        public static bool IsValid(DateTime dateOfBirth)
        {
            return new DateOfBirthValidator().Validate(dateOfBirth).IsValid;
        }

        private bool IsValidInternal(DateTime dob)
        {
            bool isValid = dob.Date <= this.referenceDate.Date;
            return isValid;
        }
    }
}
