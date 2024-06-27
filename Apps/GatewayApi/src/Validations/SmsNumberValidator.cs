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

    /// <summary>
    /// Validates SMS numbers for user profiles.
    /// </summary>
    public class SmsNumberValidator : AbstractNullableValidator<string>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SmsNumberValidator"/> class.
        /// </summary>
        public SmsNumberValidator()
        {
            this.RuleFor(smsNumber => smsNumber)
                .Must(smsNumber => PhoneNumberValidator.IsValid(smsNumber))
                .When(smsNumber => !string.IsNullOrEmpty(smsNumber))
                .OverridePropertyName("SmsNumber");
        }
    }
}
