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
    using System.Net.Mail;
    using FluentValidation;
    using HealthGateway.Common.Data.Validations;

    /// <summary>
    /// Class encapsulating validation for email addresses.
    /// </summary>
    public class OptionalEmailAddressValidator : AbstractNullableValidator<string>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OptionalEmailAddressValidator"/> class.
        /// </summary>
        public OptionalEmailAddressValidator()
        {
            this.RuleFor(emailAddress => emailAddress)
                .Must(
                    address =>
                    {
                        try
                        {
                            MailAddress addr = new(address);
                            return addr.Address == address;
                        }
#pragma warning disable CA1031 // Do not catch general exception types
                        catch
                        {
                            return false;
                        }
                    })
                .When(emailAddress => !string.IsNullOrEmpty(emailAddress))
                .OverridePropertyName("EmailAddress");
        }
    }
}
