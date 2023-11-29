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
namespace HealthGateway.GatewayApi.Validations
{
    using System;
    using FluentValidation;

    /// <summary>
    /// Validates expiry date.
    /// </summary>
    public class SharingLinkExpirationValidator : AbstractValidator<DateTime>
    {
        private readonly DateTime referenceDate;
        private readonly int expiryHours;

        /// <summary>
        /// Initializes a new instance of the <see cref="SharingLinkExpirationValidator"/> class.
        /// </summary>
        /// <param name="referenceDate">A reference point to validate against.</param>
        /// <param name="expiryHours">The number of hours valid before expiring.</param>
        public SharingLinkExpirationValidator(DateTime referenceDate, int expiryHours)
        {
            this.referenceDate = referenceDate;
            this.expiryHours = expiryHours;
            this.RuleFor(v => v).Must(this.IsValidInternal).WithMessage("Sharing link has expired.");
        }

        /// <summary>
        /// Validates date is not past x number of hours.
        /// </summary>
        /// <param name="expiryDate">Expiry date to validate.</param>
        /// <param name="referenceDate">A reference point to validate against.</param>
        /// <param name="expiryHours">The number of hours valid before expiring.</param>
        /// <returns>True if valid, false if not.</returns>
        public static bool IsValid(DateTime expiryDate, DateTime referenceDate, int expiryHours)
        {
            return new SharingLinkExpirationValidator(referenceDate, expiryHours).Validate(expiryDate).IsValid;
        }

        private bool IsValidInternal(DateTime dateTime)
        {
            TimeSpan timeDifference = this.referenceDate - dateTime;
            return Math.Abs(timeDifference.TotalHours) <= this.expiryHours;
        }
    }
}
