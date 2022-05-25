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
    /// The action representing the initiation of an associated tag.
    /// </summary>
    public class AssociateTagAction
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AssociateTagAction"/> class.
        /// </summary>
        /// <param name="adminTag">Represents the AdminTagView model.</param>
        /// <param name="feedbackId">The feedback id.</param>
        public AssociateTagAction(AdminTagView adminTag, Guid feedbackId)
        {
            this.AdminTag = adminTag;
            this.FeedbackId = feedbackId;
        }

        /// <summary>
        /// Gets or sets the admin tag view.
        /// </summary>
        public AdminTagView AdminTag { get; set; }

        /// <summary>
        /// Gets or sets feedback id.
        /// </summary>
        public Guid FeedbackId { get; set; }
    }

    /// <summary>
    /// The action representing a failed associate tag.
    /// </summary>
    public class AssociateTagFailAction : BaseFailAction
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AssociateTagFailAction"/> class.
        /// </summary>
        /// <param name="error">The request error.</param>
        public AssociateTagFailAction(RequestError error)
            : base(error)
        {
        }
    }

    /// <summary>
    /// The action representing a successful associated tag.
    /// </summary>
    public class AssociateTagSuccessAction : BaseSuccessAction<RequestResult<UserFeedbackTagView>>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AssociateTagSuccessAction"/> class.
        /// </summary>
        /// <param name="data">User feedback view data.</param>
        public AssociateTagSuccessAction(RequestResult<UserFeedbackTagView> data)
            : base(data)
        {
        }
    }

    /// <summary>
    /// The action representing the initiation of a disassociated tag.
    /// </summary>
    public class DisassociateTagAction
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DisassociateTagAction"/> class.
        /// </summary>
        /// <param name="feedbackTag">Represents the user feedback tag view model.</param>
        /// <param name="feedbackId">The feedback id.</param>
        public DisassociateTagAction(UserFeedbackTagView feedbackTag, Guid feedbackId)
        {
            this.FeedbackTag = feedbackTag;
            this.FeedbackId = feedbackId;
        }

        /// <summary>
        /// Gets or sets the user feedback tag view.
        /// </summary>
        public UserFeedbackTagView FeedbackTag { get; set; }

        /// <summary>
        /// Gets or sets the feedback id.
        /// </summary>
        public Guid FeedbackId { get; set; }
    }

    /// <summary>
    /// The action representing a failed disassociated tag.
    /// </summary>
    public class DisassociateTagFailAction : BaseFailAction
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DisassociateTagFailAction"/> class.
        /// </summary>
        /// <param name="error">The request error.</param>
        public DisassociateTagFailAction(RequestError error)
            : base(error)
        {
        }
    }

    /// <summary>
    /// The action representing a successful disassociated tag.
    /// </summary>
    public class DisassociateTagSuccessAction
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DisassociateTagSuccessAction"/> class.
        /// </summary>
        /// <param name="result">True if the disassociation was successful.</param>
        /// <param name="id">The id for user feedback tag to delete.</param>
        public DisassociateTagSuccessAction(PrimitiveRequestResult<bool> result, Guid id)
        {
            this.Result = result;
            this.Id = id;
        }

        /// <summary>
        /// Gets or sets the result.
        /// </summary>
        public PrimitiveRequestResult<bool> Result { get; set; }

        /// <summary>
        /// Gets or sets the id.
        /// </summary>
        public Guid Id { get; set; }
    }

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
    /// The action that clears the state.
    /// </summary>
    public class ResetStateAction
    {
    }
}
