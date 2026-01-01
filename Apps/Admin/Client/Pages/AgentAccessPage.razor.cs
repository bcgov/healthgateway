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
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using Fluxor;
using Fluxor.Blazor.Web.Components;
using HealthGateway.Admin.Client.Components.AgentAccess;
using HealthGateway.Admin.Client.Store.AgentAccess;
using HealthGateway.Admin.Client.Utils;
using HealthGateway.Admin.Common.Models;
using HealthGateway.Common.Data.Utils;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using MudBlazor.Services;

/// <summary>
/// Backing logic for the agent provisioning page.
/// </summary>
public partial class AgentAccessPage : FluxorComponent
{
    private const int SearchResultLimit = 25;

    private static Func<string, string?> ValidateQueryParameter => parameter =>
    {
        string? lengthValidationResult = StringManipulator.StripWhitespace(parameter).Length < 3
            ? "Query must contain at least 3 characters"
            : null;

        return string.IsNullOrWhiteSpace(parameter)
            ? "Search parameter is required"
            : lengthValidationResult;
    };

    [Inject]
    private IDispatcher Dispatcher { get; set; } = default!;

    [Inject]
    private IActionSubscriber ActionSubscriber { get; set; } = default!;

    [Inject]
    private IState<AgentAccessState> AgentAccessState { get; set; } = default!;

    [Inject]
    private IDialogService Dialog { get; set; } = default!;

    [Inject]
    private ISnackbar Snackbar { get; set; } = default!;

    [Inject]
    private IKeyInterceptorService KeyInterceptorService { get; set; } = default!;

    private string Query { get; set; } = string.Empty;

    [SuppressMessage("Minor Code Smell", "S3459:Unassigned members should be removed", Justification = "Assigned in .razor file")]
    private MudForm Form { get; set; } = default!;

    [SuppressMessage("Minor Code Smell", "S3459:Unassigned members should be removed", Justification = "Assigned in .razor file")]
    private MudMessageBox DeleteConfirmation { get; set; } = default!;

    private bool Loading => this.AgentAccessState.Value.IsLoading;

    private bool Loaded => this.AgentAccessState.Value.Loaded;

    private bool HasSearchError => this.AgentAccessState.Value.Search.Error is { Message.Length: > 0 };

    private bool HasDeleteError => this.AgentAccessState.Value.Delete.Error is { Message.Length: > 0 };

    private string? ErrorMessage => this.HasSearchError ? this.AgentAccessState.Value.Search.Error?.Message : this.AgentAccessState.Value.Delete.Error?.Message;

    private bool IsModalShown { get; set; }

    private IEnumerable<AdminAgent> Agents => this.AgentAccessState.Value.Data?.Values ?? [];

    private IEnumerable<AdminAgentRow> Rows => this.Agents.Select(a => new AdminAgentRow(a));

    /// <inheritdoc/>
    protected override void OnInitialized()
    {
        base.OnInitialized();
        this.ResetState();
        this.ActionSubscriber.SubscribeToAction<AgentAccessActions.AddSuccessAction>(this, this.DisplayAddSuccessful);
        this.ActionSubscriber.SubscribeToAction<AgentAccessActions.UpdateSuccessAction>(this, this.DisplayUpdateSuccessful);
        this.ActionSubscriber.SubscribeToAction<AgentAccessActions.DeleteSuccessAction>(this, this.DisplayDeleteSuccessful);
    }

    /// <inheritdoc/>
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await this.KeyInterceptorService.SubscribeAsync(
                "query-controls",
                new("query-input", new KeyOptions("Enter", true)),
                _ => this.SearchAsync());
        }

        await base.OnAfterRenderAsync(firstRender);
    }

    /// <inheritdoc/>
    protected override async ValueTask DisposeAsyncCore(bool disposing)
    {
        if (disposing)
        {
            await this.KeyInterceptorService.UnsubscribeAsync("query-controls");
        }

        await base.DisposeAsyncCore(disposing);
    }

    private void ResetState()
    {
        this.Dispatcher.Dispatch(new AgentAccessActions.ResetStateAction());
    }

    private async Task SearchAsync()
    {
        await this.Form.Validate();
        if (this.Form.IsValid)
        {
            this.ResetState();
            this.Dispatcher.Dispatch(new AgentAccessActions.SearchAction { Query = StringManipulator.StripWhitespace(this.Query) });
        }
    }

    private async Task AddAsync()
    {
        const string title = "Provision Agent Access";

        AdminAgent agent = new();

        await this.OpenDialogAsync(title, agent);
    }

    private async Task EditAsync(Guid id)
    {
        const string title = "Update Agent Access";

        AdminAgent? agent = this.Agents.FirstOrDefault(c => c.Id == id);
        if (agent != null)
        {
            await this.OpenDialogAsync(title, agent);
        }
    }

    private async Task DeleteAsync(Guid id)
    {
        bool? delete = await this.DeleteConfirmation.ShowAsync();
        if (delete is true)
        {
            this.Dispatcher.Dispatch(new AgentAccessActions.DeleteAction { Id = id });
        }
    }

    private async Task OpenDialogAsync(string title, AdminAgent agent)
    {
        if (this.IsModalShown)
        {
            return;
        }

        this.IsModalShown = true;

        DialogParameters parameters = new() { ["Agent"] = agent };
        DialogOptions options = new() { BackdropClick = false };

        IDialogReference dialog = await this.Dialog.ShowAsync<AgentAccessDialog>(title, parameters, options);

        await dialog.Result;
        this.IsModalShown = false;
    }

    private void DisplayAddSuccessful(AgentAccessActions.AddSuccessAction action)
    {
        this.Snackbar.Add("Agent added successfully", Severity.Success);
    }

    private void DisplayUpdateSuccessful(AgentAccessActions.UpdateSuccessAction action)
    {
        this.Snackbar.Add("Agent access updated successfully", Severity.Success);
    }

    private void DisplayDeleteSuccessful(AgentAccessActions.DeleteSuccessAction action)
    {
        this.Snackbar.Add("Agent access removed successfully", Severity.Success);
    }

    private sealed record AdminAgentRow
    {
        public AdminAgentRow(AdminAgent model)
        {
            this.Id = model.Id;
            this.Username = model.Username;
            this.IdentityProvider = FormattingUtility.FormatKeycloakIdentityProvider(model.IdentityProvider);
            this.Roles = string.Join(", ", model.Roles.Select(r => r.ToString()).OrderBy(r => r));
        }

        public Guid Id { get; }

        public string Username { get; }

        public string IdentityProvider { get; }

        public string Roles { get; }
    }
}
