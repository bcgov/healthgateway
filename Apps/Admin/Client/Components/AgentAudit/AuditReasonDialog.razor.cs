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
namespace HealthGateway.Admin.Client.Components.AgentAudit;

using Fluxor;
using Fluxor.Blazor.Web.Components;
using HealthGateway.Admin.Client.Store;
using Microsoft.AspNetCore.Components;
using MudBlazor;

/// <summary>
/// Backing logic for the AuditReasonDialog component.
/// If the Confirm button is pressed, the dialog's Result will have the Data property populated with a string representing
/// the audit reason.
/// If the Cancel button is pressed, the dialog's Result will have the Cancelled property set to true.
/// </summary>
/// <typeparam name="TAction">An action to dispatch when the confirmation button is pressed.</typeparam>
/// <typeparam name="TErrorAction">An action that indicates <typeparamref name="TAction"/> encountered an error.</typeparam>
/// <typeparam name="TSuccessAction">An action that indicates <typeparamref name="TAction"/> completed successfully.</typeparam>
public partial class AuditReasonDialog<TAction, TErrorAction, TSuccessAction> : FluxorComponent
    where TErrorAction : BaseFailAction
    where TAction : BaseAgentAuditAction
{
    /// <summary>
    /// Gets or sets the action to dispatch when an audit reason has been confirmed.
    /// </summary>
    [Parameter]
    [EditorRequired]
    public TAction AuditableAction { get; set; } = default!;

    /// <summary>
    /// Gets or sets the action to dispatch when the dialog is cancelled.
    /// </summary>
    [Parameter]
    public object? CancelAction { get; set; }

    [CascadingParameter]
    private MudDialogInstance MudDialog { get; set; } = default!;

    [Inject]
    private IActionSubscriber ActionSubscriber { get; set; } = default!;

    [Inject]
    private IDispatcher Dispatcher { get; set; } = default!;

    private string AuditReason { get; set; } = string.Empty;

    private bool IsLoading { get; set; }

    private RequestError? Error { get; set; }

    private bool SaveButtonDisabled => this.AuditReason.Length < 2;

    private string? ErrorMessage => this.Error?.Message;

    private bool HasError => this.Error is not null;

    /// <summary>
    /// Called when the component is initialized.
    /// Configure Actions and Subscriptions.
    /// </summary>
    protected override void OnInitialized()
    {
        base.OnInitialized();
        this.ActionSubscriber.SubscribeToAction<TErrorAction>(this, this.HandleActionFailed);
        this.ActionSubscriber.SubscribeToAction<TSuccessAction>(this, this.HandleActionSuccess);
    }

    /// <inheritdoc/>
    protected override void Dispose(bool disposing)
    {
        this.ActionSubscriber.UnsubscribeFromAllActions(this);
        base.Dispose(disposing);
    }

    private void HandleActionSuccess(TSuccessAction successAction)
    {
        this.IsLoading = false;
        this.MudDialog.Close(DialogResult.Ok(this.AuditReason));
    }

    private void HandleActionFailed(BaseFailAction failAction)
    {
        this.IsLoading = false;
        this.Error = failAction.Error;
    }

    private void HandleClickCancel()
    {
        if (this.CancelAction is not null)
        {
            this.Dispatcher.Dispatch(this.CancelAction);
        }

        this.MudDialog.Cancel();
    }

    private void HandleClickConfirm()
    {
        this.IsLoading = true;
        this.AuditableAction.Reason = this.AuditReason;
        this.Dispatcher.Dispatch(this.AuditableAction);
    }
}
