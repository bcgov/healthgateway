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
    using HealthGateway.GatewayApi.Validations;
    using Xunit;

    /// <summary>
    /// PhoneNumberValidator unit tests.
    /// </summary>
    public class PhoneNumberValidatorTests
    {
        /// <summary>
        /// Test range of valid phone numbers.
        /// </summary>
        /// <param name="phoneNumber">Valid phone number.</param>
        [Theory]
        [InlineData("3345678901")]
        [InlineData("4345678901")]
        [InlineData("5345678901")]
        [InlineData("7345678901")]
        [InlineData("2507001000")]
        public void ShouldBeValid(string? phoneNumber)
        {
            Assert.True(PhoneNumberValidator.IsValid(phoneNumber));
        }

        /// <summary>
        /// Test range of invalid phone numbers.
        /// </summary>
        /// <param name="phoneNumber">Invalid phone numbers.</param>
        /// <param name="reason">Reason that the phone number should be rejected.</param>
        [Theory]
        [InlineData("1031231234", "Begins with 1")]
        [InlineData("0000000000", "All zeros is invalid format")]
        [InlineData("6345678901", "Not a valid Area code")]
        [InlineData("2031231234", "Not a valid Area code")]
        [InlineData("", "Empty string is not a phone number")]
        [InlineData(null, "null is not a phone number")]
        public void ShouldNotBeValid(string? phoneNumber, string reason)
        {
            Assert.False(PhoneNumberValidator.IsValid(phoneNumber), reason);
        }
    }
}
