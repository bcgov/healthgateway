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
using System.Threading.Tasks;
using Fluxor;
using Fluxor.Blazor.Web.Components;
using HealthGateway.Admin.Client.Store.Communications;
using HealthGateway.Admin.Common.Models;
using HealthGateway.Database.Constants;
using Microsoft.AspNetCore.Components;
using MudBlazor;

/// <summary>
/// Backing logic for the BannerDialog component.
/// If the Save button is pressed, the dialog's Result will have the Data property populated with a bool value of true.
/// If the Cancel button is pressed, the dialog's Result will have the Cancelled property set to true.
/// </summary>
public partial class BannerDialog : FluxorComponent
{
    /// <summary>
    /// Gets or sets the communication model corresponding to the banner that is being edited.
    /// </summary>
    [Parameter]
    public Communication Communication { get; set; } = default!;

    private static List<CommunicationStatus> CommunicationStatuses => new() { CommunicationStatus.Draft, CommunicationStatus.New };

    [CascadingParameter]
    private MudDialogInstance MudDialog { get; set; } = default!;

    [Inject]
    private IDispatcher Dispatcher { get; set; } = default!;

    [Inject]
    private IActionSubscriber ActionSubscriber { get; set; } = default!;

    [Inject]
    private IState<CommunicationsState> CommunicationsState { get; set; } = default!;

    private bool IsNewBanner => this.Communication.Id == Guid.Empty;

    private bool HasAddError => this.CommunicationsState.Value.Add.Error != null && this.CommunicationsState.Value.Add.Error.Message.Length > 0;

    private bool HasUpdateError => this.CommunicationsState.Value.Update.Error != null && this.CommunicationsState.Value.Update.Error.Message.Length > 0;

    private string? ErrorMessage => this.HasAddError ?
        this.CommunicationsState.Value.Add.Error?.Message :
        this.CommunicationsState.Value.Update.Error?.Message;

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
        this.ActionSubscriber.SubscribeToAction<CommunicationsActions.AddSuccessAction>(this, (action) => this.MudDialog.Close(true));
        this.ActionSubscriber.SubscribeToAction<CommunicationsActions.UpdateSuccessAction>(this, (action) => this.MudDialog.Close(true));
        this.SetFormValues();
    }

    private void SetFormValues()
    {
        if (!this.IsNewBanner)
        {
            this.EffectiveDate = this.Communication.EffectiveDateTime.Date;
            this.EffectiveTime = this.Communication.EffectiveDateTime.TimeOfDay;
            this.ExpiryDate = this.Communication.ExpiryDateTime.Date;
            this.ExpiryTime = this.Communication.ExpiryDateTime.TimeOfDay;
            this.HtmlContent = this.Communication.Text;
        }
    }

    private async Task RetrieveFormValuesAsync()
    {
        this.Communication.EffectiveDateTime = this.EffectiveDate!.Value + this.EffectiveTime!.Value;
        this.Communication.ExpiryDateTime = this.ExpiryDate!.Value + this.ExpiryTime!.Value;
        this.Communication.Text = await this.RichTextEditor.BlazoredComponent.GetHTML().ConfigureAwait(true);
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
            await this.RetrieveFormValuesAsync().ConfigureAwait(true);

            if (this.IsNewBanner)
            {
                this.Dispatcher.Dispatch(new CommunicationsActions.AddAction(this.Communication));
            }
            else
            {
                this.Dispatcher.Dispatch(new CommunicationsActions.UpdateAction(this.Communication));
            }
        }
    }
}
