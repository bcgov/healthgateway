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
    using HealthGateway.Common.Data.Validations;
    using HealthGateway.GatewayApi.Models;

    /// <summary>
    /// Validates <see cref="CreateDelegationRequest"/> instances.
    /// </summary>
    public class CreateDelegationRequestValidator : AbstractValidator<CreateDelegationRequest>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CreateDelegationRequestValidator"/> class.
        /// </summary>
        /// <param name="referenceDate">A reference point to validate against.</param>
        public CreateDelegationRequestValidator(DateOnly referenceDate)
        {
            this.RuleFor(v => v.Nickname).NotEmpty().MaximumLength(20);
            this.RuleFor(v => v.Email).NotEmpty().SetValidator(new EmailValidator());
            this.RuleFor(v => v.ExpiryDate).SetValidator(new OptionalExpiryDateValidator(referenceDate));
            this.RuleFor(v => v.DataSources)
                .Must(dataSources => dataSources.Count > 0)
                .WithMessage("DataSources must have at least one item.");
        }
    }
}
