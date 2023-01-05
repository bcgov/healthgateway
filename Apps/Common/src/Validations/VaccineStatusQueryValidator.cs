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

namespace HealthGateway.Common.Validations
{
    using FluentValidation;
    using HealthGateway.Common.Data.Validations;
    using HealthGateway.Common.Models.PHSA;

    /// <summary>
    /// Validation rules for <see cref="VaccineStatusQuery"/>.
    /// </summary>
    public class VaccineStatusQueryValidator : AbstractValidator<VaccineStatusQuery>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="VaccineStatusQueryValidator"/> class.
        /// </summary>
        public VaccineStatusQueryValidator()
        {
            this.RuleFor(v => v.PersonalHealthNumber).NotEmpty().SetValidator(new PhnValidator());
        }
    }
}
