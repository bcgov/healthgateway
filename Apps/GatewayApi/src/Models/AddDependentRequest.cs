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
namespace HealthGateway.GatewayApi.Models
{
    using System;

    /// <summary>
    /// Object that defines the request for registering a dependent.
    /// </summary>
    public class AddDependentRequest
    {
        /// <summary>
        /// Gets or sets the First Name.
        /// </summary>
        public string FirstName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the Last Name.
        /// </summary>
        public string LastName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the Date Of Birth.
        /// </summary>
        public DateTime DateOfBirth { get; set; }

        /// <summary>
        /// Gets or sets the dependent's PHN.
        /// </summary>
        public string PHN { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the TestDate.
        /// </summary>
        public DateTime TestDate { get; set; }
    }
}
