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
namespace HealthGateway.Immunization.Models
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// The Immunization record data model.
    /// </summary>
    [Obsolete("Use Immunization instead. This will need to be removed once we have refactored the Controller, Service and Delegate")]
    public class ImmunizationView
    {
        /// <summary>
        /// Gets or sets the Immunization id.
        /// </summary>
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the Immunization name.
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// Gets or sets the Immunization status (completed | not-done).
        /// </summary>
        public string Status { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the Immunization occurence date time.
        /// </summary>
        public DateTime OccurrenceDateTime { get; set; } = System.DateTime.MinValue;

        /// <summary>
        /// Gets the List of Immunization agents.
        /// </summary>
        public List<ImmunizationAgent> ImmunizationAgents { get; } = new List<ImmunizationAgent>();
    }
}
