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
    using PhoneNumbers;

    /// <summary>
    /// Class encapsulating validation phone numbers leveraging Google's libphonenumber library.
    /// </summary>
    public class PhoneNumberValidator : AbstractValidator<string>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PhoneNumberValidator"/> class.
        /// </summary>
        /// <param name="region">The region that the phone number should be validated against.</param>
        public PhoneNumberValidator(string region = "US")
        {
            PhoneNumberUtil util = PhoneNumberUtil.GetInstance();
            this.RuleFor(number => number)
                .NotNull()
                .Must(
                    number =>
                    {
                        try
                        {
                            PhoneNumber? phoneNumber = util.Parse(number, region);
                            return util.IsValidNumber(phoneNumber);
                        }
                        catch (Exception e) when (e is NumberParseException)
                        {
                            return false;
                        }
                    });
        }

        /// <summary>
        /// Validate a phone number against a desired region (default: US).
        /// </summary>
        /// <param name="phoneNumber">Phone number for validation.</param>
        /// <param name="region">defaults to the "US" region.</param>
        /// <returns>True if phone number passes the validation for the current region.</returns>
        public static bool IsValid(string? phoneNumber, string region = "US")
        {
            return string.IsNullOrEmpty(phoneNumber) || new PhoneNumberValidator(region).Validate(phoneNumber).IsValid;
        }
    }
}
