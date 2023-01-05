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

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using Fluxor;
using Fluxor.Blazor.Web.Components;
using HealthGateway.Admin.Client.Components;
using HealthGateway.Admin.Client.Models;
using HealthGateway.Admin.Client.Store.Broadcasts;
using HealthGateway.Admin.Client.Store.Communications;
using HealthGateway.Admin.Common.Models;
using HealthGateway.Common.Data.Constants;
using HealthGateway.Common.Data.Models;
using Microsoft.AspNetCore.Components;
using MudBlazor;

/// <summary>
/// Backing logic for the communications page.
/// </summary>
public partial class CommunicationsPage : FluxorComponent
{
    [Inject]
    private IDispatcher Dispatcher { get; set; } = default!;

    [Inject]
    private IActionSubscriber ActionSubscriber { get; set; } = default!;

    [Inject]
    private IState<BroadcastsState> BroadcastsState { get; set; } = default!;

    [Inject]
    private IState<CommunicationsState> CommunicationsState { get; set; } = default!;

    [Inject]
    private IDialogService Dialog { get; set; } = default!;

    [Inject]
    private ISnackbar Snackbar { get; set; } = default!;

    [SuppressMessage("Minor Code Smell", "S3459:Unassigned members should be removed", Justification = "Assigned in .razor file")]
    private HgTabs? Tabs { get; set; }

    private bool BroadcastsLoading => this.BroadcastsState.Value.IsLoading;

    private bool BroadcastsLoaded => this.BroadcastsState.Value.Loaded;

    private bool CommunicationsLoading => this.CommunicationsState.Value.IsLoading;

    private bool CommunicationsLoaded => this.CommunicationsState.Value.Loaded;

    private bool HasBroadcastsLoadError =>
        this.BroadcastsState.Value.Load.Error != null && this.BroadcastsState.Value.Load.Error.Message.Length > 0;

    private bool HasBroadcastsDeleteError =>
        this.BroadcastsState.Value.Delete.Error != null && this.BroadcastsState.Value.Delete.Error.Message.Length > 0;

    private string? BroadcastsErrorMessage =>
        this.HasBroadcastsLoadError ? this.BroadcastsState.Value.Load.Error?.Message : this.BroadcastsState.Value.Delete.Error?.Message;

    private bool HasCommunicationsLoadError =>
        this.CommunicationsState.Value.Load.Error != null && this.CommunicationsState.Value.Load.Error.Message.Length > 0;

    private bool HasCommunicationsDeleteError =>
        this.CommunicationsState.Value.Delete.Error != null && this.CommunicationsState.Value.Delete.Error.Message.Length > 0;

    private string? CommunicationsErrorMessage =>
        this.HasCommunicationsLoadError ? this.CommunicationsState.Value.Load.Error?.Message : this.CommunicationsState.Value.Delete.Error?.Message;

    private bool IsModalShown { get; set; }

    private IEnumerable<ExtendedBroadcast> Broadcasts => this.BroadcastsState.Value.Data?.Values ?? Enumerable.Empty<ExtendedBroadcast>();

    private IEnumerable<ExtendedCommunication> AllCommunications =>
        this.CommunicationsState.Value.Data ?? Enumerable.Empty<ExtendedCommunication>();

    private IEnumerable<ExtendedCommunication> PublicCommunications =>
        this.AllCommunications.Where(c => c.CommunicationTypeCode == CommunicationType.Banner);

    private IEnumerable<ExtendedCommunication> InAppCommunications =>
        this.AllCommunications.Where(c => c.CommunicationTypeCode == CommunicationType.InApp);

    private IEnumerable<ExtendedCommunication> MobileCommunications =>
        this.AllCommunications.Where(c => c.CommunicationTypeCode == CommunicationType.Mobile);

    private CommunicationType? SelectedCommunicationType =>
        this.Tabs?.MudComponent.ActivePanelIndex switch
        {
            1 => CommunicationType.Banner,
            2 => CommunicationType.InApp,
            3 => CommunicationType.Mobile,
            _ => null,
        };

    private string? SelectedCommunicationName =>
        this.SelectedCommunicationType switch
        {
            CommunicationType.Banner => "Public Banner",
            CommunicationType.InApp => "In-App Banner",
            CommunicationType.Mobile => "Mobile Communication",
            _ => null,
        };

