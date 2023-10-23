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
using HealthGateway.Admin.Client.Models;
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
            FeedbackData = action.Data.ToImmutableDictionary(tag => tag.Id),
        };
    }

    [ReducerMethod]
    public static UserFeedbackState ReduceLoadFailureAction(UserFeedbackState state, UserFeedbackActions.LoadFailureAction action)
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
        IImmutableDictionary<Guid, ExtendedUserFeedbackView> data = state.FeedbackData ?? new Dictionary<Guid, ExtendedUserFeedbackView>().ToImmutableDictionary();

        ExtendedUserFeedbackView feedback = action.Data;
        data = data.Remove(feedback.Id).Add(feedback.Id, feedback);

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
    public static UserFeedbackState ReduceUpdateFailureAction(UserFeedbackState state, UserFeedbackActions.UpdateFailureAction action)
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

    [ReducerMethod]
    public static UserFeedbackState ReduceChangeAssociatedTagsAction(UserFeedbackState state, UserFeedbackActions.ChangeAssociatedTagsAction action)
    {
        IImmutableDictionary<Guid, ExtendedUserFeedbackView> data = state.FeedbackData ?? new Dictionary<Guid, ExtendedUserFeedbackView>().ToImmutableDictionary();

        if (state.FeedbackData != null && state.FeedbackData.TryGetValue(action.FeedbackId, out ExtendedUserFeedbackView? feedback))
        {
            feedback = feedback.ShallowCopy();
            feedback.IsDirty = true;
            feedback.Tags = action.TagIds.Select(
                    u => new UserFeedbackTagView
                    {
                        FeedbackId = action.FeedbackId,
                        TagId = u,
                    })
                .ToList();
            data = data.Remove(feedback.Id).Add(feedback.Id, feedback);
        }

        return state with
        {
            FeedbackData = data,
        };
    }

    [ReducerMethod(typeof(UserFeedbackActions.SaveAssociatedTagsAction))]
    public static UserFeedbackState ReduceSaveAssociatedTagsAction(UserFeedbackState state)
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
    public static UserFeedbackState ReduceSaveAssociatedTagsSuccessAction(UserFeedbackState state, UserFeedbackActions.SaveAssociatedTagsSuccessAction action)
    {
        IImmutableDictionary<Guid, ExtendedUserFeedbackView> data = state.FeedbackData ?? new Dictionary<Guid, ExtendedUserFeedbackView>().ToImmutableDictionary();

        ExtendedUserFeedbackView feedback = action.Data;
        data = data.Remove(feedback.Id).Add(feedback.Id, feedback);

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
    public static UserFeedbackState ReduceSaveAssociatedTagsFailureAction(UserFeedbackState state, UserFeedbackActions.SaveAssociatedTagsFailureAction action)
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
