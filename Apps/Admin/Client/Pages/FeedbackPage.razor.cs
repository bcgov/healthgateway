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

namespace HealthGateway.Admin.Client.Pages;

using System;
using System.Collections.Generic;
using System.Linq;
using Fluxor;
using Fluxor.Blazor.Web.Components;
using HealthGateway.Admin.Client.Store;
using HealthGateway.Admin.Client.Store.Tag;
using HealthGateway.Admin.Client.Store.UserFeedback;
using HealthGateway.Admin.Common.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using MudBlazor;

/// <summary>
/// Backing logic for the communications page.
/// </summary>
public partial class FeedbackPage : FluxorComponent
{
    [Inject]
    private IDispatcher Dispatcher { get; set; } = default!;

    [Inject]
    private IActionSubscriber ActionSubscriber { get; set; } = default!;

    [Inject]
    private IState<UserFeedbackState> UserFeedbackState { get; set; } = default!;

    [Inject]
    private IState<TagState> TagState { get; set; } = default!;

    [Inject]
    private ISnackbar Snackbar { get; set; } = default!;

    private string ActiveTag { get; set; } = string.Empty;

    private bool FeedbackLoading => this.UserFeedbackState.Value.Load.IsLoading;

    private bool FeedbackLoaded => this.UserFeedbackState.Value.Load.Loaded;

    private bool TagsLoading => this.TagState.Value.Load.IsLoading;

    private bool ActiveTagExists => this.Tags.Any(t => t.Name == this.ActiveTag.Trim());

    private IEnumerable<RequestError> TagErrors => new[]
    {
        this.TagState.Value.Load.Error,
        this.TagState.Value.Add.Error,
        this.TagState.Value.Delete.Error,
    }.OfType<RequestError>().Where(e => e.Message.Length > 0);

    private IEnumerable<AdminTagView> Tags => this.TagState.Value.Data?.Values.OrderBy(t => t.Name) ?? Enumerable.Empty<AdminTagView>();

    private IEnumerable<UserFeedbackView> Feedback => this.UserFeedbackState.Value.FeedbackData?.Values ?? Enumerable.Empty<UserFeedbackView>();

    private IEnumerable<FeedbackRow> FeedbackRows => this.Feedback.Select(f => new FeedbackRow(f));

    private MudChip[] SelectedTagChips { get; set; } = Array.Empty<MudChip>();

    private bool ActiveTagIsValid => this.ActiveTag.Trim().Length > 0;

    private bool FeedbackUpdating => this.UserFeedbackState.Value.Update.IsLoading ||
                                     this.UserFeedbackState.Value.AssociateTags.IsLoading;

    /// <inheritdoc />
    protected override void OnInitialized()
    {
        base.OnInitialized();
        this.ResetState();
        this.Dispatcher.Dispatch(new TagActions.LoadAction());
        this.Dispatcher.Dispatch(new UserFeedbackActions.LoadAction());
        this.ActionSubscriber.SubscribeToAction<TagActions.AddSuccessAction>(this, this.DisplayAddSuccessful);
        this.ActionSubscriber.SubscribeToAction<TagActions.DeleteSuccessAction>(this, this.DisplayDeleteSuccessful);
        this.ActionSubscriber.SubscribeToAction<UserFeedbackActions.UpdateSuccessAction>(this, this.DisplayUpdateSuccessful);
        this.ActionSubscriber.SubscribeToAction<UserFeedbackActions.AssociateTagsSuccessAction>(this, this.DisplayAssociateSuccessful);
    }

    private void ResetState()
    {
        this.Dispatcher.Dispatch(new TagActions.ResetStateAction());
    }

    private void DisplayAddSuccessful(TagActions.AddSuccessAction action)
    {
        this.Snackbar.Add($"Tag \"{action.Data.ResourcePayload?.Name}\" added.", Severity.Success);
        this.ActiveTag = string.Empty;
        this.StateHasChanged();
    }

    private void DisplayDeleteSuccessful(TagActions.DeleteSuccessAction action)
    {
        this.Snackbar.Add($"Tag \"{action.Data.ResourcePayload?.Name}\" deleted.", Severity.Success);
        this.ActiveTag = string.Empty;
        this.StateHasChanged();
    }

    private void DisplayUpdateSuccessful(UserFeedbackActions.UpdateSuccessAction action)
    {
        this.Snackbar.Add("Feedback updated.", Severity.Success);
    }

    private void DisplayAssociateSuccessful(UserFeedbackActions.AssociateTagsSuccessAction action)
    {
        this.Snackbar.Add("Feedback tags updated.", Severity.Success);
    }

    private void HandleKeyDownActiveTag(KeyboardEventArgs eventArgs)
    {
        if (eventArgs.Key == "Enter" && this.ActiveTagIsValid)
        {
            this.AddTag();
        }
    }

    private void AddTag()
    {
        if (this.ActiveTagExists)
        {
            this.Snackbar.Add($"Tag \"{this.ActiveTag}\" already exists.", Severity.Warning);
            this.ActiveTag = string.Empty;
            this.StateHasChanged();
            return;
        }

        if (this.TagState.Value.Add.IsLoading)
        {
            return;
        }

        this.Dispatcher.Dispatch(new TagActions.AddAction(this.ActiveTag.Trim()));
    }

    private void RemoveTag(MudChip chip)
    {
        if (this.TagState.Value.Delete.IsLoading || this.UserFeedbackState.Value.Load.IsLoading || this.UserFeedbackState.Value.AssociateTags.IsLoading)
        {
            return;
        }

        if (this.Feedback.Any(f => f.Tags.Any(t => t.Tag.Name == chip.Text)))
        {
            this.Snackbar.Add($"Tag \"{chip.Text}\" cannot be removed because it is currently associated with feedback.", Severity.Warning);
            return;
        }

        IEnumerable<AdminTagView> tags = this.Tags.Where(t => t.Name == chip.Text);
        foreach (AdminTagView tag in tags)
        {
            this.Dispatcher.Dispatch(new TagActions.DeleteAction(tag));
        }
    }

    private string DescribeTags(List<string> tagIds)
    {
        IEnumerable<AdminTagView> tags = this.Tags.Where(t => tagIds.Contains(t.Id.ToString()));
        return string.Join(", ", tags.Select(t => t.Name).OrderBy(t => t));
    }

    private void ToggleIsReviewed(Guid feedbackId)
    {
        UserFeedbackView? feedback = this.Feedback.FirstOrDefault(f => f.Id == feedbackId);
        if (feedback != null)
        {
            feedback.IsReviewed = !feedback.IsReviewed;
            this.Dispatcher.Dispatch(new UserFeedbackActions.UpdateAction(feedback));
        }
    }

    private sealed class FeedbackRow
    {
        public FeedbackRow(UserFeedbackView model)
        {
            this.Id = model.Id;
            this.DateTime = model.CreatedDateTime;
            this.Hdid = model.UserProfileId ?? string.Empty;
            this.Email = model.Email;
            this.Comments = model.Comment ?? string.Empty;
            this.TagIds = model.Tags.Select(t => t.Tag.Id).ToHashSet();
            this.IsReviewed = model.IsReviewed;
        }

        public Guid Id { get; }

        public DateTime DateTime { get; }

        public string Hdid { get; }

        public string Email { get; }

        public string Comments { get; }

        public IEnumerable<Guid> TagIds { get; set; }

        public bool IsReviewed { get; }
    }
}
