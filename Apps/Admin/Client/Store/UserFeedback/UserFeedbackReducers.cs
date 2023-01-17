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

#pragma warning disable CS1591, SA1600
public static class UserFeedbackReducers
{
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

    [ReducerMethod(typeof(UserFeedbackActions.UpdateAction))]
    public static UserFeedbackState ReduceUpdateAction(UserFeedbackState state)
    {
        return state with
        {
            Update = state.Update with
            {
                IsLoading = true,
            },
        };
    }

    [ReducerMethod]
    public static UserFeedbackState ReduceUpdateSuccessAction(UserFeedbackState state, UserFeedbackActions.UpdateSuccessAction action)
    {
        IImmutableDictionary<Guid, UserFeedbackView> data = state.FeedbackData ?? new Dictionary<Guid, UserFeedbackView>().ToImmutableDictionary();

        UserFeedbackView? feedback = action.Data.ResourcePayload;
        if (feedback != null)
        {
            data = data.Remove(feedback.Id).Add(feedback.Id, feedback);
        }

        return state with
        {
            Update = state.Update with
            {
                IsLoading = false,
                Result = action.Data,
                Error = null,
            },
            FeedbackData = data,
        };
    }

    [ReducerMethod]
    public static UserFeedbackState ReduceUpdateFailAction(UserFeedbackState state, UserFeedbackActions.UpdateFailAction action)
    {
        return state with
        {
            Update = state.Update with
            {
                IsLoading = false,
                Error = action.Error,
            },
        };
    }

    [ReducerMethod(typeof(UserFeedbackActions.AssociateTagsAction))]
    public static UserFeedbackState ReduceAssociateTagsAction(UserFeedbackState state)
    {
        return state with
        {
            AssociateTags = state.AssociateTags with
            {
                IsLoading = true,
            },
        };
    }

    [ReducerMethod]
    public static UserFeedbackState ReduceAssociateTagsSuccessAction(UserFeedbackState state, UserFeedbackActions.AssociateTagsSuccessAction action)
    {
        IImmutableDictionary<Guid, UserFeedbackView> data = state.FeedbackData ?? new Dictionary<Guid, UserFeedbackView>().ToImmutableDictionary();

        UserFeedbackView? feedback = action.Data.ResourcePayload;
        if (feedback != null)
        {
            data = data.Remove(feedback.Id).Add(feedback.Id, feedback);
        }

        return state with
        {
            AssociateTags = state.AssociateTags with
            {
                IsLoading = false,
                Error = null,
                Result = action.Data,
            },
            FeedbackData = data,
        };
    }

    [ReducerMethod]
    public static UserFeedbackState ReduceAssociateTagsFailAction(UserFeedbackState state, UserFeedbackActions.AssociateTagsFailAction action)
    {
        return state with
        {
            AssociateTags = state.AssociateTags with
            {
                IsLoading = false,
                Error = action.Error,
            },
        };
    }

    [ReducerMethod(typeof(UserFeedbackActions.ResetStateAction))]
    public static UserFeedbackState ReduceResetStateAction(UserFeedbackState state)
    {
        return state with
        {
            AssociateTags = new(),
            Load = new(),
        };
    }
}
