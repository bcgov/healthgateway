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

using HealthGateway.Common.Data.Models;

/// <summary>
/// A system broadcast with additional state information.
/// </summary>
public class ExtendedBroadcast : Broadcast
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ExtendedBroadcast"/> class.
    /// </summary>
    /// <param name="model">The broadcast model.</param>
    public ExtendedBroadcast(Broadcast model)
    {
        this.PopulateFromModel(model);
    }

    /// <summary>
    /// Gets or sets a value indicating whether the broadcast details have been expanded.
    /// </summary>
    public bool IsExpanded { get; set; }

    /// <summary>
    /// Populates all properties from a broadcast model.
    /// </summary>
    /// <param name="model">The broadcast model.</param>
    public void PopulateFromModel(Broadcast model)
    {
        this.Id = model.Id;
        this.CategoryName = model.CategoryName;
        this.DisplayText = model.DisplayText;
        this.Enabled = model.Enabled;
        this.ActionType = model.ActionType;
        this.ActionUrl = model.ActionUrl;
        this.ScheduledDateUtc = model.ScheduledDateUtc;
        this.ExpirationDateUtc = model.ExpirationDateUtc;
    }
}
