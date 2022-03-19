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

using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

/// <summary>
/// Backing logic for the HgRatingDetails component.
/// </summary>
public partial class HgRatingDetails : HgComponentBase
{
    /// <summary>
    /// Gets the integer value of IconIndex.
    /// </summary>
    public int IconIndexCount
    {
        get
        {
            return this.IconIndex != null && this.IconIndex.Length > 0 ? int.Parse(this.IconIndex, CultureInfo.InvariantCulture) : 0;
        }
    }

    /// <summary>
    /// Gets the ProgressLinearValue.
    /// </summary>
    public int ProgressLinearValue
    {
        get
        {
            if (this.RatingSummary != null)
            {
                var result = from rating in this.RatingSummary
                             where rating.Item1 == this.IconIndex && rating.Item2 > 0
                             select rating.Item2;
                return result.FirstOrDefault();
            }

            return 0;
        }
    }

    /// <summary>
    /// Gets the RatingTotal.
    /// </summary>
    public int RatingTotal
    {
        get
        {
            if (this.RatingSummary != null)
            {
                var result = from rating in this.RatingSummary
                             where rating.Item1 == this.IconIndex
                             select rating.Item3;
                return result.FirstOrDefault();
            }

            return 0;
        }
    }

    /// <summary>
    /// Gets or sets the child content of this component.
    /// </summary>
    [Parameter]
    public string? IconIndex { get; set; }

    /// <summary>
    /// Gets or sets the RatingSummary.
    /// </summary>
    [Parameter]
#pragma warning disable CA2227 // Collection properties should be read only
    public List<Tuple<string, int, int>>? RatingSummary { get; set; }
#pragma warning restore CA2227 // Collection properties should be read only
}
