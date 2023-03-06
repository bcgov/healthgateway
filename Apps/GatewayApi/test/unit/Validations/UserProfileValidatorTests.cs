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
namespace HealthGateway.GatewayApiTests.Validations
{
    using HealthGateway.Common.Data.Models;
    using HealthGateway.GatewayApi.Validations;
    using Xunit;

    /// <summary>
    /// <see cref="UserProfileValidator"/> unit tests.
    /// </summary>
    public class UserProfileValidatorTests
    {
        /// <summary>
        /// Test range of valid phone numbers for UserProfile.SmsNumber context.
        /// </summary>
        /// <param name="phoneNumber">Valid phone number.</param>
        /// <param name="comment">Reason for valid result.</param>
        [Theory]
        [InlineData("3345678901", "Valid entry")]
        [InlineData("4345678901", "Valid entry")]
        [InlineData("5345678901", "Valid entry")]
        [InlineData("7345678901", "Valid entry")]
        [InlineData("2507001000", "Valid entry")]
        [InlineData("", "Allow empty strings to clear profile of sms number")]
        [InlineData(null, "Allow null value to clear profile of sms number")]
        public void ShouldBeValid(string? phoneNumber, string comment)
        {
            UserProfile profile = new()
            {
                SmsNumber = phoneNumber,
            };

            // Both methods of validating should result in the same output.
            Assert.True(new UserProfileValidator().Validate(profile).IsValid, comment);
            Assert.True(UserProfileValidator.ValidateUserProfileSmsNumber(phoneNumber), comment);
        }

        /// <summary>
        /// Test range of invalid phone numbers for UserProfile.SmsNumber context.
        /// </summary>
        /// <param name="phoneNumber">Invalid phone numbers.</param>
        /// <param name="reason">Reason that the phone number should be rejected.</param>
        [Theory]
        [InlineData("1031231234", "Begins with 1")]
        [InlineData("0000000000", "All zeros is invalid entry")]
        [InlineData("6345678901", "Not a valid Area code")]
        public void ShouldNotBeValid(string? phoneNumber, string reason)
        {
            UserProfile profile = new()
            {
                SmsNumber = phoneNumber,
            };
            Assert.False(new UserProfileValidator().Validate(profile).IsValid, reason);
        }
    }
}
