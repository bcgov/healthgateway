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
namespace HealthGateway.Admin.Client.Store.Analytics;

using System;

/// <summary>
/// The base class for a analytics export load action.
/// </summary>
public abstract class AnalyticsBaseAction
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AnalyticsBaseAction"/> class.
    /// </summary>
    /// <param name="startDate">Optional start date to include in the query.</param>
    /// <param name="endDate">Optional end date to include in the query.</param>
    protected AnalyticsBaseAction(DateTime? startDate, DateTime? endDate)
    {
        this.StartDate = startDate;
        this.EndDate = endDate;
    }

    /// <summary>
    /// Gets or sets start date.
    /// </summary>
    public DateTime? StartDate { get; set; }

    /// <summary>
    /// Gets or sets end date.
    /// </summary>
    public DateTime? EndDate { get; set; }
}
