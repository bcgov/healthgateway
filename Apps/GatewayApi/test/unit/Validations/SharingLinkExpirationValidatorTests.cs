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
    using System;
    using HealthGateway.GatewayApi.Validations;
    using Xunit;

    /// <summary>
    /// Validates Expiry Date.
    /// </summary>
    public class SharingLinkExpirationValidatorTests
    {
        /// <summary>
        /// Tests for sharing link expiration validator.
        /// </summary>
        /// <param name="secondsSinceCreation">The number of seconds since expiration.</param>
        /// <param name="success">The value indicates whether the test should succeed or not.</param>
        [Theory]
        [InlineData(0, true)] // 0 Hours
        [InlineData(172800, true)] // 48 hours
        [InlineData(172801, false)] // 48 hours plus 1 second
        public void ValidateSharingLinkExpiration(int secondsSinceCreation, bool success)
        {
            // Arrange
            int expiryHours = 48;
            DateTime now = DateTime.UtcNow;
            DateTime expiryDate = now.AddSeconds(-secondsSinceCreation);

            // Act
            bool actual = SharingLinkExpirationValidator.IsValid(expiryDate, now, expiryHours);

            // Assert
            Assert.True(actual == success);
        }
    }
}
