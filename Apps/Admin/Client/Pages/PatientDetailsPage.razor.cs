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
namespace HealthGateway.Admin.Client.Pages
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Fluxor;
    using Fluxor.Blazor.Web.Components;
    using HealthGateway.Admin.Client.Authorization;
    using HealthGateway.Admin.Client.Store.PatientDetails;
    using HealthGateway.Admin.Client.Store.PatientSupport;
    using HealthGateway.Admin.Client.Utils;
    using HealthGateway.Admin.Common.Constants;
    using HealthGateway.Admin.Common.Models;
    using HealthGateway.Common.Data.Constants;
    using HealthGateway.Common.Data.Extensions;
    using Microsoft.AspNetCore.Components;
    using Microsoft.AspNetCore.Components.Authorization;
    using Microsoft.AspNetCore.WebUtilities;
    using Microsoft.Extensions.Primitives;

    /// <summary>
    /// Backing logic for the Patient Details page.
    /// </summary>
    public partial class PatientDetailsPage : FluxorComponent
    {
        [Inject]
        private IDispatcher Dispatcher { get; set; } = default!;

        [Inject]
        private IState<PatientDetailsState> PatientDetailsState { get; set; } = default!;

        [Inject]
        private IState<PatientSupportState> PatientSupportState { get; set; } = default!;

        [Inject]
        private NavigationManager NavigationManager { get; set; } = default!;

        [Inject]
        private AuthenticationStateProvider AuthenticationStateProvider { get; set; } = default!;

        private bool PatientsLoaded => this.PatientSupportState.Value.Loaded;

        private bool HasPatientSupportDetailsError => this.PatientDetailsState.Value.Error is { Message.Length: > 0 };

        private bool HasPatientsError => this.PatientSupportState.Value.Error is { Message.Length: > 0 };

        private bool HasPatientsWarning => this.PatientSupportState.Value.WarningMessages.Any();

        private PatientSupportResult? Patient =>
            this.PatientSupportState.Value.Result?.SingleOrDefault(x => x.PersonalHealthNumber == this.Phn);

        private string? StatusWarning => this.Patient == null ? null : FormattingUtility.FormatPatientStatus(this.Patient.Status);

        private bool IsGatewayUser => this.Patient?.Status == PatientStatus.Default;

        private bool ShowStatusWarning => (this.AuthenticationState?.User).IsInAnyRole(Roles.Admin, Roles.Reviewer) || this.Patient?.Status != PatientStatus.NotUser;

        private AuthenticationState? AuthenticationState { get; set; }

        private string Phn { get; set; } = string.Empty;

        private string PreviousSearchedPhn { get; set; } = string.Empty;

        /// <inheritdoc/>
        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();
            this.AuthenticationState = await this.AuthenticationStateProvider.GetAuthenticationStateAsync();
        }

        /// <inheritdoc/>
        protected override void OnInitialized()
        {
            base.OnInitialized();

            Uri uri = this.NavigationManager.ToAbsoluteUri(this.NavigationManager.Uri);
            if (QueryHelpers.ParseQuery(uri.Query).TryGetValue("phn", out StringValues phn) && phn != StringValues.Empty)
            {
                this.Phn = phn.ToString();
                this.RetrievePatientDetails();
            }
            else
            {
                this.NavigationManager.NavigateTo("/support");
            }
        }

        private void RetrievePatientDetails()
        {
            if (this.Patient?.PersonalHealthNumber != this.PreviousSearchedPhn)
            {
                this.Dispatcher.Dispatch(new PatientSupportActions.ResetStateAction());
                this.Dispatcher.Dispatch(new PatientSupportActions.LoadAction { QueryType = PatientQueryType.Phn, QueryString = this.Phn });
                this.Dispatcher.Dispatch(new PatientDetailsActions.ResetStateAction());
                this.Dispatcher.Dispatch(new PatientDetailsActions.LoadAction { QueryType = ClientRegistryType.Phn, QueryString = this.Phn, RefreshVaccineDetails = false });
            }

            this.PreviousSearchedPhn = this.Phn;
        }
    }
}
