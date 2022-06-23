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
using System.Threading.Tasks;
using Fluxor;
using Fluxor.Blazor.Web.Components;
using HealthGateway.Admin.Client.Store;
using HealthGateway.Admin.Client.Store.Tag;
using HealthGateway.Admin.Client.Store.UserFeedback;
using HealthGateway.Admin.Common.Constants;
using HealthGateway.Admin.Common.Models;
using Microsoft.AspNetCore.Components;
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

    [Inject]
    private NavigationManager NavigationManager { get; set; } = default!;

    private bool FeedbackLoading => this.UserFeedbackState.Value.Load.IsLoading;

    private bool FeedbackLoaded => this.UserFeedbackState.Value.Load.Loaded;

    private AddTagFormModel AddTagModel { get; } = new();

    private IEnumerable<RequestError> TagErrors => new[]
    {
        this.TagState.Value.Load.Error,
        this.TagState.Value.Add.Error,
        this.TagState.Value.Delete.Error,
    }.OfType<RequestError>().Where(e => e.Message.Length > 0);

    private IEnumerable<AdminTagView> Tags => this.TagState.Value.Data?.Values.OrderBy(t => t.Name) ?? Enumerable.Empty<AdminTagView>();

    private IEnumerable<UserFeedbackView> Feedback => this.UserFeedbackState.Value.FeedbackData?.Values ?? Enumerable.Empty<UserFeedbackView>();

    private IEnumerable<FeedbackRow> FeedbackRows => this.Feedback
        .Where(f => this.TagIdFilter.All(t => f.Tags.Any(ft => ft.TagId == t)))
        .OrderByDescending(f => f.CreatedDateTime)
        .Select(f => new FeedbackRow(f));

    private MudChip[] SelectedTagChips { get; set; } = Array.Empty<MudChip>();

    private IEnumerable<Guid> TagIdFilter => this.SelectedTagChips.Select(c => c.Value).OfType<Guid>();

    private bool FeedbackUpdating => this.UserFeedbackState.Value.Update.IsLoading ||
                                     this.UserFeedbackState.Value.AssociateTags.IsLoading;

    /// <inheritdoc />
    protected override void OnInitialized()
    {
        base.OnInitialized();
        this.ResetState();
        this.ActionSubscriber.SubscribeToAction<UserFeedbackActions.UpdateSuccessAction>(this, this.HandleFeedbackUpdateSuccessful);
        this.ActionSubscriber.SubscribeToAction<UserFeedbackActions.AssociateTagsSuccessAction>(this, this.HandleFeedbackAssociateSuccessful);
        this.ActionSubscriber.SubscribeToAction<TagActions.AddSuccessAction>(this, this.HandleTagAddSuccessful);
        this.ActionSubscriber.SubscribeToAction<TagActions.DeleteSuccessAction>(this, this.HandleTagDeleteSuccessful);
        this.Dispatcher.Dispatch(new TagActions.LoadAction());
        this.Dispatcher.Dispatch(new UserFeedbackActions.LoadAction());
    }

    private void ResetState()
    {
        this.Dispatcher.Dispatch(new TagActions.ResetStateAction());
    }

    private void HandleTagAddSuccessful(TagActions.AddSuccessAction action)
    {
        this.Snackbar.Add($"Tag \"{action.Data.ResourcePayload?.Name}\" added.", Severity.Success);
        this.AddTagModel.Clear();
        this.StateHasChanged();
    }

    private void HandleTagDeleteSuccessful(TagActions.DeleteSuccessAction action)
    {
        this.Snackbar.Add($"Tag \"{action.Data.ResourcePayload?.Name}\" deleted.", Severity.Success);
        this.AddTagModel.Clear();
        this.StateHasChanged();
    }

    private void HandleFeedbackUpdateSuccessful(UserFeedbackActions.UpdateSuccessAction action)
    {
        this.Snackbar.Add("Feedback updated.", Severity.Success);
    }

    private void HandleFeedbackAssociateSuccessful(UserFeedbackActions.AssociateTagsSuccessAction action)
    {
        this.Snackbar.Add("Feedback tags updated.", Severity.Success);
    }

    private void AddTag()
    {
        if (this.TagState.Value.Add.IsLoading)
        {
            return;
        }

        string tagName = this.AddTagModel.Name.Trim();
        if (tagName.Length == 0)
        {
            this.Snackbar.Add($"Tag is invalid.", Severity.Warning);
            this.AddTagModel.Clear();
            this.StateHasChanged();
            return;
        }

        if (this.Tags.Any(t => t.Name == tagName))
        {
            this.Snackbar.Add($"Tag \"{tagName}\" already exists.", Severity.Warning);
            this.AddTagModel.Clear();
            this.StateHasChanged();
            return;
        }

        this.Dispatcher.Dispatch(new TagActions.AddAction(tagName));
    }

    private void RemoveTag(MudChip chip)
    {
        if (this.TagState.Value.Delete.IsLoading || this.UserFeedbackState.Value.Load.IsLoading || this.UserFeedbackState.Value.AssociateTags.IsLoading)
        {
            return;
        }

        Guid tagId = (Guid)chip.Value;

        if (this.Feedback.Any(f => f.Tags.Any(t => t.TagId == tagId)))
        {
            this.Snackbar.Add($"Tag \"{chip.Text}\" cannot be removed because it is currently associated with feedback.", Severity.Warning);
            return;
        }

        AdminTagView? tag = this.Tags.SingleOrDefault(t => t.Id == tagId);
        if (tag != null)
        {
            this.Dispatcher.Dispatch(new TagActions.DeleteAction(tag));
        }
    }

    private void NavigateToSupport(string hdid)
    {
        this.NavigationManager.NavigateTo($"support?{UserQueryType.HDID}={hdid}");
    }

    private string DescribeTags(List<string> tagIds)
    {
        IEnumerable<AdminTagView> tags = this.Tags.Where(t => tagIds.Contains(t.Id.ToString()));
        return string.Join(", ", tags.Select(t => t.Name).OrderBy(t => t));
    }

    private void AssociateTags(IEnumerable<Guid> tagIds, Guid feedbackId)
    {
        this.Dispatcher.Dispatch(new UserFeedbackActions.AssociateTagsAction(tagIds, feedbackId));
    }

    private void ToggleIsReviewed(Guid feedbackId)
    {
        UserFeedbackView? currentFeedback = this.Feedback.FirstOrDefault(f => f.Id == feedbackId);
        if (currentFeedback != null)
        {
            UserFeedbackView updatedFeedback = currentFeedback.ShallowCopy();
            updatedFeedback.IsReviewed = !updatedFeedback.IsReviewed;
            this.Dispatcher.Dispatch(new UserFeedbackActions.UpdateAction(updatedFeedback));
        }
    }

    private sealed class AddTagFormModel
    {
        public string Name { get; set; } = string.Empty;

        public void Clear() => this.Name = string.Empty;
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
            this.TagIds = model.Tags.Select(t => t.TagId);
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
