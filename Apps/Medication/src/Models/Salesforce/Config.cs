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
namespace HealthGateway.Medication.Models.Salesforce
{
    using System;
    using HealthGateway.Common.AccessManagement.Authentication.Models;

    /// <summary>
    /// Provides configuration data for the Salesforce delegate.
    /// </summary>
    public class Config
    {
        /// <summary>
        /// The key used to lookup Salesforce configuration.
        /// </summary>
        public const string SalesforceConfigSectionKey = "Salesforce";

        /// <summary>
        /// Gets or sets the Salesforce external endpoint.
        /// </summary>
        public Uri Endpoint { get; set; } = null!;

        /// <summary>
        /// Gets or sets the URI for the client credentials grant.
        /// </summary>
        public Uri TokenUri { get; set; } = null!;

        /// <summary>
        /// Gets or sets the parameters for the client credentials grant.
        /// </summary>
        public ClientCredentialsRequestParameters ClientAuthentication { get; set; } = new();
    }
}
