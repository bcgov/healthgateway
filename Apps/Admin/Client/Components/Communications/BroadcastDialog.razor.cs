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
namespace HealthGateway.Admin.Client.Components.Communications;

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Fluxor;
using Fluxor.Blazor.Web.Components;
using HealthGateway.Admin.Client.Components.Common;
using HealthGateway.Admin.Client.Services;
using HealthGateway.Admin.Client.Store.Broadcasts;
using HealthGateway.Common.Data.Models;
using Microsoft.AspNetCore.Components;
using MudBlazor;

/// <summary>
/// Backing logic for the BroadcastDialog component.
/// If the Save button is pressed, the dialog's Result will have the Data property populated with a bool value of true.
/// If the Cancel button is pressed, the dialog's Result will have the Canceled property set to true.
/// </summary>
public partial class BroadcastDialog : FluxorComponent
{
    /// <summary>
    /// Gets or sets the broadcast model corresponding to the broadcast that is being edited.
    /// </summary>
    [Parameter]
    public Broadcast Broadcast { get; set; } = default!;

    private static List<BroadcastActionType> ActionTypes =>
    [
        BroadcastActionType.None,
        BroadcastActionType.InternalLink,
        BroadcastActionType.ExternalLink,
    ];

    [SuppressMessage("Style", "IDE0072:Populate switch", Justification = "Team decision")]
    private Func<string, string?> ValidateActionUrl =>
        urlString =>
        {
            return this.Broadcast.ActionType switch
            {
                BroadcastActionType.InternalLink or BroadcastActionType.ExternalLink => urlString.StartsWith("https://", StringComparison.InvariantCultureIgnoreCase)
                                                                                        && Uri.TryCreate(this.ActionUrlString, UriKind.Absolute, out Uri? _)
                    ? null
                    : "URL is invalid",
                _ => urlString.Length == 0 ? null : "Selected Action Type does not support Action URL",
            };
        };

    [CascadingParameter]
    private IMudDialogInstance MudDialog { get; set; } = default!;

    [Inject]
    private IDispatcher Dispatcher { get; set; } = default!;

    [Inject]
    private IActionSubscriber ActionSubscriber { get; set; } = default!;

    [Inject]
    private IState<BroadcastsState> BroadcastsState { get; set; } = default!;

    [Inject]
    private IDateConversionService DateConversionService { get; set; } = default!;

    private bool IsNewBroadcast => this.Broadcast.Id == Guid.Empty;

    private bool HasAddError => this.BroadcastsState.Value.Add.Error is { Message.Length: > 0 };

    private bool HasUpdateError => this.BroadcastsState.Value.Update.Error is { Message.Length: > 0 };

    private string? ErrorMessage => this.HasAddError ? this.BroadcastsState.Value.Add.Error?.Message : this.BroadcastsState.Value.Update.Error?.Message;

    private MudForm Form { get; set; } = default!;

    private HgTextField<string> ActionUrlTextField { get; set; } = default!;

    private MudDatePicker EffectiveDatePicker { get; set; } = default!;

    private MudTimePicker EffectiveTimePicker { get; set; } = default!;

    private MudDatePicker ExpiryDatePicker { get; set; } = default!;

    private MudTimePicker ExpiryTimePicker { get; set; } = default!;

    private DateTime? EffectiveDate { get; set; }

    private TimeSpan? EffectiveTime { get; set; }

    private DateTime? ExpiryDate { get; set; }

    private TimeSpan? ExpiryTime { get; set; }

    private string ActionUrlString { get; set; } = string.Empty;

    /// <inheritdoc/>
    protected override void OnInitialized()
    {
        base.OnInitialized();
        this.ActionSubscriber.SubscribeToAction<BroadcastsActions.AddSuccessAction>(this, _ => this.MudDialog.Close(true));
        this.ActionSubscriber.SubscribeToAction<BroadcastsActions.UpdateSuccessAction>(this, _ => this.MudDialog.Close(true));
        this.SetFormValues();
    }

    private void SetFormValues()
    {
        if (this.IsNewBroadcast)
        {
            DateTime now = this.DateConversionService.ConvertFromUtc(DateTime.UtcNow);
            DateTime tomorrow = now.AddDays(1);
            this.EffectiveDate = now.Date;
            this.EffectiveTime = now.TimeOfDay;
            this.ExpiryDate = tomorrow.Date;
            this.ExpiryTime = tomorrow.TimeOfDay;
            this.ActionUrlString = string.Empty;
        }
        else
        {
            DateTime scheduledDateTime = this.DateConversionService.ConvertFromUtc(this.Broadcast.ScheduledDateUtc);
            DateTime? expirationDateTime = this.DateConversionService.ConvertFromUtc(this.Broadcast.ExpirationDateUtc);
            this.EffectiveDate = scheduledDateTime.Date;
            this.EffectiveTime = scheduledDateTime.TimeOfDay;
            this.ExpiryDate = expirationDateTime?.Date;
            this.ExpiryTime = expirationDateTime?.TimeOfDay;
            this.ActionUrlString = this.Broadcast.ActionUrl?.ToString() ?? string.Empty;
        }
    }

    private void RetrieveFormValues()
    {
        this.Broadcast.ScheduledDateUtc = (this.EffectiveDate!.Value + this.EffectiveTime!.Value).ToUniversalTime();
        this.Broadcast.ExpirationDateUtc = this.ExpiryDate == null || this.ExpiryTime == null
            ? null
            : (this.ExpiryDate.Value + this.ExpiryTime.Value).ToUniversalTime();

        this.Broadcast.ActionUrl = this.ActionUrlString.Length > 0 &&
                                   Uri.TryCreate(this.ActionUrlString, UriKind.Absolute, out Uri? result)
            ? result
            : null;
    }

    private void HandleClickCancel()
    {
        this.Dispatcher.Dispatch(new BroadcastsActions.ClearAddErrorAction());
        this.Dispatcher.Dispatch(new BroadcastsActions.ClearUpdateErrorAction());
        this.MudDialog.Cancel();
    }

    private async Task HandleClickSaveAsync()
    {
        await this.Form.Validate();
        if (this.Form.IsValid)
        {
            this.RetrieveFormValues();

            if (this.IsNewBroadcast)
            {
                this.Dispatcher.Dispatch(new BroadcastsActions.AddAction { Broadcast = this.Broadcast });
            }
            else
            {
                this.Dispatcher.Dispatch(new BroadcastsActions.UpdateAction { Broadcast = this.Broadcast });
            }
        }
    }

    private void ActionTypeChanged(BroadcastActionType type)
    {
        this.Broadcast.ActionType = type;
        if (type == BroadcastActionType.None)
        {
            this.ActionUrlString = string.Empty;
        }

        this.ActionUrlTextField.MudComponent.ResetValidation();
    }
}
