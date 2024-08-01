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
namespace HealthGateway.Admin.Client.Components.BetaAccess
{
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;
    using Fluxor;
    using Fluxor.Blazor.Web.Components;
    using HealthGateway.Admin.Client.Api;
    using HealthGateway.Admin.Client.Store;
    using HealthGateway.Admin.Client.Store.BetaAccess;
    using HealthGateway.Admin.Client.Utils;
    using HealthGateway.Admin.Common.Constants;
    using HealthGateway.Admin.Common.Models;
    using HealthGateway.Common.Data.Models;
    using Microsoft.AspNetCore.Components;
    using Microsoft.Extensions.Logging;
    using MudBlazor;
    using Refit;

    /// <summary>
    /// Backing logic for the AllBetaAccessTable component.
    /// </summary>
    public partial class AllBetaAccessTable : FluxorComponent
    {
        [Inject]
        private IDispatcher Dispatcher { get; set; } = default!;

        [Inject]
        private IActionSubscriber ActionSubscriber { get; set; } = default!;

        [Inject]
        private IState<BetaAccessState> BetaAccessState { get; set; } = default!;

        [Inject]
        private IBetaFeatureApi BetaFeatureApi { get; set; } = default!;

        [Inject]
        private ILogger<AllBetaAccessTable> Logger { get; set; } = default!;

        private BaseRequestState<IEnumerable<UserBetaAccess>> GetAllUserAccessState => this.BetaAccessState.Value.GetAllUserAccess;

        private MudTable<UserBetaAccess> Table { get; set; } = default!;

        /// <inheritdoc />
        protected override void OnInitialized()
        {
            base.OnInitialized();
            this.ActionSubscriber.SubscribeToAction<BetaAccessActions.SetUserAccessSuccessAction>(this, _ => this.Table.ReloadServerData());
        }

        private void HandleToggleAccess(UserBetaAccess row, BetaFeature betaFeature, bool value)
        {
            if (value)
            {
                row.BetaFeatures.Add(betaFeature);
            }
            else
            {
                row.BetaFeatures.Remove(betaFeature);
            }

            this.Dispatcher.Dispatch(new BetaAccessActions.SetUserAccessAction { Request = new() { Email = row.Email, BetaFeatures = row.BetaFeatures } });
        }

        private async Task<TableData<UserBetaAccess>> GetAllBetaAccessAsync(TableState state, CancellationToken token)
        {
            this.Dispatcher.Dispatch(new BetaAccessActions.GetAllUserAccessAction());
            this.Logger.LogInformation("Retrieving all user access");

            try
            {
                PaginatedResult<UserBetaAccess> response = await this.BetaFeatureApi.GetAllUserAccessAsync(state.Page, state.PageSize, token);

                this.Logger.LogInformation("All user access retrieved successfully");
                this.Dispatcher.Dispatch(new BetaAccessActions.GetAllUserAccessSuccessAction { Data = response.Data });

                return new TableData<UserBetaAccess> { Items = response.Data, TotalItems = response.TotalCount };
            }
            catch (Exception e) when (e is ApiException or HttpRequestException)
            {
                RequestError error = StoreUtility.FormatRequestError(e);
                this.Logger.LogError(e, "Error retrieving all user access, reason: {Message}", e.Message);
                this.Dispatcher.Dispatch(new BetaAccessActions.GetAllUserAccessFailureAction { Error = error });

                return new TableData<UserBetaAccess> { Items = [], TotalItems = 0 };
            }
        }
    }
}
