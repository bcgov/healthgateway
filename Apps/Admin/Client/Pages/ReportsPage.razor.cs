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
using HealthGateway.Admin.Client.Store;
using HealthGateway.Admin.Client.Store.AdminReport;
using HealthGateway.Admin.Common.Models;
using Microsoft.AspNetCore.Components;
using MudBlazor;

/// <summary>
/// Backing logic for the Reports page.
/// </summary>
public partial class ReportsPage : FluxorComponent
{
    [Inject]
    private IDispatcher Dispatcher { get; set; } = default!;

    [Inject]
    private NavigationManager NavigationManager { get; set; } = default!;

    [Inject]
    private IState<AdminReportState> AdminReportState { get; set; } = default!;

    private BaseRequestState<IEnumerable<BlockedAccessRecord>> BlockedAccessState => this.AdminReportState.Value.BlockedAccess;

    private BaseRequestState<IEnumerable<string>> ProtectedDependentsState => this.AdminReportState.Value.ProtectedDependents;

    private IEnumerable<BlockedAccessRecord> BlockedAccessRecords => this.BlockedAccessState.Result ?? Enumerable.Empty<BlockedAccessRecord>();

    private IEnumerable<string> ProtectedDependentHdids => this.ProtectedDependentsState.Result ?? Enumerable.Empty<string>();

    private string? BlockedAccessErrorMessage => this.BlockedAccessState.Error?.Message;

    private bool HasBlockedAccessError => this.BlockedAccessErrorMessage?.Length > 0;

    private string? ProtectedDependentsErrorMessage => this.ProtectedDependentsState.Error?.Message;

    private bool HasProtectedDependentsError => this.ProtectedDependentsErrorMessage?.Length > 0;

    /// <inheritdoc/>
    protected override void OnInitialized()
    {
        base.OnInitialized();
        this.Dispatcher.Dispatch(new AdminReportActions.ResetStateAction());
        this.Dispatcher.Dispatch(new AdminReportActions.GetBlockedAccessAction());
        this.Dispatcher.Dispatch(new AdminReportActions.GetProtectedDependentsAction());
    }

    private void HandleBlockedAccessRowClick(TableRowClickEventArgs<BlockedAccessRecord> args)
    {
        this.NavigationManager.NavigateTo($"/patient-details?hdid={args.Item.Hdid}");
    }
}
