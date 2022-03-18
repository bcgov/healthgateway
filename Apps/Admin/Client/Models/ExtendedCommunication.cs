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

namespace HealthGateway.Admin.Client.Models;

using HealthGateway.Admin.Common.Models;

/// <summary>
/// A system communication with additional state information.
/// </summary>
public class ExtendedCommunication : Communication
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ExtendedCommunication"/> class.
    /// </summary>
    /// <param name="model">The communication model.</param>
    public ExtendedCommunication(Communication model)
    {
        this.Id = model.Id;
        this.Text = model.Text;
        this.Subject = model.Subject;
        this.EffectiveDateTime = model.EffectiveDateTime;
        this.ExpiryDateTime = model.ExpiryDateTime;
        this.ScheduledDateTime = model.ScheduledDateTime;
        this.CommunicationTypeCode = model.CommunicationTypeCode;
        this.CommunicationStatusCode = model.CommunicationStatusCode;
        this.Priority = model.Priority;
        this.CreatedBy = model.CreatedBy;
        this.CreatedDateTime = model.CreatedDateTime;
        this.UpdatedBy = model.UpdatedBy;
        this.UpdatedDateTime = model.UpdatedDateTime;
        this.Version = model.Version;
    }

    /// <summary>
    /// Gets or sets a value indicating whether the communication details have been expanded.
    /// </summary>
    public bool IsExpanded { get; set; }
}
