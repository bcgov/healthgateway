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
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Fluxor;
    using Fluxor.Blazor.Web.Components;
    using HealthGateway.Admin.Client.Services;
    using HealthGateway.Admin.Client.Store.Configuration;
    using HealthGateway.Admin.Client.Store.HealthData;
    using HealthGateway.Admin.Client.Store.PatientDetails;
    using HealthGateway.Admin.Client.Store.PatientSupport;
    using HealthGateway.Admin.Client.Utils;
    using HealthGateway.Admin.Common.Constants;
    using HealthGateway.Admin.Common.Models;
    using HealthGateway.Common.Data.Models;
    using Microsoft.AspNetCore.Components;
    using MudBlazor;

    /// <summary>
    /// Backing logic for the AccountTab component.
    /// </summary>
    public partial class AccountTab : FluxorComponent
    {
        /// <summary>
        /// Gets or sets the patient's personal health number.
        /// </summary>
        [Parameter]
        public string Phn { get; set; } = string.Empty;

        [Inject]
        private IState<PatientSupportState> PatientSupportState { get; set; } = null!;

        [Inject]
        private IState<PatientDetailsState> PatientDetailsState { get; set; } = null!;

        [Inject]
        private IDateConversionService DateConversionService { get; set; } = null!;

        [Inject]
        private IState<ConfigurationState> ConfigurationState { get; set; } = null!;

        [Inject]
        private IState<HealthDataState> HealthDataState { get; set; } = null!;

        [Inject]
        private IActionSubscriber ActionSubscriber { get; set; } = null!;

        [Inject]
        private IDispatcher Dispatcher { get; set; } = null!;

        [Inject]
        private ISnackbar Snackbar { get; set; } = default!;

        private PatientSupportResult? Patient =>
            this.PatientSupportState.Value.Result?.SingleOrDefault(x => x.PersonalHealthNumber == this.Phn);

        private DateTime? ProfileCreatedDateTime =>
            this.Patient?.ProfileCreatedDateTime == null ? null : this.DateConversionService.ConvertFromUtc(this.Patient.ProfileCreatedDateTime.Value);

        private DateTime? ProfileLastLoginDateTime =>
            this.Patient?.ProfileLastLoginDateTime == null ? null : this.DateConversionService.ConvertFromUtc(this.Patient.ProfileLastLoginDateTime.Value);

        private IEnumerable<MessagingVerificationModel> MessagingVerifications => this.PatientDetailsState.Value.MessagingVerifications ?? [];

        private bool PatientSupportDetailsLoading => this.PatientDetailsState.Value.IsLoading;

        private bool RefreshImagingCacheIsLoading => this.HealthDataState.Value.RefreshImagingCache.IsLoading;

        private bool RefreshLabCacheIsLoading => this.HealthDataState.Value.RefreshLabsCache.IsLoading;

        private bool IsDefaultPatientStatus => this.Patient?.Status == PatientStatus.Default;

        private bool? IsAccountRegistered => this.PatientDetailsState.Value.IsAccountRegistered;

        private Dictionary<string, bool>? Features => this.ConfigurationState.Value.Result?.Features;

        private bool ShowApiRegistration => FeatureToggleUtility.IsEnabled(this.Features, "ShowApiRegistration");

        private bool ShowImagingRefresh => FeatureToggleUtility.IsEnabled(this.Features, "ShowImagingRefresh");

        private bool ShowLabsRefresh => FeatureToggleUtility.IsEnabled(this.Features, "ShowLabsRefresh");

        private DateOnly? LastImagingRefreshDate => this.PatientDetailsState.Value.LastImagingRefreshDate;

        private DateOnly? LastLabsRefreshDate => this.PatientDetailsState.Value.LastLabsRefreshDate;

        /// <inheritdoc/>
        protected override void OnInitialized()
        {
            base.OnInitialized();
            this.ResetHealthDataState();
            this.ActionSubscriber.SubscribeToAction<HealthDataActions.RefreshImagingCacheSuccessAction>(this, this.DisplayRefreshImagingCacheSuccessful);
            this.ActionSubscriber.SubscribeToAction<HealthDataActions.RefreshLabsCacheSuccessAction>(this, this.DisplayRefreshLabsCacheSuccessful);
        }

        private void ResetHealthDataState()
        {
            this.Dispatcher.Dispatch(new HealthDataActions.ResetStateAction());
        }

        private void RefreshImagingCache()
        {
            this.Dispatcher.Dispatch(new HealthDataActions.RefreshImagingCacheAction { Phn = this.Phn });
        }

        private void RefreshLabsCache()
        {
            this.Dispatcher.Dispatch(new HealthDataActions.RefreshLabsCacheAction { Phn = this.Phn });
        }

        private void DisplayRefreshImagingCacheSuccessful(HealthDataActions.RefreshImagingCacheSuccessAction action)
        {
            this.Snackbar.Add("Imaging data refresh submitted successfully.", Severity.Success);

            this.Dispatcher.Dispatch(
                new PatientDetailsActions.LoadAction
                {
                    QueryType = ClientRegistryType.Phn,
                    QueryString = this.Phn,
                    RefreshVaccineDetails = false,
                });
        }

        private void DisplayRefreshLabsCacheSuccessful(HealthDataActions.RefreshLabsCacheSuccessAction action)
        {
            this.Snackbar.Add("Labs data refresh submitted successfully.", Severity.Success);

            this.Dispatcher.Dispatch(
                new PatientDetailsActions.LoadAction
                {
                    QueryType = ClientRegistryType.Phn,
                    QueryString = this.Phn,
                    RefreshVaccineDetails = false,
                });
        }
    }
}
