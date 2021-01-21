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
namespace HealthGateway.CommonTests.Utils
{
    using System;
    using System.IO;
    using System.Security.Claims;
    using System.Text;
    using HealthGateway.Common.Utils;
    using Newtonsoft.Json;
    using Xunit;

    /// <summary>
    /// Unit Tests for HashGenerator.
    /// </summary>
    public class HashGeneratorTest
    {
        /// <summary>
        /// Should Compute Hash To Guid.
        /// </summary>
        [Fact]
        public void ShouldComputeHashToGuid()
        {
            var aLongString = "17ADDA2E26EE6E8EA930A51577026E9E67FBDF46EC175C8DEFD3EAB08437AE0";
            var generatedGuid = HashGenerator.ComputeHashToGuid(aLongString);
            Assert.NotEqual(generatedGuid.ToString(), Guid.Empty.ToString());
        }
    }
}
