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
    using FluentValidation;
    using HealthGateway.Common.Data.Validations;
    using HealthGateway.GatewayApi.Models;

    /// <summary>
    /// Validates <see cref="AddDependentRequestValidator"/> instances.
    /// </summary>
    public class AddDependentRequestValidator : AbstractValidator<AddDependentRequest>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AddDependentRequestValidator"/> class.
        /// </summary>
        /// <param name="maxDependantAge">optionally set the maximum age of a dependent, defaults to 12.</param>
        public AddDependentRequestValidator(int maxDependantAge = 12)
        {
            this.RuleFor(v => v.Phn).SetValidator(new PhnValidator());
            this.RuleFor(v => v.DateOfBirth).SetValidator(new DateOfBirthValidator());
            this.RuleFor(v => v.DateOfBirth).SetValidator(new DependentAgeValidator(maxDependentAge: maxDependantAge));
        }
    }
}
