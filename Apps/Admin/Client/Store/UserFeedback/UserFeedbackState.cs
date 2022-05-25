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
namespace HealthGateway.Admin.Client.Store.UserFeedback;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Fluxor;
using HealthGateway.Admin.Common.Models;
using HealthGateway.Common.Data.ViewModels;

/// <summary>
/// The state for the feature.
/// State should be decorated with [FeatureState] for automatic discovery when services.AddFluxor is called.
/// </summary>
[FeatureState]
public record UserFeedbackState
{
    /// <summary>
    /// Gets the request state for associate tag to user feedback.
    /// </summary>
    public BaseRequestState<RequestResult<UserFeedbackTagView>> AssociateTag { get; init; } = new();

    /// <summary>
    /// Gets the request state for disassociate tag from user feedback tag.
    /// </summary>
    public BaseRequestState<PrimitiveRequestResult<bool>> DisassociateTag { get; init; } = new();

    /// <summary>
    /// Gets the request state for loading user feedback.
    /// </summary>
    public BaseRequestState<RequestResult<IEnumerable<UserFeedbackView>>> Load { get; init; } = new();

    /// <summary>
    /// Gets the collection of user feedback data.
    /// </summary>
    public IImmutableDictionary<Guid, UserFeedbackView>? FeedbackData { get; init; }

    /// <summary>
    /// Gets the collection of user feedback tag data.
    /// </summary>
    public IImmutableDictionary<Guid, UserFeedbackTagView>? FeedbackTagData { get; init; }
}
