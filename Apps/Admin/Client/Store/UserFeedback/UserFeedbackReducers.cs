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
using System.Linq;
using Fluxor;
using HealthGateway.Admin.Common.Models;
using HealthGateway.Common.Data.ViewModels;

/// <summary>
/// The effects for the feature.
/// </summary>
public static class UserFeedbackReducers
{
    /// <summary>
    /// The reducer for associating tag to user feedback.
    /// </summary>
    /// <param name="state">The Tag state.</param>
    /// <returns>The new state.</returns>
    [ReducerMethod(typeof(UserFeedbackActions.AssociateTagAction))]
    public static UserFeedbackState ReduceAssociateTagAction(UserFeedbackState state)
    {
        return state with
        {
            AssociateTag = state.AssociateTag with
            {
                IsLoading = true,
            },
        };
    }

    /// <summary>
    /// The reducer for associating tag to user feedback success action.
    /// </summary>
    /// <param name="state">The user feedback state.</param>
    /// <param name="action">The load success action.</param>
    /// <returns>The new state.</returns>
    [ReducerMethod]
    public static UserFeedbackState ReduceAssociateTagSuccessAction(UserFeedbackState state, UserFeedbackActions.AssociateTagSuccessAction action)
    {
        IImmutableDictionary<Guid, UserFeedbackView> data = state.FeedbackData ?? new Dictionary<Guid, UserFeedbackView>().ToImmutableDictionary();

        UserFeedbackTagView? tag = action.Data.ResourcePayload;
        if (tag != null && data.TryGetValue(tag.Id, out UserFeedbackView? feedback))
        {
            feedback?.Tags.Add(tag);
        }

        return state with
        {
            AssociateTag = state.AssociateTag with
            {
                IsLoading = true,
                Error = null,
                Result = action.Data,
            },
            FeedbackData = data,
        };
    }

    /// <summary>
    /// The reducer for the associating tag to user fail action.
    /// </summary>
    /// <param name="state">The user feedback state.</param>
    /// <param name="action">The add fail action.</param>
    /// <returns>The new state.</returns>
    [ReducerMethod]
    public static UserFeedbackState ReduceAssociateTagFailAction(UserFeedbackState state, UserFeedbackActions.AssociateTagFailAction action)
    {
        return state with
        {
            AssociateTag = state.AssociateTag with
            {
                IsLoading = false,
                Error = action.Error,
            },
        };
    }

    /// <summary>
    /// The reducer for dissociating tag from user feedback.
    /// </summary>
    /// <param name="state">The user feedback state.</param>
    /// <returns>The new state.</returns>
    [ReducerMethod(typeof(UserFeedbackActions.DissociateTagAction))]
    public static UserFeedbackState ReduceDissociateTagAction(UserFeedbackState state)
    {
        return state with
        {
            DissociateTag = state.DissociateTag with
            {
                IsLoading = true,
            },
        };
    }

    /// <summary>
    /// The reducer for disassociating tag from user feedback success action.
    /// </summary>
    /// <param name="state">The user feedback state.</param>
    /// <param name="action">The load success action.</param>
    /// <returns>The new state.</returns>
    [ReducerMethod]
    public static UserFeedbackState ReduceDissociateTagSuccessAction(UserFeedbackState state, UserFeedbackActions.DissociateTagSuccessAction action)
    {
        IImmutableDictionary<Guid, UserFeedbackView> data = state.FeedbackData ?? new Dictionary<Guid, UserFeedbackView>().ToImmutableDictionary();

        if (action.Result.ResourcePayload && data.TryGetValue(action.FeedbackId, out UserFeedbackView? feedback))
        {
            feedback?.Tags.Remove(action.FeedbackTag);
        }

        return state with
        {
            DissociateTag = state.DissociateTag with
            {
                IsLoading = true,
                Error = null,
                Result = action.Result,
            },
            FeedbackData = data,
        };
    }

    /// <summary>
    /// The reducer for loading User Feedback.
    /// </summary>
    /// <param name="state">The user feedback state.</param>
    /// <returns>The new state.</returns>
    [ReducerMethod(typeof(UserFeedbackActions.LoadAction))]
    public static UserFeedbackState ReduceLoadAction(UserFeedbackState state)
    {
        return state with
        {
            Load = state.Load with
            {
                IsLoading = true,
            },
        };
    }

    /// <summary>
    /// The reducer for the load success action.
    /// </summary>
    /// <param name="state">The user feedback state.</param>
    /// <param name="action">The load success action.</param>
    /// <returns>The new state.</returns>
    [ReducerMethod]
    public static UserFeedbackState ReduceLoadSuccessAction(UserFeedbackState state, UserFeedbackActions.LoadSuccessAction action)
    {
        return state with
        {
            Load = state.Load with
            {
                IsLoading = false,
                Result = action.Data,
                Error = null,
            },
            FeedbackData = action.Data.ResourcePayload.ToImmutableDictionary(tag => tag.Id),
        };
    }

    /// <summary>
    /// The reducer for the load fail action.
    /// </summary>
    /// <param name="state">The user feedback state.</param>
    /// <param name="action">The load fail action.</param>
    /// <returns>The new state.</returns>
    [ReducerMethod]
    public static UserFeedbackState ReduceLoadFailAction(UserFeedbackState state, UserFeedbackActions.LoadFailAction action)
    {
        return state with
        {
            Load = state.Load with
            {
                IsLoading = false,
                Error = action.Error,
            },
        };
    }

    /// <summary>
    /// The reducer for the reset state action.
    /// </summary>
    /// <param name="state">The user feedback state.</param>
    /// <returns>The default state.</returns>
    [ReducerMethod(typeof(UserFeedbackActions.ResetStateAction))]
    public static UserFeedbackState ReduceResetStateAction(UserFeedbackState state)
    {
        return state with
        {
            AssociateTag = new(),
            Load = new(),
        };
    }
}
