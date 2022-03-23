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
namespace HealthGateway.Admin.Client.Components;

using Fluxor;
using Fluxor.Blazor.Web.Components;
using HealthGateway.Admin.Client.Store;
using HealthGateway.Admin.Client.Store.Dashboard;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

/// <summary>
/// Backing logic for the RatingSummary component.
/// </summary>
public partial class RatingSummary : FluxorComponent
{
    [Inject]
    private IState<DashboardState> DashboardState { get; set; } = default!;

    private BaseRequestState<IDictionary<string, int>> RatingSummaryResult => this.DashboardState.Value.RatingSummary ?? default!;

    private IDictionary<int, int>? Ratings => this.RatingSummaryResult?.Result?.ToDictionary(r => Convert.ToInt32(r.Key, CultureInfo.InvariantCulture), r => r.Value);

    private int TotalRatings => this.RatingSummaryResult?.Result?.Select(r => r.Value)?.Sum() ?? 0;

    private string AverageRating
    {
        get
        {
            decimal totalScore = this.Ratings
                       ?.Select(r => r.Key * r.Value)
                       ?.Sum() ?? 0;

            return this.TotalRatings != 0 ? (totalScore / this.TotalRatings).ToString("0.00", CultureInfo.InvariantCulture) : "N/A";
        }
    }

    private List<(int? Count, int Percentage)> RatingDetails
    {
        get
        {
            List<(int? Count, int Percentage)>? details = new();
            for (int stars = 1; stars <= 5; stars++)
            {
                int percentage = 0;
                int? count = null;
                if (this.Ratings?.ContainsKey(stars) == true)
                {
                    percentage = this.TotalRatings > 0 ? (100 * this.Ratings[stars] / this.TotalRatings) : 0;
                    count = this.Ratings[stars];
                }

                details.Add((count, percentage));
            }

            return details;
        }
    }
}
