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
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Fluxor;
    using Fluxor.Blazor.Web.Components;
    using HealthGateway.Admin.Client.Authorization;
    using HealthGateway.Admin.Client.Store.PatientDetails;
    using HealthGateway.Admin.Client.Store.PatientSupport;
    using HealthGateway.Admin.Common.Constants;
    using HealthGateway.Admin.Common.Models;
    using HealthGateway.Admin.Common.Models.CovidSupport;
    using HealthGateway.Common.Data.Constants;
    using HealthGateway.Common.Data.Models;
    using HealthGateway.Common.Data.Utils;
    using HealthGateway.Common.Data.ViewModels;
    using Microsoft.AspNetCore.Components;
    using Microsoft.AspNetCore.Components.Authorization;
    using Microsoft.AspNetCore.WebUtilities;
    using Microsoft.Extensions.Configuration;
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
        private IConfiguration Configuration { get; set; } = default!;

        [Inject]
        private AuthenticationStateProvider AuthenticationStateProvider { get; set; } = default!;

        private bool PatientSupportDetailsLoading => this.PatientDetailsState.Value.IsLoading;

        private bool HasPatientSupportDetailsError => this.PatientDetailsState.Value.Error is { Message.Length: > 0 };

        private IEnumerable<MessagingVerificationModel> MessagingVerifications => this.PatientDetailsState.Value.MessagingVerifications ?? Enumerable.Empty<MessagingVerificationModel>();

        private IEnumerable<DataSource> BlockedDataSources => this.PatientDetailsState.Value.BlockedDataSources ?? Enumerable.Empty<DataSource>();

        private IEnumerable<AgentAction> AgentAuditHistory =>
            this.PatientDetailsState.Value.AgentActions?.OrderByDescending(a => a.TransactionDateTime) ?? Enumerable.Empty<AgentAction>();

        private IEnumerable<VaccineDose> VaccineDoses =>
            this.PatientDetailsState.Value.VaccineDetails?.Doses
                .Where(_ => this.PatientDetailsState.Value.VaccineDetails?.Blocked == false)
                .OrderByDescending(dose => dose.Date) ?? Enumerable.Empty<VaccineDose>();

        private CovidAssessmentDetailsResponse? AssessmentInfo => this.PatientDetailsState.Value.Result?.CovidAssessmentDetails;

        private IEnumerable<PreviousAssessmentDetails> AssessmentDetails =>
            this.AssessmentInfo?.PreviousAssessmentDetailsList?.OrderByDescending(a => a.DateTimeOfAssessment) ?? Enumerable.Empty<PreviousAssessmentDetails>();

        private bool PatientsLoaded => this.PatientSupportState.Value.Loaded;

        private bool HasPatientsError => this.PatientSupportState.Value.Error is { Message.Length: > 0 };

        private bool HasPatientsWarning => this.PatientSupportState.Value.WarningMessages.Any();

        private PatientSupportResult? Patient =>
            this.PatientSupportState.Value.Result?.SingleOrDefault(x => x.Hdid == this.Hdid);

        private string PatientName => StringManipulator.JoinWithoutBlanks(new[] { this.Patient?.PreferredName?.GivenName, this.Patient?.PreferredName?.Surname });

        private string? StatusWarning => this.Patient == null ? null : MapStatusToWarning(this.Patient.Status);

        private Address? MailAddress => this.Patient?.PostalAddress ?? this.Patient?.PhysicalAddress;

        private DateTime? ProfileCreatedDateTime =>
            this.Patient?.ProfileCreatedDateTime == null ? null : TimeZoneInfo.ConvertTimeFromUtc(this.Patient.ProfileCreatedDateTime.Value, this.GetTimeZone());

        private DateTime? ProfileLastLoginDateTime =>
            this.Patient?.ProfileLastLoginDateTime == null ? null : TimeZoneInfo.ConvertTimeFromUtc(this.Patient.ProfileLastLoginDateTime.Value, this.GetTimeZone());

        private bool CanViewAccountDetails => this.UserHasRole(Roles.Admin) || this.UserHasRole(Roles.Reviewer);

        private bool CanViewMessagingVerifications => this.UserHasRole(Roles.Admin) || this.UserHasRole(Roles.Reviewer);

        private bool CanViewDatasetAccess => this.UserHasRole(Roles.Admin) || this.UserHasRole(Roles.Reviewer);

        private bool CanEditDatasetAccess => this.UserHasRole(Roles.Admin);

        private bool CanViewAgentAuditHistory => this.UserHasRole(Roles.Admin) || this.UserHasRole(Roles.Reviewer);

        private bool CanViewHdid => !this.UserHasRole(Roles.Support);

        private bool CanViewCovidDetails => this.UserHasRole(Roles.Support);

        private string Covid19TreatmentAssessmentPath => $"/covid-19-treatment-assessment?hdid={this.Hdid}";

        private AuthenticationState? AuthenticationState { get; set; }

        private string Hdid { get; set; } = string.Empty;

        private string PreviousSearchedHdid { get; set; } = string.Empty;

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
            if (QueryHelpers.ParseQuery(uri.Query).TryGetValue("hdid", out StringValues hdid) && hdid != StringValues.Empty)
            {
                this.Hdid = hdid.ToString();
                this.RetrievePatientDetails();
            }
            else
            {
                this.NavigationManager.NavigateTo("/support");
            }
        }

        private static string? MapStatusToWarning(PatientStatus status)
        {
            return status switch
            {
                PatientStatus.NotFound => "Patient not found",
                PatientStatus.Deceased => "Patient is deceased",
                PatientStatus.NotUser => "Patient is not a user",
                _ => null,
            };
        }

        private void RetrievePatientDetails()
        {
            if (this.Patient?.Hdid != this.PreviousSearchedHdid)
            {
                this.Dispatcher.Dispatch(new PatientSupportActions.ResetStateAction());
                this.Dispatcher.Dispatch(new PatientSupportActions.LoadAction { QueryType = PatientQueryType.Hdid, QueryString = this.Hdid });
                this.Dispatcher.Dispatch(new PatientDetailsActions.ResetStateAction());
                this.Dispatcher.Dispatch(new PatientDetailsActions.LoadAction { Hdid = this.Hdid });
            }

            this.PreviousSearchedHdid = this.Hdid;
        }

        private bool UserHasRole(string role)
        {
            return this.AuthenticationState?.User.IsInRole(role) == true;
        }

        private TimeZoneInfo GetTimeZone()
        {
            return DateFormatter.GetLocalTimeZone(this.Configuration);
        }
    }
}
