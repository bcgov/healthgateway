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

namespace HealthGateway.Laboratory.Validations
{
    using FluentValidation;
    using HealthGateway.Common.Data.Validations;
    using HealthGateway.Laboratory.Models.PHSA;

    /// <summary>
    /// Validates <see cref="PublicLabTestKit"/>.
    /// </summary>
    public class PublicLabTestKitValidator : AbstractValidator<PublicLabTestKit>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PublicLabTestKitValidator"/> class.
        /// </summary>
        public PublicLabTestKitValidator()
        {
            this.RuleFor(v => v.Phn).SetValidator(new PhnValidator()).When(v => !string.IsNullOrEmpty(v.Phn));
            this.RuleFor(v => v.StreetAddress).NotEmpty().When(v => string.IsNullOrEmpty(v.Phn));
            this.RuleFor(v => v.PostalOrZip).NotEmpty().When(v => string.IsNullOrEmpty(v.Phn));
            this.RuleFor(v => v.City).NotEmpty().When(v => string.IsNullOrEmpty(v.Phn));
        }
    }
}
