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
namespace HealthGateway.Common.Data.Models
{
    using System;

    /// <summary>
    /// Interface representing audit fields to be recorded in all DB entities.
    /// </summary>
    public interface IAuditable
    {
        /// <summary>
        /// Gets or sets the The audit created by field.
        /// </summary>
        string CreatedBy { get; set; }

        /// <summary>
        /// Gets or sets the audit created date/time field.
        /// </summary>
        DateTime CreatedDateTime { get; set; }

        /// <summary>
        /// Gets or sets the audit updated by field.
        /// </summary>
        string UpdatedBy { get; set; }

        /// <summary>
        /// Gets or sets the audit updated date/time.
        /// </summary>
        DateTime UpdatedDateTime { get; set; }
    }
}
