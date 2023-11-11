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
    using HealthGateway.Admin.Common.Models.CovidSupport;
    using HealthGateway.Common.Data.Models;
    using HealthGateway.Common.Data.Utils;
    using Microsoft.AspNetCore.Components;
    using Microsoft.AspNetCore.Components.Authorization;

    /// <summary>
    /// Backing logic for the ProfileTab component.
    /// </summary>
    public partial class ProfileTab : FluxorComponent
    {
        /// <summary>
        /// Gets or sets the patient's personal health number.
        /// </summary>
        [Parameter]
        public string Phn { get; set; } = string.Empty;

        [Inject]
        private IState<PatientDetailsState> PatientDetailsState { get; set; } = default!;

        [Inject]
        private IState<PatientSupportState> PatientSupportState { get; set; } = default!;

        [Inject]
        private AuthenticationStateProvider AuthenticationStateProvider { get; set; } = default!;

        private AuthenticationState? AuthenticationState { get; set; }

        private PatientSupportResult? Patient =>
            this.PatientSupportState.Value.Result?.SingleOrDefault(x => x.PersonalHealthNumber == this.Phn);

        private string PatientName => StringManipulator.JoinWithoutBlanks(new[] { this.Patient?.PreferredName?.GivenName, this.Patient?.PreferredName?.Surname });

        private Address? MailAddress => this.Patient?.PostalAddress ?? this.Patient?.PhysicalAddress;

        private string Covid19TreatmentAssessmentPath => $"/covid-19-treatment-assessment?phn={this.Phn}";

        private VaccineDetails? VaccineDetails => this.PatientDetailsState.Value.VaccineDetails;

        private CovidAssessmentDetailsResponse? AssessmentInfo => this.PatientDetailsState.Value.Result?.CovidAssessmentDetails;

        private IEnumerable<PreviousAssessmentDetails> AssessmentDetails =>
            this.AssessmentInfo?.PreviousAssessmentDetailsList?.OrderByDescending(a => a.DateTimeOfAssessment) ?? Enumerable.Empty<PreviousAssessmentDetails>();

        private bool CanViewCovidDetails => this.UserHasRole(Roles.Support) || this.UserHasRole(Roles.Admin);

        private bool ImmunizationsAreBlocked => this.VaccineDetails?.Blocked ?? false;

        private bool PatientSupportDetailsLoading => this.PatientDetailsState.Value.IsLoading;

        /// <inheritdoc/>
        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();
            this.AuthenticationState = await this.AuthenticationStateProvider.GetAuthenticationStateAsync();
        }

        private bool UserHasRole(string role)
        {
            return this.AuthenticationState?.User.IsInRole(role) == true;
        }
    }
}
