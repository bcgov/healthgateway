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
namespace HealthGateway.Admin.Server.Validations
{
    using FluentValidation;
    using HealthGateway.Common.Data.Validations;
    using HealthGateway.Common.Models;

    /// <summary>
    /// Validates <see cref="DependentPatientValidator"/> instances.
    /// </summary>
    public class DependentPatientValidator : AbstractValidator<PatientModel>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DependentPatientValidator"/> class.
        /// </summary>
        /// <param name="maxDependentAge">The maximum age of the dependent.</param>
        public DependentPatientValidator(int maxDependentAge)
        {
            this.RuleFor(v => v.Birthdate).SetValidator(new AgeRangeValidator(youngerThan: maxDependentAge));
        }
    }
}
