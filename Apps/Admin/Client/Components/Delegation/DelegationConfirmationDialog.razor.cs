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
namespace HealthGateway.Admin.Client.Components.Delegation;

using Fluxor;
using Fluxor.Blazor.Web.Components;
using HealthGateway.Admin.Client.Store.Delegation;
using Microsoft.AspNetCore.Components;
using MudBlazor;

/// <summary>
/// Backing logic for the DelegationConfirmationDialog component.
/// If the Save button is pressed, the dialog's Result will have the Data property populated with a bool value of true.
/// If the Cancel button is pressed, the dialog's Result will have the Cancelled property set to true.
/// </summary>
public partial class DelegationConfirmationDialog : FluxorComponent
{
    /// <summary>
    /// Represents the types of delegation confirmations.
    /// </summary>
    public enum ConfirmationType
    {
        /// <summary>
        /// Confirmation for protecting a dependent.
        /// </summary>
        Protect,

        /// <summary>
        /// Confirmation for unprotecting a dependent.
        /// </summary>
        Unprotect,
    }

    /// <summary>
    /// Gets or sets the type of delegation confirmation.
    /// </summary>
    [Parameter]
    [EditorRequired]
    public ConfirmationType Type { get; set; }

    [CascadingParameter]
    private MudDialogInstance MudDialog { get; set; } = default!;

    [Inject]
    private IDispatcher Dispatcher { get; set; } = default!;

    [Inject]
    private IActionSubscriber ActionSubscriber { get; set; } = default!;

    [Inject]
    private IState<DelegationState> DelegationState { get; set; } = default!;

    private bool HasProtectError => this.DelegationState.Value.Protect.Error is { Message.Length: > 0 };

    private string? ProtectErrorMessage => this.DelegationState.Value.Protect.Error?.Message;

    private bool HasUnprotectError => this.DelegationState.Value.Unprotect.Error is { Message.Length: > 0 };

    private string? UnprotectErrorMessage => this.DelegationState.Value.Unprotect.Error?.Message;

    private bool Loading => this.Type switch
    {
        ConfirmationType.Protect => this.DelegationState.Value.Protect.IsLoading,
        ConfirmationType.Unprotect => this.DelegationState.Value.Unprotect.IsLoading,
        _ => false,
    };

    /// <inheritdoc/>
    protected override void OnInitialized()
    {
        base.OnInitialized();
        this.ActionSubscriber.SubscribeToAction<DelegationActions.ProtectDependentSuccessAction>(this, _ => this.MudDialog.Close(true));
        this.ActionSubscriber.SubscribeToAction<DelegationActions.UnprotectDependentSuccessAction>(this, _ => this.MudDialog.Close(true));
    }

    private void HandleClickCancel()
    {
        this.Dispatcher.Dispatch(new DelegationActions.ClearProtectErrorAction());
        this.Dispatcher.Dispatch(new DelegationActions.ClearUnprotectErrorAction());
        this.MudDialog.Cancel();
    }

    private void HandleClickConfirm()
    {
        switch (this.Type)
        {
            case ConfirmationType.Protect:
                this.Dispatcher.Dispatch(new DelegationActions.ProtectDependentAction());
                break;
            case ConfirmationType.Unprotect:
                this.Dispatcher.Dispatch(new DelegationActions.UnprotectDependentAction());
                break;
        }
    }
}
