// -------------------------------------------------------------------------
//  Copyright © 2019 Province of British Columbia
//
//  Licensed under the Apache License, Version 2.0 (the "License");
//  you may not use this file except in compliance with the License.
//  You may obtain a copy of the License at
//
//  http://www.apache.org/licenses/LICENSE-2.0
//
//  Unless required by applicable law or agreed to in writing, software
//  distributed under the License is distributed on an "AS IS" BASIS,
//  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  See the License for the specific language governing permissions and
//  limitations under the License.
// -------------------------------------------------------------------------
namespace HealthGateway.Admin.Client.Components.Details
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Fluxor;
    using Fluxor.Blazor.Web.Components;
    using HealthGateway.Admin.Client.Authorization;
    using HealthGateway.Admin.Client.Store.PatientDetails;
    using HealthGateway.Admin.Client.Store.PatientSupport;
    using HealthGateway.Admin.Common.Models;
    using HealthGateway.Common.Data.Constants;
    using Microsoft.AspNetCore.Components;
    using Microsoft.AspNetCore.Components.Authorization;

    /// <summary>
    /// Backing logic for the ManageTab component.
    /// </summary>
    public partial class ManageTab : FluxorComponent
    {
        /// <summary>
        /// Gets or sets the Phn applicable to the current patient.
        /// </summary>
        [Parameter]
        public string Phn { get; set; } = string.Empty;

        [Inject]

        private AuthenticationStateProvider AuthenticationStateProvider { get; set; } = default!;

        [Inject]
        private IState<PatientDetailsState> PatientDetailsState { get; set; } = default!;

        [Inject]
        private IState<PatientSupportState> PatientSupportState { get; set; } = default!;

        private AuthenticationState? AuthenticationState { get; set; }

        private IEnumerable<DataSource> BlockedDataSources => this.PatientDetailsState.Value.BlockedDataSources ?? [];

        private IEnumerable<PatientSupportDependentInfo> Dependents => this.PatientDetailsState.Value.Dependents ?? [];

        private PatientSupportResult? Patient =>
            this.PatientSupportState.Value.Result?.SingleOrDefault(x => x.PersonalHealthNumber == this.Phn);

        private string Hdid => this.Patient?.Hdid ?? string.Empty;

        private bool PatientSupportDetailsLoading => this.PatientDetailsState.Value.IsLoading;

        private bool CanEditDatasetAccess => this.AuthenticationState?.User.IsInRole(Roles.Admin) == true;

        /// <inheritdoc/>
        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();
            this.AuthenticationState = await this.AuthenticationStateProvider.GetAuthenticationStateAsync();
        }
    }
}
