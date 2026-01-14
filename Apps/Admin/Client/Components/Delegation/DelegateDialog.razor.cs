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

using System;
using System.Linq;
using System.Threading.Tasks;
using Fluxor;
using Fluxor.Blazor.Web.Components;
using HealthGateway.Admin.Client.Models;
using HealthGateway.Admin.Client.Store.Delegation;
using HealthGateway.Admin.Common.Constants;
using HealthGateway.Common.Data.Utils;
using Microsoft.AspNetCore.Components;
using MudBlazor;

/// <summary>
/// Backing logic for the DelegateDialog component.
/// If the Save button is pressed, the dialog's Result will have the Data property populated with a bool value of true.
/// If the Cancel button is pressed, the dialog's Result will have the Canceled property set to true.
/// </summary>
public partial class DelegateDialog : FluxorComponent
{
    [CascadingParameter]
    private IMudDialogInstance MudDialog { get; set; } = default!;

    [Inject]
    private IDispatcher Dispatcher { get; set; } = default!;

    [Inject]
    private IState<DelegationState> DelegationState { get; set; } = default!;

    private bool Searching => this.DelegationState.Value.DelegateSearch.IsLoading;

    private bool Loaded => this.DelegationState.Value.DelegateSearch.Loaded;

    private bool HasSearchError => this.DelegationState.Value.DelegateSearch.Error is { Message.Length: > 0 };

    private bool DelegateAlreadyAllowed => this.DelegationState.Value.Delegates
        .Any(d => d.PersonalHealthNumber == this.Phn && d.StagedDelegationStatus is DelegationStatus.Added or DelegationStatus.Allowed);

    private string? ErrorMessage => this.HasSearchError ? this.DelegationState.Value.DelegateSearch.Error?.Message : null;

    private ResultModel? Result => this.DelegationState.Value.DelegateSearch.Result != null
        ? new ResultModel(this.DelegationState.Value.DelegateSearch.Result)
        : null;

    private MudForm Form { get; set; } = default!;

    private string Phn { get; set; } = string.Empty;

    private void HandleClickCancel()
    {
        this.Dispatcher.Dispatch(new DelegationActions.ClearDelegateSearchAction());
        this.MudDialog.Cancel();
    }

    private async Task HandleClickSearchAsync()
    {
        await this.Form.Validate();
        if (this.Form.IsValid)
        {
            this.Dispatcher.Dispatch(new DelegationActions.DelegateSearchAction { Phn = this.Phn });
        }
    }

    private void HandleClickSave()
    {
        this.Dispatcher.Dispatch(new DelegationActions.AddDelegateAction { StagedDelegationStatus = DelegationStatus.Allowed });
        this.Dispatcher.Dispatch(new DelegationActions.SetEditModeAction { Enabled = true });
        this.MudDialog.Close();
    }

    private sealed record ResultModel
    {
        public ResultModel(ExtendedDelegateInfo model)
        {
            this.Name = StringManipulator.JoinWithoutBlanks([model.FirstName, model.LastName]);
            this.Address = AddressUtility.GetAddressAsSingleLine(model.PhysicalAddress ?? model.PostalAddress);
            this.DateOfBirth = model.Birthdate;
        }

        public string Name { get; }

        public DateTime DateOfBirth { get; }

        public string Address { get; }
    }
}
