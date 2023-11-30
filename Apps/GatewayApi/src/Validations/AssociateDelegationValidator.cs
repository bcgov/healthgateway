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
    using HealthGateway.Database.Models;

    /// <summary>
    /// Validates <see cref="Delegation"/> instances.
    /// </summary>
    public class AssociateDelegationValidator : AbstractValidator<Delegation>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AssociateDelegationValidator"/> class.
        /// </summary>
        /// <param name="delegateHdid">The delegate's hdid.</param>
        /// <param name="referenceDate">A reference point to validate against.</param>
        /// <param name="expiryHours">The number of hours valid before expiring.</param>
        public AssociateDelegationValidator(string delegateHdid, DateTime referenceDate, int expiryHours)
        {
            this.RuleFor(v => v.CreatedDateTime).SetValidator(new SharingLinkExpirationValidator(referenceDate, expiryHours));
            this.RuleFor(v => v.ResourceOwnerHdid)
                .NotEqual(delegateHdid)
                .WithMessage("The delegation cannot be associated with oneself.");
            this.When(v => v.ProfileHdid != null, () => this.RuleFor(v => v.ProfileHdid).Equal(delegateHdid).WithMessage("Delegation has already been associated with another profile."));
        }
    }
}
