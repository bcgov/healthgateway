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
using System.Diagnostics.CodeAnalysis;
using HealthGateway.Admin.Common.Models;
using HealthGateway.Common.Data.ViewModels;

/// <summary>
/// Static class that implements all actions for the feature.
/// </summary>
[SuppressMessage("Design", "CA1034:Nested types should not be visible", Justification = "Team decision")]
public static class UserFeedbackActions
{
    /// <summary>
    /// The action representing the initiation of a load.
    /// </summary>
    public record LoadAction;

    /// <summary>
    /// The action representing a failed load.
    /// </summary>
    public record LoadFailureAction : BaseFailureAction;

    /// <summary>
    /// The action representing a successful load.
    /// </summary>
    public record LoadSuccessAction : BaseSuccessAction<RequestResult<IEnumerable<UserFeedbackView>>>;

    /// <summary>
    /// The action representing the initiation of an update.
    /// </summary>
    public record UpdateAction
    {
        /// <summary>
        /// Gets the user feedback view.
        /// </summary>
        public required UserFeedbackView UserFeedbackView { get; init; }
    }

    /// <summary>
    /// The action representing a failed update.
    /// </summary>
    public record UpdateFailureAction : BaseFailureAction;

    /// <summary>
    /// The action representing a successful update.
    /// </summary>
    public record UpdateSuccessAction : BaseSuccessAction<RequestResult<UserFeedbackView>>;

    /// <summary>
    /// The action representing the initiation of an associated tag.
    /// </summary>
    public record AssociateTagsAction
    {
        /// <summary>
        /// Gets the tag IDs.
        /// </summary>
        public required IEnumerable<Guid> TagIds { get; init; }

        /// <summary>
        /// Gets the feedback ID.
        /// </summary>
        public required Guid FeedbackId { get; init; }
    }

    /// <summary>
    /// The action representing a failed tag association.
    /// </summary>
    public record AssociateTagsFailureAction : BaseFailureAction;

    /// <summary>
    /// The action representing a successful tag association.
    /// </summary>
    public record AssociateTagsSuccessAction : BaseSuccessAction<RequestResult<UserFeedbackView>>;

    /// <summary>
    /// The action that clears the state.
    /// </summary>
    public record ResetStateAction;
}
