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
    using FluentValidation.Results;
    using HealthGateway.GatewayApi.Validations;
    using Xunit;

    /// <summary>
    /// AssociateDelegationValidator unit tests.
    /// </summary>
    public class AssociateDelegationRequestValidatorTests
    {
        private const string EncryptedDelegationId =
            "CfDJ8LLY0gwTcqdJoMgQs9OOAXd7dzOHlGh2YP-gemIPfyfqR4_7Igrj4B85s2bvgYSnrCgOPFHP8C0pG2oMTfrU2qxyT0BdMMET_MbEii514O4HtalNmPCVHl1dDLFyruuBA3RwRyC70uoVxZtpavV4tRRGd9lAq5VNwVWNez4kuvfC";

        private const string EmptyEncryptedDelegationId = "";

        /// <summary>
        /// Validate associating a delegation request.
        /// </summary>
        /// <param name="encryptedDelegationId">The delegation's encrypted delegation Id.</param>
        /// <param name="success">The value indicates whether the test should succeed or not.</param>
        [Theory]
        [InlineData(EncryptedDelegationId, true)]
        [InlineData(EmptyEncryptedDelegationId, false)]
        public void AssociateDelegationRequestValidator(string encryptedDelegationId, bool success)
        {
            // Arrange and Act
            ValidationResult result = new AssociateDelegationRequestValidator().Validate(encryptedDelegationId);

            // Verify
            Assert.True(result.IsValid == success);
        }
    }
}
