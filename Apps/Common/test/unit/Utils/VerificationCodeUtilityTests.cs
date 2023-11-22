//-------------------------------------------------------------------------
// Copyright © 2019 Province of British Columbia
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//-------------------------------------------------------------------------
namespace HealthGateway.CommonTests.Utils
{
    using HealthGateway.Common.Utils;
    using Xunit;

    /// <summary>
    /// VerificationCodeUtility's Unit Tests.
    /// </summary>
    public class VerificationCodeUtilityTests
    {
        /// <summary>
        /// Should generate verification code.
        /// </summary>
        [Fact]
        public void GenerateVerificationCode()
        {
            // Setup and Act
            string verificationCode = VerificationCodeUtility.Generate();

            // Verify
            Assert.NotNull(verificationCode);
            Assert.True(verificationCode.Length == 6);
            Assert.True(IsPositiveInteger(verificationCode));
        }

        // Check if the string is a positive numeric value
        private static bool IsPositiveInteger(string input)
        {
            if (int.TryParse(input, out int result))
            {
                return result > 0;
            }

            return false;
        }
    }
}
