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
namespace HealthGateway.Admin.Client.Services;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HealthGateway.Admin.Common.Models;
using HealthGateway.Common.Data.ViewModels;
using Refit;

/// <summary>
/// API for interacting with user feedback.
/// </summary>
public interface IUserFeedbackApi
{
    /// <summary>
    /// Gets all user feedback.
    /// </summary>
    /// <returns>The wrapped collection of models.</returns>
    [Get("/")]
    Task<ApiResponse<RequestResult<IEnumerable<UserFeedbackView>>>> GetAll();

    /// <summary>
    /// Associate an existing admin tag to the feedback with matching id.
    /// </summary>
    /// <returns>The added tag model wrapped in a request result.</returns>
    /// <param name="tag">The tag model.</param>
    /// <param name="feedbackId">The feedback id.</param>
    [Put("/{feedbackId}/Tag")]
    Task<ApiResponse<RequestResult<UserFeedbackTagView>>> AssociateTag([Body] AdminTagView tag, Guid feedbackId);

    /// <summary>
    /// Dissociate an existing admin tag from the feedback.
    /// </summary>
    /// <returns>A boolean indicating success or failure of dissociation of tag.</returns>
    /// <param name="feedbackTag">The user feedback tag model.</param>
    /// <param name="feedbackId">The feedback id.</param>
    [Delete("/{feedbackId}/Tag")]
    Task<ApiResponse<PrimitiveRequestResult<bool>>> DissociateTag([Body] UserFeedbackTagView feedbackTag, Guid feedbackId);
}
