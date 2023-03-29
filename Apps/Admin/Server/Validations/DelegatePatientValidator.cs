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
    /// Validates <see cref="DelegatePatientValidator"/> instances.
    /// </summary>
    public class DelegatePatientValidator : AbstractValidator<PatientModel>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DelegatePatientValidator"/> class.
        /// </summary>
        /// <param name="minDelegateAge">The minimum age of the delegate.</param>
        public DelegatePatientValidator(int minDelegateAge)
        {
            this.RuleFor(v => v.Birthdate).SetValidator(new AgeRangeValidator(olderThan: minDelegateAge));
        }
    }
}
