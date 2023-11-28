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
    /// Validates expiry date.
    /// </summary>
    public class OptionalExpiryDateValidator : AbstractValidator<DateOnly?>
    {
        private readonly DateOnly referenceDate;

        /// <summary>
        /// Initializes a new instance of the <see cref="OptionalExpiryDateValidator"/> class.
        /// </summary>
        /// <param name="referenceDate">A reference point to validate against.</param>
        public OptionalExpiryDateValidator(DateOnly referenceDate)
        {
            this.referenceDate = referenceDate;
            this.RuleFor(v => v).Must(this.IsValidInternal).WithMessage("Invalid date").When(v => v != null);
        }

        /// <summary>
        /// Validates an expiry date is not a past date.
        /// </summary>
        /// <param name="expiryDate">Expiry date to validate.</param>
        /// <param name="referenceDate">A reference point to validate against.</param>
        /// <returns>True if valid, false if not.</returns>
        public static bool IsValid(DateOnly expiryDate, DateOnly referenceDate)
        {
            return new OptionalExpiryDateValidator(referenceDate).Validate(expiryDate).IsValid;
        }

        private bool IsValidInternal(DateOnly? expiryDate)
        {
            bool isValid = expiryDate >= this.referenceDate;
            return isValid;
        }
    }
}
