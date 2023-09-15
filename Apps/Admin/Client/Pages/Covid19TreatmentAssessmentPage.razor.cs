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
    using HealthGateway.Admin.Client.Store.PatientDetails;
    using HealthGateway.Admin.Client.Store.PatientSupport;
    using HealthGateway.Admin.Common.Constants;
    using HealthGateway.Admin.Common.Models;
    using HealthGateway.Admin.Common.Models.CovidSupport;
    using HealthGateway.Common.Data.Constants;
    using HealthGateway.Common.Data.Models;
    using HealthGateway.Common.Data.Utils;
    using HealthGateway.Common.Data.Validations;
    using Microsoft.AspNetCore.Components;
    using Microsoft.AspNetCore.WebUtilities;
    using Microsoft.Extensions.Primitives;
    using MudBlazor;
    using AssessmentAddressConfirmationDialog = HealthGateway.Admin.Client.Components.Support.AddressConfirmationDialog<
        HealthGateway.Admin.Client.Store.PatientDetails.PatientDetailsActions.SubmitCovid19TreatmentAssessmentFailAction,
        HealthGateway.Admin.Client.Store.PatientDetails.PatientDetailsActions.SubmitCovid19TreatmentAssessmentSuccessAction>;

    /// <summary>
    /// Backing logic for the COVID-19 Treatment Assessment page.
    /// </summary>
    public partial class Covid19TreatmentAssessmentPage : FluxorComponent
    {
        [Inject]
        private IDispatcher Dispatcher { get; set; } = default!;

        [Inject]
        private IState<PatientDetailsState> PatientDetailsState { get; set; } = default!;

        [Inject]
        private IState<PatientSupportState> PatientSupportState { get; set; } = default!;

        [Inject]
        private IDialogService Dialog { get; set; } = default!;

        [Inject]
        private NavigationManager NavigationManager { get; set; } = default!;

        [Inject]
        private IActionSubscriber ActionSubscriber { get; set; } = default!;

        private bool HasPatientSupportDetailsError => this.PatientDetailsState.Value.Error is { Message.Length: > 0 };

        private bool PatientsLoaded => this.PatientSupportState.Value.Loaded;

        private bool HasPatientsError => this.PatientSupportState.Value.Error is { Message.Length: > 0 };

        private bool HasPatientsWarning => this.PatientSupportState.Value.WarningMessages.Any();

        private PatientSupportResult? Patient =>
            this.PatientSupportState.Value.Result?.SingleOrDefault(x => x.Hdid == this.Hdid);

        private bool PatientDetailsLoaded => this.PatientDetailsState.Value.Loaded;

        private CovidAssessmentDetailsResponse? AssessmentInfo => this.PatientDetailsState.Value.Result?.CovidAssessmentDetails;

        private string PatientName => StringManipulator.JoinWithoutBlanks(new[] { this.Patient?.PreferredName?.GivenName, this.Patient?.PreferredName?.Surname });

        private int Age => AgeRangeValidator.CalculateAge(DateTime.UtcNow, this.Patient?.Birthdate.Value.ToDateTime(TimeOnly.MinValue) ?? DateTime.UtcNow);

        private string? StatusWarning => this.Patient == null ? null : MapStatusToWarning(this.Patient.Status);

        private string PatientDetailsUrl => $"/patient-details?hdid={this.Hdid}";

        private MudForm Form { get; set; } = default!;

        private MudDatePicker SymptomOnsetDatePicker { get; set; } = default!;

        private string? Hdid { get; set; }

        private CovidAssessmentRequest Request { get; } = new();

        private bool SymptomOnsetTooLongAgo
        {
            get
            {
                DateTime? onsetDate = this.Request.SymptomOnsetDate;
                if (onsetDate == null)
                {
                    return false;
                }

                return DateTime.UtcNow.Date > onsetDate.Value.Date.AddDays(10);
            }
        }

        /// <inheritdoc/>
        protected override void OnInitialized()
        {
            base.OnInitialized();

            Uri uri = this.NavigationManager.ToAbsoluteUri(this.NavigationManager.Uri);
            if (QueryHelpers.ParseQuery(uri.Query).TryGetValue("hdid", out StringValues hdid))
            {
                this.Hdid = hdid;
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
            if (this.Patient == null)
            {
                this.Dispatcher.Dispatch(new PatientSupportActions.ResetStateAction());
                this.Dispatcher.Dispatch(new PatientSupportActions.LoadAction(PatientQueryType.Hdid, this.Hdid));
            }

            if (this.AssessmentInfo == null)
            {
                this.Dispatcher.Dispatch(new PatientDetailsActions.ResetStateAction());
                this.Dispatcher.Dispatch(new PatientDetailsActions.LoadAction(this.Hdid));
            }
        }

        private async Task HandleClickConfirm()
        {
            await this.Form.Validate().ConfigureAwait(true);
            if (!this.Form.IsValid)
            {
                return;
            }

            await this.OpenAddressConfirmationDialog();
        }

        private void SubmitAssessment(Address address)
        {
            CovidAssessmentRequest request = new()
            {
                StreetAddresses = address.StreetLines.ToList(),
                City = address.City,
                ProvOrState = address.State,
                PostalCode = address.PostalCode,
                Country = address.Country,
            };
            this.Dispatcher.Dispatch(new PatientDetailsActions.SubmitCovid19TreatmentAssessmentAction { Request = request, Hdid = this.Hdid });
        }

        private async Task OpenAddressConfirmationDialog()
        {
            Address? address = this.Patient?.PostalAddress ?? this.Patient?.PhysicalAddress;

            const string title = "Confirm Address";
            DialogParameters parameters = new()
            {
                [nameof(AssessmentAddressConfirmationDialog.ActionOnConfirm)] = (Action<Address>)this.SubmitAssessment,
                [nameof(AssessmentAddressConfirmationDialog.DefaultAddress)] = address,
                [nameof(AssessmentAddressConfirmationDialog.ConfirmButtonLabel)] = "Send",
                [nameof(AssessmentAddressConfirmationDialog.OutputCountryCodeFormat)] = true,
            };
            DialogOptions options = new()
            {
                DisableBackdropClick = true,
                FullWidth = true,
                MaxWidth = MaxWidth.Small,
            };
            IDialogReference dialog = await this.Dialog
                .ShowAsync<AssessmentAddressConfirmationDialog>(
                    title,
                    parameters,
                    options)
                .ConfigureAwait(true);

            DialogResult result = await dialog.Result.ConfigureAwait(true);
            if (!result.Canceled)
            {
                this.NavigationManager.NavigateTo(this.PatientDetailsUrl);
            }
        }

        private static string? ValidatePhoneNumber(string number)
        {
            if (string.IsNullOrWhiteSpace(number))
            {
                return "Required";
            }

            return !AddressUtility.PhoneNumberRegex().IsMatch(number) ? "Invalid phone number" : null;
        }

        private static string? ValidateRequiredOption(CovidTherapyAssessmentOption option)
        {
            return option == CovidTherapyAssessmentOption.Unspecified ? "Required" : null;
        }
    }
}
