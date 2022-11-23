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
namespace HealthGateway.Common.Data.Utils
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Utilities for interacting with Configurations.
    /// </summary>
    public static class ConfigurationUtility
    {
        /// <summary>
        /// Dynamically constructs a service endpoint, replacing placeholder values with a host and port retrieved from environmental variables.
        /// </summary>
        /// <param name="baseEndpoint">The endpoint string containing ${{serviceHost}} and ${{servicePort}} placeholders.</param>
        /// <param name="hostEnvironmentVariable">The name of the environmental variable containing the host.</param>
        /// <param name="portEnvironmentVariable">The name of the environmental variable containing the port.</param>
        /// <returns>A string containing the computed endpoint.</returns>
        public static string ConstructServiceEndpoint(string baseEndpoint, string hostEnvironmentVariable, string portEnvironmentVariable)
        {
            string? serviceHost = Environment.GetEnvironmentVariable(hostEnvironmentVariable);
            string? servicePort = Environment.GetEnvironmentVariable(portEnvironmentVariable);

            Dictionary<string, string> replacementData = new()
            {
                ["serviceHost"] = serviceHost!,
                ["servicePort"] = servicePort!,
            };

            return StringManipulator.Replace(baseEndpoint, replacementData);
        }
    }
}
