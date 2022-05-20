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
    private IState<TagState> TagState { get; set; } = default!;

    [Inject]
    private ISnackbar Snackbar { get; set; } = default!;

    private string? ActiveTag { get; set; } = string.Empty;

    private bool TagsLoading => this.TagState.Value.Load.IsLoading;

    private bool ActiveTagExists => this.Tags.Any(t => t.Name == this.ActiveTag);

    private IEnumerable<RequestError> TagErrors => new[]
    {
        this.TagState.Value.Load.Error,
        this.TagState.Value.Add.Error,
        this.TagState.Value.Delete.Error,
    }.OfType<RequestError>().Where(e => e.Message.Length > 0);

    private IEnumerable<AdminTagView> Tags => this.TagState.Value.Data?.Values ?? Enumerable.Empty<AdminTagView>();

    /// <inheritdoc/>
    protected override void OnInitialized()
    {
        base.OnInitialized();
        this.ResetState();
        this.Dispatcher.Dispatch(new TagActions.LoadAction());
        this.ActionSubscriber.SubscribeToAction<TagActions.AddSuccessAction>(this, this.DisplayAddSuccessful);
        this.ActionSubscriber.SubscribeToAction<TagActions.DeleteSuccessAction>(this, this.DisplayDeleteSuccessful);
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

    private async Task<IEnumerable<string>> FilterTagsAsync(string? text)
    {
        IEnumerable<AdminTagView> tags = this.Tags;
        if (text != null)
        {
            tags = tags.Where(t => t.Name.StartsWith(text, StringComparison.InvariantCulture));
        }

        return tags.Select(t => t.Name);
    }

    private async Task AddTagAsync()
    {
        if (this.ActiveTagExists || this.TagState.Value.Add.IsLoading || this.ActiveTag == null)
        {
            return;
        }

        this.Dispatcher.Dispatch(new TagActions.AddAction(this.ActiveTag));
    }

    private async Task RemoveTagAsync()
    {
        if (this.TagState.Value.Delete.IsLoading)
        {
            return;
        }

        IEnumerable<AdminTagView> tags = this.Tags.Where(t => t.Name == this.ActiveTag);
        foreach (AdminTagView tag in tags)
        {
            this.Dispatcher.Dispatch(new TagActions.DeleteAction(tag));
        }
    }
}
