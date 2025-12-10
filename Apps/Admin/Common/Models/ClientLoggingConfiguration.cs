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
namespace HealthGateway.Admin.Common.Models
{
    using System.Collections.Generic;

    /// <summary>
    /// Configuration to be used by external clients for logging behavior.
    /// </summary>
    public class ClientLoggingConfiguration
    {
        /// <summary>
        /// Gets the log level settings for the client, where each key represents
        /// a logging category (e.g., "Default", "Microsoft") and the value specifies
        /// the minimum log level to apply (e.g., "Information", "Warning").
        /// </summary>
        public Dictionary<string, string>? LogLevel { get; init; }
    }
}
