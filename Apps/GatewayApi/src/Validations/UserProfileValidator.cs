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
    using HealthGateway.Common.Data.Models;

    /// <summary>
    /// Validates <see cref="UserProfile"/> instances.
    /// </summary>
    public class UserProfileValidator : AbstractValidator<UserProfile>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UserProfileValidator"/> class.
        /// </summary>
        public UserProfileValidator()
        {
            this.RuleFor(profile => profile.SmsNumber)
                .Must(smsNumber => string.IsNullOrEmpty(smsNumber) || PhoneNumberValidator.IsValid(smsNumber));
        }

        /// <summary>
        /// Convenience method to test sms phone number in the userprofile context.
        /// </summary>
        /// <param name="phoneNumber">Phone number for sms notifications.</param>
        /// <returns>True if UserProfileValidator rules for SmsNumber member pass.</returns>
        public static bool ValidateUserProfileSmsNumber(string? phoneNumber)
        {
            UserProfile tempProfile = new()
            {
                SmsNumber = phoneNumber,
            };
            return new UserProfileValidator().Validate(tempProfile).IsValid;
        }
    }
}
