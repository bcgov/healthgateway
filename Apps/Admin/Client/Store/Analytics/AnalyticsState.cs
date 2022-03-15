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

using Fluxor;

/// <summary>
/// The state for the feature.
/// State should be decorated with [FeatureState] for automatic discovery when services. AddFluxor is called.
/// </summary>
[FeatureState]
public record AnalyticsState
{
    /// <summary>
    /// Gets the user profiles report.
    /// </summary>
    public ReportState UserProfilesReport { get; init; } = new ReportState();

    /// <summary>
    /// Gets the comments report.
    /// </summary>
    public ReportState CommentsReport { get; init; } = new ReportState();

    /// <summary>
    /// Gets the notes report.
    /// </summary>
    public ReportState NotesReport { get; init; } = new ReportState();

    /// <summary>
    /// Gets the ratings report.
    /// </summary>
    public ReportState RatingsReport { get; init; } = new ReportState();

    /// <summary>
    /// Gets the inactive users report.
    /// </summary>
    public ReportState InactiveUsersReport { get; init; } = new ReportState();
}
