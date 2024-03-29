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
    public class DateOfBirthValidator : AbstractValidator<DateOnly>
    {
        private readonly DateOnly referenceDate;

        /// <summary>
        /// Initializes a new instance of the <see cref="DateOfBirthValidator"/> class.
        /// </summary>
        /// <param name="referenceDate">A reference point to validate against, defaults to DateTime.Today.</param>
        public DateOfBirthValidator(DateOnly? referenceDate = null)
        {
            referenceDate ??= DateOnly.FromDateTime(DateTime.Today);
            this.referenceDate = referenceDate.Value;

            this.RuleFor(v => v).NotEmpty().Must(this.IsValidInternal).WithMessage("Invalid date");
        }

        private bool IsValidInternal(DateOnly dob)
        {
            return dob <= this.referenceDate;
        }
    }
}
