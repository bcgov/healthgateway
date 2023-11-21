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
namespace HealthGateway.Common.Data.Tests.Validations
{
    using HealthGateway.Common.Data.Validations;
    using Xunit;

    /// <summary>
    /// Validates Email string.
    /// </summary>
    public class EmailValidatorTests
    {
        /// <summary>
        /// Tests for email validator.
        /// </summary>
        /// <param name="email">The email to validate.</param>
        /// <param name="success">The value indicates whether the test should succeed or not.</param>
        [Theory]
        [InlineData("user@gateway.gov.bc.ca", true)]
        [InlineData("user@gateway.ca", true)]
        [InlineData("user@gateway", false)]
        [InlineData("user@", false)]
        [InlineData("user", false)]
        public void ValidateEmail(string email, bool success)
        {
            bool actual = EmailValidator.IsValid(email);
            Assert.True(actual == success);
        }
    }
}
