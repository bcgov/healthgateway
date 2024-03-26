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
namespace HealthGateway.Common.Data.Tests.Utils
{
    using System;
    using HealthGateway.Common.Data.Utils;
    using Xunit;

    /// <summary>
    /// Unit Tests for ConfigurationUtility.
    /// </summary>
    public class ConfigurationUtilityTests
    {
        /// <summary>
        /// Tests for ConstructServiceEndpoint.
        /// </summary>
        [Fact]
        public void ValidateToEnumString()
        {
            const string expected = "http://dev-hgcdogs-svc:3000/";

            const string baseEndpoint = "http://${serviceHost}:${servicePort}/";
            Environment.SetEnvironmentVariable("serviceHost", "dev-hgcdogs-svc");
            Environment.SetEnvironmentVariable("servicePort", "3000");

            string actual = ConfigurationUtility.ConstructServiceEndpoint(baseEndpoint, "serviceHost", "servicePort");

            Assert.Equal(expected, actual);
        }
    }
}
