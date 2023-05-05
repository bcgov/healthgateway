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
    using Fluxor;
    using Fluxor.Blazor.Web.Components;
    using HealthGateway.Admin.Client.Store.MessageVerification;
    using HealthGateway.Admin.Client.Store.PatientSupport;
    using HealthGateway.Admin.Common.Constants;
    using HealthGateway.Admin.Common.Models;
    using HealthGateway.Common.Data.Constants;
    using HealthGateway.Common.Data.Utils;
    using HealthGateway.Common.Data.ViewModels;
    using Microsoft.AspNetCore.Components;
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
        private IState<MessageVerificationState> MessageVerificationState { get; set; } = default!;

        [Inject]
        private IState<PatientSupportState> PatientSupportState { get; set; } = default!;

        [Inject]
        private NavigationManager NavigationManager { get; set; } = default!;

        [Inject]
        private IActionSubscriber ActionSubscriber { get; set; } = default!;

        private bool MessagingVerificationsLoading => this.MessageVerificationState.Value.IsLoading;

        private bool HasMessagingVerificationsError => this.MessageVerificationState.Value.Error is { Message.Length: > 0 };

        private IEnumerable<MessagingVerificationModel> MessagingVerifications => this.MessageVerificationState.Value.Data ?? Enumerable.Empty<MessagingVerificationModel>();

        private bool PatientsLoaded => this.PatientSupportState.Value.Loaded;

        private bool HasPatientsError => this.PatientSupportState.Value.Error is { Message.Length: > 0 };

        private bool HasPatientsWarning => this.PatientSupportState.Value.WarningMessages.Any();

        private PatientSupportDetails? Patient =>
            this.PatientSupportState.Value.Result?.SingleOrDefault(x => x.Hdid == this.Hdid);

        private string PatientName => StringManipulator.JoinWithoutBlanks(new[] { this.Patient?.PreferredName?.GivenName, this.Patient?.PreferredName?.Surname });

        private string? StatusWarning => this.Patient == null ? null : MapStatusToWarning(this.Patient.Status);

        private string? Hdid { get; set; }

        /// <inheritdoc/>
        protected override void OnInitialized()
        {
            base.OnInitialized();

            Uri uri = this.NavigationManager.ToAbsoluteUri(this.NavigationManager.Uri);
            if (QueryHelpers.ParseQuery(uri.Query).TryGetValue("hdid", out StringValues hdid))
            {
                this.Hdid = hdid;
                this.RetrievePatientData();
                this.ActionSubscriber.SubscribeToAction<PatientSupportActions.LoadSuccessAction>(this, _ => this.RetrieveMessagingVerifications());
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

        private void RetrievePatientData()
        {
            if (this.Patient == null)
            {
                this.Dispatcher.Dispatch(new PatientSupportActions.ResetStateAction());
                this.Dispatcher.Dispatch(new MessageVerificationActions.ResetStateAction());
                this.Dispatcher.Dispatch(new PatientSupportActions.LoadAction(PatientQueryType.Hdid, this.Hdid));
            }
            else
            {
                this.RetrieveMessagingVerifications();
            }
        }

        private void RetrieveMessagingVerifications()
        {
            this.Dispatcher.Dispatch(new MessageVerificationActions.ResetStateAction());
            if (this.Patient?.ProfileCreatedDateTime != null)
            {
                this.Dispatcher.Dispatch(new MessageVerificationActions.LoadAction(this.Hdid));
            }
        }
    }
}
