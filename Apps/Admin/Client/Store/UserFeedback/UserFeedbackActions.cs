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
    public class LoadAction
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LoadAction"/> class.
        /// </summary>
        public LoadAction()
        {
        }
    }

    /// <summary>
    /// The action representing a failed load.
    /// </summary>
    public class LoadFailAction : BaseFailAction
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LoadFailAction"/> class.
        /// </summary>
        /// <param name="error">The request error.</param>
        public LoadFailAction(RequestError error)
            : base(error)
        {
        }
    }

    /// <summary>
    /// The action representing a successful load.
    /// </summary>
    public class LoadSuccessAction : BaseSuccessAction<RequestResult<IEnumerable<UserFeedbackView>>>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LoadSuccessAction"/> class.
        /// </summary>
        /// <param name="requestResultModel">User feedback view data.</param>
        public LoadSuccessAction(RequestResult<IEnumerable<UserFeedbackView>> requestResultModel)
            : base(requestResultModel)
        {
        }
    }

    /// <summary>
    /// The action representing the initiation of an update.
    /// </summary>
    public class UpdateAction
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UpdateAction"/> class.
        /// </summary>
        /// <param name="userFeedbackView">Represents the user feedback model.</param>
        public UpdateAction(UserFeedbackView userFeedbackView)
        {
            this.UserFeedbackView = userFeedbackView;
        }

        /// <summary>
        /// Gets or sets the user feedback view.
        /// </summary>
        public UserFeedbackView UserFeedbackView { get; set; }
    }

    /// <summary>
    /// The action representing a failed update.
    /// </summary>
    public class UpdateFailAction : BaseFailAction
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UpdateFailAction"/> class.
        /// </summary>
        /// <param name="error">The request error.</param>
        public UpdateFailAction(RequestError error)
            : base(error)
        {
        }
    }

    /// <summary>
    /// The action representing a successful update.
    /// </summary>
    public class UpdateSuccessAction : BaseSuccessAction<RequestResult<UserFeedbackView>>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UpdateSuccessAction"/> class.
        /// </summary>
        /// <param name="requestResultModel">User feedback view data.</param>
        public UpdateSuccessAction(RequestResult<UserFeedbackView> requestResultModel)
            : base(requestResultModel)
        {
        }
    }

    /// <summary>
    /// The action representing the initiation of an associated tag.
    /// </summary>
    public class AssociateTagsAction
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AssociateTagsAction"/> class.
        /// </summary>
        /// <param name="tagIds">The tag IDs.</param>
        /// <param name="feedbackId">The feedback ID.</param>
        public AssociateTagsAction(IList<Guid> tagIds, Guid feedbackId)
        {
            this.TagIds = tagIds;
            this.FeedbackId = feedbackId;
        }

        /// <summary>
        /// Gets the tag IDs.
        /// </summary>
        public IList<Guid> TagIds { get; }

        /// <summary>
        /// Gets or sets the feedback ID.
        /// </summary>
        public Guid FeedbackId { get; set; }
    }

    /// <summary>
    /// The action representing a failed tag association.
    /// </summary>
    public class AssociateTagsFailAction : BaseFailAction
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AssociateTagsFailAction"/> class.
        /// </summary>
        /// <param name="error">The request error.</param>
        public AssociateTagsFailAction(RequestError error)
            : base(error)
        {
        }
    }

    /// <summary>
    /// The action representing a successful tag association.
    /// </summary>
    public class AssociateTagsSuccessAction : BaseSuccessAction<RequestResult<UserFeedbackView>>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AssociateTagsSuccessAction"/> class.
        /// </summary>
        /// <param name="data">User feedback view data.</param>
        public AssociateTagsSuccessAction(RequestResult<UserFeedbackView> data)
            : base(data)
        {
        }
    }

    /// <summary>
    /// The action that clears the state.
    /// </summary>
    public class ResetStateAction
    {
    }
}
