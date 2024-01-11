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
using HealthGateway.Admin.Client.Models;
using HealthGateway.Admin.Client.Store;
using HealthGateway.Admin.Client.Store.PatientSupport;
using HealthGateway.Admin.Client.Store.Tag;
using HealthGateway.Admin.Client.Store.UserFeedback;
using HealthGateway.Admin.Client.Utils;
using HealthGateway.Admin.Common.Models;
using HealthGateway.Common.Data.Constants;
using HealthGateway.Common.Data.Utils;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using MudBlazor;

/// <summary>
/// Backing logic for the communications page.
/// </summary>
public partial class FeedbackPage : FluxorComponent
{
    [Inject]
    private IConfiguration Configuration { get; set; } = default!;

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

    [Inject]
    private IJSRuntime JsRuntime { get; set; } = default!;

    [Inject]
    private ILogger<FeedbackPage> Logger { get; set; } = default!;

    private bool FeedbackLoading => this.UserFeedbackState.Value.Load.IsLoading;

    private bool FeedbackLoaded => this.UserFeedbackState.Value.Load.Loaded;

    private AddTagFormModel AddTagModel { get; } = new();

    private MudTextField<string> AddTagNameInput { get; set; } = default!;

    private IEnumerable<RequestError> TagErrors => new[]
        {
            this.TagState.Value.Load.Error,
            this.TagState.Value.Add.Error,
            this.TagState.Value.Delete.Error,
        }
        .OfType<RequestError>()
        .Where(e => e.Message.Length > 0);

    private IEnumerable<AdminTagView> Tags => (this.TagState.Value.Data?.Values ?? []).OrderBy(t => t.Name);

    private IEnumerable<ExtendedUserFeedbackView> Feedback => this.UserFeedbackState.Value.FeedbackData?.Values ?? [];

    private IEnumerable<ExtendedUserFeedbackView> FilteredFeedback => this.Feedback
        .Where(f => this.TagIdFilter.All(t => f.Tags.Any(ft => ft.TagId == t)));

    private IEnumerable<FeedbackRow> FeedbackRows => this.FilteredFeedback.Select(f => new FeedbackRow(f));

    private bool AnyUnsavedFeedbackChanges => this.FeedbackRows.Any(f => f.IsDirty);

    private MudChip[] SelectedTagChips { get; set; } = [];

    private IEnumerable<Guid> TagIdFilter => this.SelectedTagChips.Select(c => c.Value).OfType<Guid>();

    private bool TagsUpdating => this.TagState.Value.Load.IsLoading ||
                                 this.TagState.Value.Add.IsLoading ||
                                 this.TagState.Value.Delete.IsLoading;

    private bool FeedbackUpdating => this.UserFeedbackState.Value.Update.IsLoading ||
                                     this.UserFeedbackState.Value.AssociateTags.IsLoading;

    /// <inheritdoc/>
    protected override void OnInitialized()
    {
        base.OnInitialized();
        this.ResetState();
        this.ActionSubscriber.SubscribeToAction<UserFeedbackActions.UpdateSuccessAction>(this, this.HandleFeedbackUpdateSuccessful);
        this.ActionSubscriber.SubscribeToAction<UserFeedbackActions.SaveAssociatedTagsSuccessAction>(this, this.HandleFeedbackAssociateSuccessful);
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
        Task.Run(async () => await this.AddTagNameInput.FocusAsync());
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

    private void HandleFeedbackAssociateSuccessful(UserFeedbackActions.SaveAssociatedTagsSuccessAction action)
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
            this.Snackbar.Add("Tag is invalid.", Severity.Warning);
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

        this.Dispatcher.Dispatch(new TagActions.AddAction { TagName = tagName });
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
            this.Dispatcher.Dispatch(new TagActions.DeleteAction { AdminTagView = tag });
        }
    }

    private async Task NavigateToSupportAsync(string hdid)
    {
        await StoreUtility.LoadPatientSupportAction(this.Dispatcher, this.JsRuntime, PatientQueryType.Hdid, hdid);
        this.ActionSubscriber.SubscribeToAction<PatientSupportActions.LoadSuccessAction>(this, this.NavigateToPatientDetails);
    }

    private void NavigateToPatientDetails(PatientSupportActions.LoadSuccessAction action)
    {
        if (action.Data.Count == 1)
        {
            this.NavigationManager.NavigateTo($"/patient-details?phn={action.Data.Single().PersonalHealthNumber}");
        }
    }

    private string DescribeTags(List<string> tagIds)
    {
        IEnumerable<AdminTagView> tags = this.Tags.Where(t => tagIds.Contains(t.Id.ToString()));
        return string.Join(", ", tags.Select(t => t.Name).OrderBy(t => t));
    }

    private void ChangeAssociatedTags(IEnumerable<Guid> tagIds, Guid feedbackId)
    {
        this.Logger.LogInformation("Change associated tags started");
        this.Dispatcher.Dispatch(new UserFeedbackActions.ChangeAssociatedTagsAction { TagIds = tagIds, FeedbackId = feedbackId });
        this.Logger.LogInformation("Change associated tags finished");
    }

    private void SaveAssociatedTags(Guid feedbackId)
    {
        this.Logger.LogInformation("Save associated tags started");
        IEnumerable<Guid> tagIds = this.FeedbackRows.Single(r => r.Id == feedbackId).TagIds;
        this.Dispatcher.Dispatch(new UserFeedbackActions.SaveAssociatedTagsAction { TagIds = tagIds, FeedbackId = feedbackId });
        this.Logger.LogInformation("Save associated tags finished.");
    }

    private void ToggleIsReviewed(Guid feedbackId)
    {
        ExtendedUserFeedbackView? currentFeedback = this.Feedback.FirstOrDefault(f => f.Id == feedbackId);
        if (currentFeedback != null)
        {
            ExtendedUserFeedbackView updatedFeedback = currentFeedback.ShallowCopy();
            updatedFeedback.IsReviewed = !updatedFeedback.IsReviewed;
            this.Dispatcher.Dispatch(new UserFeedbackActions.UpdateAction { UserFeedbackView = updatedFeedback });
        }
    }

    private DateTime ConvertDateTime(DateTime utcDateTime)
    {
        return TimeZoneInfo.ConvertTimeFromUtc(utcDateTime, DateFormatter.GetLocalTimeZone(this.Configuration));
    }

    private sealed class AddTagFormModel
    {
        public string Name { get; set; } = string.Empty;

        public void Clear()
        {
            this.Name = string.Empty;
        }
    }

    private sealed class FeedbackRow
    {
        public FeedbackRow(ExtendedUserFeedbackView model)
        {
            this.Id = model.Id;
            this.DateTime = model.CreatedDateTime;
            this.Hdid = model.UserProfileId ?? string.Empty;
            this.Email = model.Email;
            this.Comments = model.Comment ?? string.Empty;
            this.TagIds = model.Tags.Select(t => t.TagId);
            this.IsReviewed = model.IsReviewed;
            this.IsDirty = model.IsDirty;
        }

        public Guid Id { get; }

        public DateTime DateTime { get; }

        public string Hdid { get; }

        public string Email { get; }

        public string Comments { get; }

        public IEnumerable<Guid> TagIds { get; }

        public bool IsReviewed { get; }

        public bool IsDirty { get; }
    }
}
