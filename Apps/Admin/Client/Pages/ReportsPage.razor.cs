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

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Fluxor;
using Fluxor.Blazor.Web.Components;
using HealthGateway.Admin.Client.Api;
using HealthGateway.Admin.Client.Store;
using HealthGateway.Admin.Client.Store.AdminReport;
using HealthGateway.Admin.Client.Store.PatientSupport;
using HealthGateway.Admin.Client.Utils;
using HealthGateway.Admin.Common.Models.AdminReports;
using HealthGateway.Common.Data.Constants;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using MudBlazor;
using Refit;
using SortDirection = HealthGateway.Common.Data.Constants.SortDirection;

/// <summary>
/// Backing logic for the Reports page.
/// </summary>
public partial class ReportsPage : FluxorComponent
{
    [Inject]
    private IActionSubscriber ActionSubscriber { get; set; } = default!;

    [Inject]
    private IDispatcher Dispatcher { get; set; } = default!;

    [Inject]
    private NavigationManager NavigationManager { get; set; } = default!;

    [Inject]
    private IState<AdminReportState> AdminReportState { get; set; } = default!;

    [Inject]
    private IJSRuntime JsRuntime { get; set; } = default!;

    [Inject]
    private IAdminReportApi AdminReportApi { get; set; } = default!;

    private string? ProtectedDependentsErrorMessage { get; set; }

    private BaseRequestState<IEnumerable<BlockedAccessRecord>> BlockedAccessState => this.AdminReportState.Value.BlockedAccess;

    private IEnumerable<BlockedAccessRecord> BlockedAccessRecords => this.BlockedAccessState.Result ?? Enumerable.Empty<BlockedAccessRecord>();

    private string? BlockedAccessErrorMessage => this.BlockedAccessState.Error?.Message;

    private bool HasBlockedAccessError => this.BlockedAccessErrorMessage?.Length > 0;

    private bool HasProtectedDependentsError => this.ProtectedDependentsErrorMessage?.Length > 0;

    /// <inheritdoc/>
    protected override void OnInitialized()
    {
        base.OnInitialized();
        this.Dispatcher.Dispatch(new AdminReportActions.ResetStateAction());
        this.Dispatcher.Dispatch(new AdminReportActions.GetBlockedAccessAction());
    }

    private async Task<TableData<ProtectedDependentRecord>> GetProtectedDependentsTableData(TableState tableState)
    {
        SortDirection sortDirection = tableState.SortDirection == MudBlazor.SortDirection.Descending ? SortDirection.Descending : SortDirection.Ascending;
        ProtectedDependentReport report = new(ImmutableArray<ProtectedDependentRecord>.Empty, new ReportMetadata(0, tableState.Page, tableState.PageSize));
        try
        {
            report = await this.AdminReportApi.GetProtectedDependentsReport(tableState.Page, tableState.PageSize, sortDirection);
        }
        catch (Exception e) when (e is ApiException or HttpRequestException)
        {
            RequestError error = StoreUtility.FormatRequestError(e);
            this.ProtectedDependentsErrorMessage = error.Message;
        }

        return new TableData<ProtectedDependentRecord>
        {
            Items = report.Records,
            TotalItems = report.Metadata.TotalCount,
        };
    }

    private async Task HandleBlockedAccessRowClick(TableRowClickEventArgs<BlockedAccessRecord> args)
    {
        await StoreUtility.LoadPatientSupportAction(this.Dispatcher, this.JsRuntime, PatientQueryType.Hdid, args.Item.Hdid);
        this.ActionSubscriber.SubscribeToAction<PatientSupportActions.LoadSuccessAction>(this, this.NavigateToPatientDetails);
    }

    private void HandleProtectedDependentsRowClick(TableRowClickEventArgs<ProtectedDependentRecord> args)
    {
        if (args.Item.Phn != null)
        {
            this.NavigationManager.NavigateTo("/delegation?Phn=" + args.Item.Phn);
        }
    }

    private void NavigateToPatientDetails(PatientSupportActions.LoadSuccessAction action)
    {
        if (action.Data.Count == 1)
        {
            this.NavigationManager.NavigateTo($"/patient-details?phn={action.Data.Single().PersonalHealthNumber}");
        }
    }
}
