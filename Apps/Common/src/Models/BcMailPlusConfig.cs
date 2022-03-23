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
namespace HealthGateway.Common.Models
{
    using System.Collections.Generic;
    using HealthGateway.Common.Data.Utils;

    /// <summary>
    /// Defines the BC Mail Plus Configuration model.
    /// </summary>
    public class BcMailPlusConfig
    {
        /// <summary>
        /// Gets or sets the endpoint for the BCMailPlus integration.
        /// </summary>
        public string Endpoint { get; set; } = null!;

        /// <summary>
        /// Gets or sets the BC Mail Plus server hostname.
        /// </summary>
        public string Host { get; set; } = null!;

        /// <summary>
        /// Gets or sets the BC Mail JOB Environment to use.
        /// </summary>
        public string JobEnvironment { get; set; } = null!;

        /// <summary>
        /// Gets or sets the Job Class to use.
        /// </summary>
        public string JobClass { get; set; } = null!;

        /// <summary>
        /// Gets or sets the authentication token to use.
        /// </summary>
        public string Token { get; set; } = null!;

        /// <summary>
        /// Gets or sets the default time to wait for a new request.
        /// </summary>
        public int BackOffMilliseconds { get; set; }

        /// <summary>
        /// Gets or sets the maximum number of retries.
        /// </summary>
        public int MaxRetries { get; set; }

        /// <summary>
        /// Gets or sets the Schema Version to use.
        /// </summary>
        public string SchemaVersion { get; set; } = null!;

        /// <summary>
        /// Gets or sets the number of minutes to cache asset information.
        /// </summary>
        public int CacheTtl { get; set; }

        /// <summary>
        /// Returns the Endpoint after resolving any variable references.
        /// Endpoint should be something like https://${HOST}/${ENV}/auth=${TOKEN}/JSON/ .
        /// </summary>
        /// <returns>A string representing the Base Endpoint for BCMail Plus.</returns>
        public string ResolvedEndpoint()
        {
            Dictionary<string, string> keyValues = new()
            {
                { "HOST", this.Host },
                { "ENV", this.JobEnvironment },
                { "TOKEN", this.Token },
            };
            return StringManipulator.Replace(this.Endpoint, keyValues)!;
        }
    }
}
