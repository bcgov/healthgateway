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
namespace HealthGateway.Admin.Client.Components;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading.Tasks;
using Fluxor;
using Fluxor.Blazor.Web.Components;
using HealthGateway.Admin.Client.Store.AgentAccess;
using HealthGateway.Admin.Common.Constants;
using HealthGateway.Admin.Common.Models;
using Microsoft.AspNetCore.Components;
using MudBlazor;

/// <summary>
/// Backing logic for the AgentAccessDialog component.
/// If the Save button is pressed, the dialog's Result will have the Data property populated with a bool value of true.
/// If the Cancel button is pressed, the dialog's Result will have the Cancelled property set to true.
/// </summary>
public partial class AgentAccessDialog : FluxorComponent
{
    /// <summary>
    /// Gets or sets the agent model corresponding to the agent that is being edited.
    /// </summary>
    [Parameter]
    public AdminAgent Agent { get; set; } = default!;

    private static List<KeycloakIdentityProvider> IdentityProviders => new()
    {
        KeycloakIdentityProvider.Idir,
        KeycloakIdentityProvider.PhsaAzure,
    };

    private static List<IdentityAccessRole> AccessRoles => new()
    {
        IdentityAccessRole.AdminUser,
        IdentityAccessRole.AdminReviewer,
        IdentityAccessRole.SupportUser,
    };

    [CascadingParameter]
    private MudDialogInstance MudDialog { get; set; } = default!;

    [Inject]
    private IDispatcher Dispatcher { get; set; } = default!;

    [Inject]
    private IActionSubscriber ActionSubscriber { get; set; } = default!;

    [Inject]
    private IState<AgentAccessState> AgentAccessState { get; set; } = default!;

    private bool IsEdit => this.Agent.Id != Guid.Empty;

    private bool HasAddError => this.AgentAccessState.Value.Add.Error is { Message.Length: > 0 };

    private bool HasUpdateError => this.AgentAccessState.Value.Update.Error is { Message.Length: > 0 };

    private string? ErrorMessage => this.HasAddError ? this.AgentAccessState.Value.Add.Error?.Message : this.AgentAccessState.Value.Update.Error?.Message;

    private bool SaveButtonDisabled => this.IsEdit ? this.Agent.Roles.SetEquals(this.InitialRoles) : this.Agent.IdentityProvider == KeycloakIdentityProvider.Unknown;

    private MudForm Form { get; set; } = default!;

    private IReadOnlySet<IdentityAccessRole> InitialRoles { get; set; } = ImmutableHashSet<IdentityAccessRole>.Empty;

    /// <inheritdoc/>
    protected override void OnInitialized()
    {
        base.OnInitialized();
        this.ActionSubscriber.SubscribeToAction<AgentAccessActions.AddSuccessAction>(this, _ => this.MudDialog.Close(true));
        this.ActionSubscriber.SubscribeToAction<AgentAccessActions.UpdateSuccessAction>(this, _ => this.MudDialog.Close(true));
        this.InitialRoles = new HashSet<IdentityAccessRole>(this.Agent.Roles);
    }

    private void HandleClickCancel()
    {
        this.MudDialog.Cancel();
    }

    private async Task HandleClickSaveAsync()
    {
        await this.Form.Validate().ConfigureAwait(true);
        if (this.Form.IsValid)
        {
            if (this.IsEdit)
            {
                this.Dispatcher.Dispatch(new AgentAccessActions.UpdateAction(this.Agent));
            }
            else
            {
                this.Dispatcher.Dispatch(new AgentAccessActions.AddAction(this.Agent));
            }
        }
    }

    private void UpdateRoles(IEnumerable<IdentityAccessRole> roles)
    {
        this.Agent.Roles.Clear();
        this.Agent.Roles.UnionWith(roles);
    }
}
