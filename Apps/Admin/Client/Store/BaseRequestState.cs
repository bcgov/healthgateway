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

namespace HealthGateway.Admin.Client.Store;

/// <summary>
/// A state holding information relating to an HTTP request.
/// </summary>
/// <typeparam name="TModel">The type of the model returned by the request.</typeparam>
public record BaseRequestState<TModel>
    where TModel : class
{
    /// <summary>
    /// Gets the result.
    /// </summary>
    public TModel? Result { get; init; }

    /// <summary>
    /// Gets the request error if available.
    /// </summary>
    public RequestError? Error { get; init; }

    /// <summary>
    /// Gets a value indicating whether the request is loading.
    /// </summary>
    public bool IsLoading { get; init; }

    /// <summary>
    /// Gets a value indicating whether the request has been loaded.
    /// </summary>
    public bool Loaded => this.Result != null;
}
