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
    using HealthGateway.Admin.Client.Models;
    using HealthGateway.Admin.Client.Store.MessageVerification;
    using HealthGateway.Admin.Client.Store.PatientSupport;
    using HealthGateway.Common.Data.Constants;
    using HealthGateway.Common.Data.Utils;
    using HealthGateway.Common.Data.Validations;
    using HealthGateway.Common.Data.ViewModels;
    using Microsoft.AspNetCore.Components;
    using Microsoft.AspNetCore.WebUtilities;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Primitives;
    using MudBlazor;

    /// <summary>
    /// Backing logic for the Support page.
    /// </summary>
    public partial class SupportPage : FluxorComponent
    {
        private static readonly PhnValidator PhnValidator = new();

        private static List<PatientQueryType> QueryTypes => new() { PatientQueryType.Phn, PatientQueryType.Email, PatientQueryType.Sms, PatientQueryType.Hdid, PatientQueryType.Dependent };

        [Inject]
        private IDispatcher Dispatcher { get; set; } = default!;

        [Inject]
        private IState<MessageVerificationState> MessageVerificationState { get; set; } = default!;

        [Inject]
        private IState<PatientSupportState> PatientSupportState { get; set; } = default!;

        [Inject]
        private NavigationManager NavigationManager { get; set; } = default!;

        [Inject]
        private IConfiguration Configuration { get; set; } = default!;

        private PatientQueryType QueryType { get; set; } = PatientQueryType.Phn;

        private PatientQueryType SelectedQueryType
        {
            get => this.QueryType;

            set
            {
                this.ResetPatientSupportState();
                this.QueryParameter = string.Empty;
                this.QueryType = value;
            }
        }

        private string QueryParameter { get; set; } = string.Empty;

        private MudForm Form { get; set; } = default!;

        private bool MessagingVerificationsLoading => this.MessageVerificationState.Value.IsLoading;

        private bool PhnOrDependentSelected => this.SelectedQueryType is PatientQueryType.Phn or PatientQueryType.Dependent;

        private bool HasMessagingVerificationsError => this.MessageVerificationState.Value.Error is { Message.Length: > 0 };

        private IEnumerable<MessagingVerificationModel> MessagingVerifications => this.MessageVerificationState.Value.Data ?? Enumerable.Empty<MessagingVerificationModel>();

        private bool PatientsLoading => this.PatientSupportState.Value.IsLoading;

        private bool PatientsLoaded => this.PatientSupportState.Value.Loaded;

        private bool HasPatientsError => this.PatientSupportState.Value.Error is { Message.Length: > 0 };

        private bool HasPatientsWarning => this.PatientSupportState.Value.WarningMessage is { Length: > 0 };

        private IEnumerable<ExtendedPatientSupportDetails> Patients =>
            this.PatientSupportState.Value.Data ?? Enumerable.Empty<ExtendedPatientSupportDetails>();

        private IEnumerable<PatientRow> PatientRows => this.Patients.Select(v => new PatientRow(v, this.GetTimeZone()));

        private Func<string, string?> ValidateQueryParameter => parameter =>
        {
            if (string.IsNullOrWhiteSpace(parameter))
            {
                return "Search parameter is required";
            }

            if (this.PhnOrDependentSelected && !PhnValidator.Validate(StringManipulator.StripWhitespace(parameter)).IsValid)
            {
                return "Invalid PHN";
            }

            if ((this.SelectedQueryType == PatientQueryType.Email || this.SelectedQueryType == PatientQueryType.Sms) && StringManipulator.StripWhitespace(parameter)?.Length < 5)
            {
                return "Email/SMS must be minimum 5 characters";
            }

            if (this.SelectedQueryType == PatientQueryType.Sms && !StringManipulator.IsPositiveNumeric(parameter))
            {
                return "SMS must contain digits only";
            }

            return null;
        };

        /// <inheritdoc/>
        protected override void OnInitialized()
        {
            base.OnInitialized();
            this.ResetPatientSupportState();
            this.ResetMessageVerificationState();

            Uri uri = this.NavigationManager.ToAbsoluteUri(this.NavigationManager.Uri);

            if (QueryHelpers.ParseQuery(uri.Query).TryGetValue(PatientQueryType.Hdid.ToString(), out StringValues hdid))
            {
                this.Dispatcher.Dispatch(new PatientSupportActions.LoadAction(PatientQueryType.Hdid, StringManipulator.StripWhitespace(hdid)));
                this.QueryParameter = hdid!;
                this.SelectedQueryType = PatientQueryType.Hdid;
            }
        }

        private static string FormatQueryType(PatientQueryType queryType)
        {
            return queryType switch
            {
                PatientQueryType.Hdid => "HDID",
                PatientQueryType.Phn => "PHN",
                PatientQueryType.Sms => "SMS",
                _ => queryType.ToString(),
            };
        }

        private IList<MessagingVerificationModel> GetMessagingVerificationModels(string hdid)
        {
            return this.MessagingVerifications.Where(v => v.UserProfileId == hdid).ToList();
        }

        private TimeZoneInfo GetTimeZone()
        {
            return DateFormatter.GetLocalTimeZone(this.Configuration);
        }

        private bool HasMessagingVerification(string hdid)
        {
            return this.MessagingVerifications.ToList().Exists(v => v.UserProfileId == hdid);
        }

        private void ResetPatientSupportState()
        {
            this.Dispatcher.Dispatch(new PatientSupportActions.ResetStateAction());
        }

        private void ResetMessageVerificationState()
        {
            this.Dispatcher.Dispatch(new MessageVerificationActions.ResetStateAction());
        }

        private void ToggleExpandRow(string hdid)
        {
            if (!this.HasMessagingVerification(hdid))
            {
                this.Dispatcher.Dispatch(new MessageVerificationActions.LoadAction(hdid));
            }

            this.Dispatcher.Dispatch(new PatientSupportActions.ToggleIsExpandedAction(hdid));
        }

        private async Task SearchAsync()
        {
            await this.Form.Validate().ConfigureAwait(true);
            if (this.Form.IsValid)
            {
                this.ResetPatientSupportState();
                this.ResetMessageVerificationState();
                this.Dispatcher.Dispatch(new PatientSupportActions.LoadAction(this.SelectedQueryType, StringManipulator.StripWhitespace(this.QueryParameter)));
            }
        }

        private sealed record PatientRow
        {
            public PatientRow(ExtendedPatientSupportDetails model, TimeZoneInfo timezone)
            {
                this.Hdid = model.Hdid;
                this.PersonalHealthNumber = model.PersonalHealthNumber;
                this.CreatedDateTime = model.ProfileCreatedDateTime != null ? TimeZoneInfo.ConvertTimeFromUtc((DateTime)model.ProfileCreatedDateTime, timezone) : model.ProfileCreatedDateTime;
                this.LastLoginDateTime = model.ProfileLastLoginDateTime != null ? TimeZoneInfo.ConvertTimeFromUtc((DateTime)model.ProfileLastLoginDateTime, timezone) : model.ProfileLastLoginDateTime;
                this.IsExpanded = model.IsExpanded;
                this.PhysicalAddress = model.PhysicalAddress;
                this.PostalAddress = model.PostalAddress;
            }

            public string Hdid { get; }

            public string PersonalHealthNumber { get; }

            public DateTime? CreatedDateTime { get; }

            public DateTime? LastLoginDateTime { get; }

            public bool IsExpanded { get; }

            public string PhysicalAddress { get; }

            public string PostalAddress { get; }

            public string PostalAddressLabel => !this.IsSameAddress || (this.PhysicalAddress.Length > 0 && this.PostalAddress.Length == 0) ? "Mailing Address" : "Address";

            public bool IsSameAddress => this.PhysicalAddress.Equals(this.PostalAddress, StringComparison.OrdinalIgnoreCase);

            public bool IsPhysicalAddressShown => this.PhysicalAddress.Length > 0 && !this.IsSameAddress;

            public bool IsPostalAddressShown => this.PostalAddress.Length > 0;

            public static string NoAddressMessage => "No address on record";
        }
    }
}
