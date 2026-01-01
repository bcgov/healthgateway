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
using System.Threading.Tasks;
using Fluxor;
using Fluxor.Blazor.Web.Components;
using HealthGateway.Admin.Client.Components.Common;
using HealthGateway.Admin.Client.Services;
using HealthGateway.Admin.Client.Store.Communications;
using HealthGateway.Admin.Common.Models;
using HealthGateway.Common.Data.Constants;
using Microsoft.AspNetCore.Components;
using MudBlazor;

/// <summary>
/// Backing logic for the CommunicationDialog component.
/// If the Save button is pressed, the dialog's Result will have the Data property populated with a bool value of true.
/// If the Cancel button is pressed, the dialog's Result will have the Canceled property set to true.
/// </summary>
public partial class CommunicationDialog : FluxorComponent
{
    /// <summary>
    /// Gets or sets the communication model corresponding to the communication that is being edited.
    /// </summary>
    [Parameter]
    public Communication Communication { get; set; } = default!;

    private static IEnumerable<CommunicationStatus> CommunicationStatuses => [CommunicationStatus.Draft, CommunicationStatus.New];

    [CascadingParameter]
    private IMudDialogInstance MudDialog { get; set; } = default!;

    [Inject]
    private IDispatcher Dispatcher { get; set; } = default!;

    [Inject]
    private IActionSubscriber ActionSubscriber { get; set; } = default!;

    [Inject]
    private IState<CommunicationsState> CommunicationsState { get; set; } = default!;

    [Inject]
    private IDateConversionService DateConversionService { get; set; } = default!;

    private bool IsNewCommunication => this.Communication.Id == Guid.Empty;

    private bool HasAddError => this.CommunicationsState.Value.Add.Error != null && this.CommunicationsState.Value.Add.Error.Message.Length > 0;

    private bool HasUpdateError => this.CommunicationsState.Value.Update.Error != null && this.CommunicationsState.Value.Update.Error.Message.Length > 0;

    private string? ErrorMessage => this.HasAddError ? this.CommunicationsState.Value.Add.Error?.Message : this.CommunicationsState.Value.Update.Error?.Message;

    private MudForm Form { get; set; } = default!;

    private MudDatePicker EffectiveDatePicker { get; set; } = default!;

    private MudTimePicker EffectiveTimePicker { get; set; } = default!;

    private MudDatePicker ExpiryDatePicker { get; set; } = default!;

    private MudTimePicker ExpiryTimePicker { get; set; } = default!;

    private HgRichTextEditor RichTextEditor { get; set; } = default!;

    private DateTime? EffectiveDate { get; set; }

    private TimeSpan? EffectiveTime { get; set; }

    private DateTime? ExpiryDate { get; set; }

    private TimeSpan? ExpiryTime { get; set; }

    private string HtmlContent { get; set; } = string.Empty;

    /// <inheritdoc/>
    protected override void OnInitialized()
    {
        base.OnInitialized();
        this.ActionSubscriber.SubscribeToAction<CommunicationsActions.AddSuccessAction>(this, _ => this.MudDialog.Close(true));
        this.ActionSubscriber.SubscribeToAction<CommunicationsActions.UpdateSuccessAction>(this, _ => this.MudDialog.Close(true));
        this.SetFormValues();
    }

    private void SetFormValues()
    {
        if (this.IsNewCommunication)
        {
            DateTime now = this.DateConversionService.ConvertFromUtc(DateTime.UtcNow);
            DateTime tomorrow = now.AddDays(1);
            this.EffectiveDate = now.Date;
            this.EffectiveTime = now.TimeOfDay;
            this.ExpiryDate = tomorrow.Date;
            this.ExpiryTime = tomorrow.TimeOfDay;
        }
        else
        {
            DateTime effectiveDateTime = this.DateConversionService.ConvertFromUtc(this.Communication.EffectiveDateTime);
            DateTime expiryDateTime = this.DateConversionService.ConvertFromUtc(this.Communication.ExpiryDateTime);
            this.EffectiveDate = effectiveDateTime.Date;
            this.EffectiveTime = effectiveDateTime.TimeOfDay;
            this.ExpiryDate = expiryDateTime.Date;
            this.ExpiryTime = expiryDateTime.TimeOfDay;
            this.HtmlContent = this.Communication.Text;
        }
    }

    private async Task RetrieveFormValuesAsync()
    {
        this.Communication.EffectiveDateTime = (this.EffectiveDate!.Value + this.EffectiveTime!.Value).ToUniversalTime();
        this.Communication.ExpiryDateTime = (this.ExpiryDate!.Value + this.ExpiryTime!.Value).ToUniversalTime();
        this.Communication.Text = await this.RichTextEditor.BlazoredComponent.GetHTML();
    }

    private void HandleClickCancel()
    {
        this.Dispatcher.Dispatch(new CommunicationsActions.ClearAddErrorAction());
        this.Dispatcher.Dispatch(new CommunicationsActions.ClearUpdateErrorAction());
        this.MudDialog.Cancel();
    }

    private async Task HandleClickSaveAsync()
    {
        await this.Form.Validate();
        if (this.Form.IsValid)
        {
            await this.RetrieveFormValuesAsync();

            if (this.IsNewCommunication)
            {
                this.Dispatcher.Dispatch(new CommunicationsActions.AddAction { Communication = this.Communication });
            }
            else
            {
                this.Dispatcher.Dispatch(new CommunicationsActions.UpdateAction { Communication = this.Communication });
            }
        }
    }
}
