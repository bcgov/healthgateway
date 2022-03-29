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
using System.Linq;
using System.Threading.Tasks;
using Fluxor;
using Fluxor.Blazor.Web.Components;
using HealthGateway.Admin.Client.Components;
using HealthGateway.Admin.Client.Models;
using HealthGateway.Admin.Client.Store.Communications;
using HealthGateway.Admin.Common.Models;
using HealthGateway.Database.Constants;
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
    private IState<CommunicationsState> CommunicationsState { get; set; } = default!;

    [Inject]
    private IDialogService Dialog { get; set; } = default!;

    [Inject]
    private ISnackbar Snackbar { get; set; } = default!;

    private HgTabs? Tabs { get; set; }

    private bool CommunicationsLoading => this.CommunicationsState.Value.IsLoading;

    private bool CommunicationsLoaded => this.CommunicationsState.Value.Loaded;

    private bool HasLoadError => this.CommunicationsState.Value.Load.Error != null && this.CommunicationsState.Value.Load.Error.Message.Length > 0;

    private bool HasDeleteError => this.CommunicationsState.Value.Delete.Error != null && this.CommunicationsState.Value.Delete.Error.Message.Length > 0;

    private string? ErrorMessage => this.HasLoadError ?
        this.CommunicationsState.Value.Load.Error?.Message :
        this.CommunicationsState.Value.Delete.Error?.Message;

    private bool IsModalShown { get; set; }

    private IEnumerable<ExtendedCommunication> AllCommunications =>
        this.CommunicationsState.Value.Data ?? Enumerable.Empty<ExtendedCommunication>();

    private IEnumerable<ExtendedCommunication> GlobalCommunications =>
        this.AllCommunications.Where(c => c.CommunicationTypeCode == CommunicationType.Banner);

    private IEnumerable<ExtendedCommunication> InAppCommunications =>
        this.AllCommunications.Where(c => c.CommunicationTypeCode == CommunicationType.InApp);

    private CommunicationType? SelectedCommunicationType =>
        this.Tabs?.MudComponent?.ActivePanelIndex switch
        {
            0 => CommunicationType.Banner,
            1 => CommunicationType.InApp,
            _ => null,
        };

    private string? SelectedBannerName =>
        this.SelectedCommunicationType switch
        {
            CommunicationType.Banner => "Global",
            CommunicationType.InApp => "In-App",
            _ => null,
        };

    /// <inheritdoc/>
    protected override void OnInitialized()
    {
        base.OnInitialized();
        this.ResetState();
        this.Dispatcher.Dispatch(new CommunicationsActions.LoadAction());
        this.ActionSubscriber.SubscribeToAction<CommunicationsActions.AddSuccessAction>(this, this.DisplayAddSuccessful);
        this.ActionSubscriber.SubscribeToAction<CommunicationsActions.UpdateSuccessAction>(this, this.DisplayUpdateSuccessful);
        this.ActionSubscriber.SubscribeToAction<CommunicationsActions.DeleteSuccessAction>(this, this.DisplayDeleteSuccessful);
    }

    private void ResetState()
    {
        this.Dispatcher.Dispatch(new CommunicationsActions.ResetStateAction());
    }

    private async Task CreateBannerAsync()
    {
        CommunicationType? type = this.SelectedCommunicationType;
        if (type == null)
        {
            return;
        }

        string title = $"Create {this.SelectedBannerName} Banner";
        Communication communication = new()
        {
            CommunicationTypeCode = type!.Value,
            CommunicationStatusCode = CommunicationStatus.Draft,
        };

        await this.OpenBannerDialogAsync(title, communication).ConfigureAwait(true);
    }

    private async Task EditBannerAsync(ExtendedCommunication communication)
    {
        CommunicationType? type = this.SelectedCommunicationType;
        if (type == null)
        {
            return;
        }

        string title = $"Edit {this.SelectedBannerName} Banner";

        await this.OpenBannerDialogAsync(title, communication).ConfigureAwait(true);
    }

    private async Task OpenBannerDialogAsync(string title, Communication communication)
    {
        if (this.IsModalShown)
        {
            return;
        }

        this.IsModalShown = true;

        DialogParameters parameters = new() { [nameof(BannerDialog.Communication)] = communication };
        DialogOptions options = new() { DisableBackdropClick = true };
        IDialogReference dialog = this.Dialog.Show<BannerDialog>(title, parameters, options);

        await dialog.Result.ConfigureAwait(true);
        this.IsModalShown = false;
    }

    private void DisplayAddSuccessful(CommunicationsActions.AddSuccessAction action)
    {
        this.Snackbar.Add("Communication added.", Severity.Success);
    }

    private void DisplayUpdateSuccessful(CommunicationsActions.UpdateSuccessAction action)
    {
        this.Snackbar.Add("Communication updated.", Severity.Success);
    }

    private void DisplayDeleteSuccessful(CommunicationsActions.DeleteSuccessAction action)
    {
        this.Snackbar.Add("Communication deleted.", Severity.Success);
    }
}