    /// <inheritdoc/>
    protected override void OnInitialized()
    {
        base.OnInitialized();
        this.ResetState();
        this.Dispatcher.Dispatch(new BroadcastsActions.LoadAction());
        this.Dispatcher.Dispatch(new CommunicationsActions.LoadAction());
        this.ActionSubscriber.SubscribeToAction<BroadcastsActions.AddSuccessAction>(this, this.DisplayAddBroadcastSuccessful);
        this.ActionSubscriber.SubscribeToAction<BroadcastsActions.UpdateSuccessAction>(this, this.DisplayUpdateBroadcastSuccessful);
        this.ActionSubscriber.SubscribeToAction<BroadcastsActions.DeleteSuccessAction>(this, this.DisplayDeleteBroadcastSuccessful);
        this.ActionSubscriber.SubscribeToAction<CommunicationsActions.AddSuccessAction>(this, this.DisplayAddCommunicationSuccessful);
        this.ActionSubscriber.SubscribeToAction<CommunicationsActions.UpdateSuccessAction>(this, this.DisplayUpdateCommunicationSuccessful);
        this.ActionSubscriber.SubscribeToAction<CommunicationsActions.DeleteSuccessAction>(this, this.DisplayDeleteCommunicationSuccessful);
    }

    private void ResetState()
    {
        this.Dispatcher.Dispatch(new BroadcastsActions.ResetStateAction());
        this.Dispatcher.Dispatch(new CommunicationsActions.ResetStateAction());
    }

    private async Task CreateBroadcastAsync()
    {
        string title = "Create Notification";

        Broadcast broadcast = new();

        await this.OpenBroadcastDialogAsync(title, broadcast).ConfigureAwait(true);
    }

    private async Task EditBroadcastAsync(ExtendedBroadcast broadcast)
    {
        string title = "Edit Notification";

        await this.OpenBroadcastDialogAsync(title, broadcast).ConfigureAwait(true);
    }

    private async Task OpenBroadcastDialogAsync(string title, Broadcast broadcast)
    {
        if (this.IsModalShown)
        {
            return;
        }

        this.IsModalShown = true;

        DialogParameters parameters = new() { [nameof(BroadcastDialog.Broadcast)] = broadcast };
        DialogOptions options = new() { DisableBackdropClick = true };
        IDialogReference dialog = await this.Dialog.ShowAsync<BroadcastDialog>(title, parameters, options).ConfigureAwait(true);

        await dialog.Result.ConfigureAwait(true);
        this.IsModalShown = false;
    }

    private async Task CreateCommunicationAsync()
    {
        CommunicationType? type = this.SelectedCommunicationType;
        if (type == null)
        {
            return;
        }

        string title = $"Create {this.SelectedCommunicationName}";

        Communication communication = new()
        {
            CommunicationTypeCode = type.Value,
            CommunicationStatusCode = CommunicationStatus.Draft,
        };

        await this.OpenCommunicationDialogAsync(title, communication).ConfigureAwait(true);
    }

    private async Task EditCommunicationAsync(ExtendedCommunication communication)
    {
        CommunicationType? type = this.SelectedCommunicationType;
        if (type == null)
        {
            return;
        }

        string title = $"Edit {this.SelectedCommunicationName}";

        await this.OpenCommunicationDialogAsync(title, communication).ConfigureAwait(true);
    }

    private async Task OpenCommunicationDialogAsync(string title, Communication communication)
    {
        if (this.IsModalShown)
        {
            return;
        }

        this.IsModalShown = true;

        DialogParameters parameters = new() { [nameof(CommunicationDialog.Communication)] = communication };
        DialogOptions options = new() { DisableBackdropClick = true };
        IDialogReference dialog = await this.Dialog.ShowAsync<CommunicationDialog>(title, parameters, options).ConfigureAwait(true);

        await dialog.Result.ConfigureAwait(true);
        this.IsModalShown = false;
    }

    private void DisplayAddBroadcastSuccessful(BroadcastsActions.AddSuccessAction action)
    {
        this.Snackbar.Add("Notification added.", Severity.Success);
    }

    private void DisplayUpdateBroadcastSuccessful(BroadcastsActions.UpdateSuccessAction action)
    {
        this.Snackbar.Add("Notification updated.", Severity.Success);
    }

    private void DisplayDeleteBroadcastSuccessful(BroadcastsActions.DeleteSuccessAction action)
    {
        this.Snackbar.Add("Notification deleted.", Severity.Success);
    }

    private void DisplayAddCommunicationSuccessful(CommunicationsActions.AddSuccessAction action)
    {
        this.Snackbar.Add("Communication added.", Severity.Success);
    }

    private void DisplayUpdateCommunicationSuccessful(CommunicationsActions.UpdateSuccessAction action)
    {
        this.Snackbar.Add("Communication updated.", Severity.Success);
    }

    private void DisplayDeleteCommunicationSuccessful(CommunicationsActions.DeleteSuccessAction action)
    {
        this.Snackbar.Add("Communication deleted.", Severity.Success);
    }
}
