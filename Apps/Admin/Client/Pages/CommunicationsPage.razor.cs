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
using Fluxor;
using Fluxor.Blazor.Web.Components;
using HealthGateway.Admin.Client.Models;
using HealthGateway.Admin.Client.Store.Communications;
using Microsoft.AspNetCore.Components;

/// <summary>
/// Backing logic for the communications page.
/// </summary>
public partial class CommunicationsPage : FluxorComponent
{
    [Inject]
    private IDispatcher Dispatcher { get; set; } = default!;

    [Inject]
    private IState<CommunicationsState> CommunicationsState { get; set; } = default!;

    private bool CommunicationsLoading => this.CommunicationsState.Value.IsLoading;

    private bool CommunicationsLoaded => this.CommunicationsState.Value.Loaded;

    private bool HasError => this.CommunicationsState.Value.Error != null && this.CommunicationsState.Value.Error.Message.Length > 0;

    private IEnumerable<ExtendedCommunication> AllCommunications =>
        this.CommunicationsState.Value.Data ?? Enumerable.Empty<ExtendedCommunication>();

    private IEnumerable<ExtendedCommunication> GlobalCommunications =>
        this.AllCommunications.Where(c => c.CommunicationTypeCode == Database.Constants.CommunicationType.Banner);

    private IEnumerable<ExtendedCommunication> InAppCommunications =>
        this.AllCommunications.Where(c => c.CommunicationTypeCode == Database.Constants.CommunicationType.InApp);

    /// <inheritdoc/>
    protected override void OnInitialized()
    {
        base.OnInitialized();
        this.ResetState();
        this.Dispatcher.Dispatch(new CommunicationsActions.LoadAction());
    }

    /// <summary>
    /// Resets the component to its initial state.
    /// </summary>
    private void ResetState()
    {
        this.Dispatcher.Dispatch(new CommunicationsActions.ResetStateAction());
    }
}
