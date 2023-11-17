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
    public class ExpiryDateValidator : AbstractValidator<DateOnly>
    {
        private readonly DateOnly referenceDate;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExpiryDateValidator"/> class.
        /// </summary>
        /// <param name="referenceDate">A reference point to validate against, defaults to UtcNow.</param>
        public ExpiryDateValidator(DateOnly? referenceDate = null)
        {
            this.referenceDate = referenceDate ?? DateOnly.FromDateTime(DateTime.UtcNow);
            this.RuleFor(v => v).NotEmpty().Must(this.IsValidInternal).WithMessage("Invalid date");
        }

        /// <summary>
        /// Validates an expiry date is not a past date.
        /// </summary>
        /// <param name="expiryDate">Expiry date to validate.</param>
        /// <returns>True if valid, false if not.</returns>
        public static bool IsValid(DateOnly expiryDate)
        {
            return new ExpiryDateValidator().Validate(expiryDate).IsValid;
        }

        private bool IsValidInternal(DateOnly expiryDate)
        {
            bool isValid = expiryDate >= this.referenceDate;
            return isValid;
        }
    }
}
