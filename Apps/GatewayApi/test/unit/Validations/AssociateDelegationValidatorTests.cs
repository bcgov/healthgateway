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
    using FluentValidation.Results;
    using HealthGateway.Database.Models;
    using HealthGateway.GatewayApi.Validations;
    using Xunit;

    /// <summary>
    /// AssociateDelegationValidator unit tests.
    /// </summary>
    public class AssociateDelegationValidatorTests
    {
        private const string DelegateHdid = "DEV4FPEGCXG2NB5K2USBL52S66SC3GOUHWRP3GTXR2BTY5HEC4YA";
        private const string ProfileHdid = "GO4DOSMRJ7MFKPPADDZ3FK2MOJ45SFKONJWR67XNLMZQFNEHDKDA";
        private const string ResourceOwnerHdid = "P6FFO433A5WPMVTGM7T4ZVWBKCSVNAYGTWTU3J2LWMGUMERKI72A";

        /// <summary>
        /// Validate associating a delegation.
        /// </summary>
        /// <param name="resourceOwnerHdid">The delegation's resource owner hdid.</param>
        /// <param name="delegateHdid">The delegate's hdid.</param>
        /// <param name="profileHdid">The profile hdid stored in Delegation.</param>
        /// <param name="secondsToExpiration">The number of seconds to expiration.</param>
        /// <param name="success">The value indicates whether the test should succeed or not.</param>
        [Theory]
        [InlineData(ResourceOwnerHdid, DelegateHdid, null, 0, true)] // 0 Hours
        [InlineData(ResourceOwnerHdid, DelegateHdid, null, -172800, true)] // 48 hours
        [InlineData(ResourceOwnerHdid, DelegateHdid, null, -172801, false)] // 48 hours plus 1 second
        [InlineData(ResourceOwnerHdid, ResourceOwnerHdid, null, 0, false)]
        [InlineData(ResourceOwnerHdid, DelegateHdid, ProfileHdid, 0, false)]
        public void AssociateDelegationValidator(string resourceOwnerHdid, string delegateHdid, string? profileHdid, int secondsToExpiration, bool success)
        {
            // Arrange
            int expiryHours = 48;
            DateTime now = DateTime.UtcNow;
            DateTime createdDateTime = now.AddSeconds(secondsToExpiration);

            Delegation delegation = new()
            {
                Id = Guid.NewGuid(),
                ResourceOwnerHdid = resourceOwnerHdid,
                ProfileHdid = profileHdid,
                CreatedDateTime = createdDateTime,
            };

            // Act
            ValidationResult result = new AssociateDelegationValidator(delegateHdid, now, expiryHours).Validate(delegation);

            // Verify
            Assert.True(result.IsValid == success);
        }
    }
}
