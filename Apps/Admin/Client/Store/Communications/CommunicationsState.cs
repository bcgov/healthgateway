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

namespace HealthGateway.Admin.Client.Store.Communications;

using System.Collections.Generic;
using Fluxor;
using HealthGateway.Admin.Client.Models;
using HealthGateway.Admin.Common.Models;
using HealthGateway.Common.Data.ViewModels;

/// <summary>
/// The state for the feature.
/// State should be decorated with [FeatureState] for automatic discovery when services. AddFluxor is called.
/// </summary>
[FeatureState]
public record CommunicationsState
{
    /// <summary>
    /// Gets the request state for adds.
    /// </summary>
    public BaseRequestState<RequestResult<Communication>> Add { get; init; } = new();

    /// <summary>
    /// Gets the request state for loads.
    /// </summary>
    public BaseRequestState<RequestResult<IEnumerable<Communication>>> Load { get; init; } = new();

    /// <summary>
    /// Gets the request state for updates.
    /// </summary>
    public BaseRequestState<RequestResult<Communication>> Update { get; init; } = new();

    /// <summary>
    /// Gets the request state for deletions.
    /// </summary>
    public BaseRequestState<RequestResult<Communication>> Delete { get; init; } = new();

    /// <summary>
    /// Gets the collection of data.
    /// </summary>
    public IList<ExtendedCommunication>? Data { get; init; }

    /// <summary>
    /// Gets the request error if available.
    /// </summary>
    public RequestError? Error { get; init; }

    /// <summary>
    /// Gets a value indicating whether a request is loading.
    /// </summary>
    public bool IsLoading { get; init; }

    /// <summary>
    /// Gets a value indicating whether the data has been loaded.
    /// </summary>
    public bool Loaded => this.Data != null;
}
