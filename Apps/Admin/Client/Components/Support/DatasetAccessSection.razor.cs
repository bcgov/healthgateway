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
namespace HealthGateway.Admin.Client.Components.Support
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Fluxor;
    using Fluxor.Blazor.Web.Components;
    using HealthGateway.Admin.Client.Components.AgentAudit;
    using HealthGateway.Admin.Client.Store.PatientDetails;
    using HealthGateway.Common.Data.Constants;
    using Microsoft.AspNetCore.Components;
    using MudBlazor;

    /// <summary>
    /// Backing logic for support's dataset access section.
    /// </summary>
    public partial class DatasetAccessSection : FluxorComponent
    {
        private readonly IEnumerable<DataSource> dataSources = Enum.GetValues<DataSource>().Where(x => x != DataSource.Unknown);

        /// <summary>
        /// Working copy of the data sources which are currently set as blocked for the patient.
        /// </summary>
        private IEnumerable<DataSource> blockedDataSources = Enumerable.Empty<DataSource>();

        /// <summary>
        /// Gets or sets the data sources which are currently set as blocked for the patient.
        /// </summary>
        [Parameter]
        [EditorRequired]
        public IEnumerable<DataSource> Data { get; set; } = Enumerable.Empty<DataSource>();

        /// <summary>
        /// Gets or sets a value indicating whether the component is loading.
        /// </summary>
        [Parameter]
        [EditorRequired]
        public bool IsLoading { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether the component is editable.
        /// </summary>
        [Parameter]
        [EditorRequired]
        public bool CanEdit { get; set; } = false;

        /// <summary>
        /// Gets or sets the patient's hdid.
        /// </summary>
        [Parameter]
        [EditorRequired]
        public string Hdid { get; set; } = string.Empty;

        [Inject]
        private IDispatcher Dispatcher { get; set; } = default!;

        [Inject]
        private IState<PatientDetailsState> PatientDetailsState { get; set; } = default!;

        [Inject]
        private IActionSubscriber ActionSubscriber { get; set; } = default!;

        [Inject]
        private IDialogService Dialog { get; set; } = default!;

        [Inject]
        private ISnackbar Snackbar { get; set; } = default!;

        private bool IsDirty => !this.Data.OrderBy(x => x).SequenceEqual(this.blockedDataSources.OrderBy(x => x));

        /// <summary>
        /// Overrides the base OnParametersSet method.
        /// This allows us to ensure that the incoming data is set to the blockedDataSources.
        /// </summary>
        protected override void OnParametersSet()
        {
            // Subscribing to the PatientDetailsActions.LoadSuccessAction resulted in missing the initial binding.
            this.SetBlockedDataSources();
        }

        private static string MapDataSourceToFriendlyText(DataSource dataSource)
        {
            return dataSource switch
            {
                DataSource.ClinicalDocument => "Clinical Documents",
                DataSource.Immunization => "Immunizations",
                DataSource.LabResult => "Laboratory Results",
                DataSource.DiagnosticImaging => "Diagnostic Imaging",
                DataSource.Medication => "Medications",
                DataSource.Note => "Notes",
                DataSource.HealthVisit => "Health Visits",
                DataSource.HospitalVisit => "Hospital Visits",
                DataSource.Covid19TestResult => "COVID-19 Test Results",
                DataSource.OrganDonorRegistration => "Organ Donor Registration",
                DataSource.SpecialAuthorityRequest => "Special Authority Requests",
                DataSource.CancerScreening => "Cancer Screening",
                _ => dataSource.ToString(),
            };
        }

        private void SetBlockedDataSources()
        {
            this.blockedDataSources = this.Data.ToList();
        }

        private void CancelChanges()
        {
            this.SetBlockedDataSources();
        }

        private async Task SaveChanges()
        {
            const string title = "Confirm Update";
            DialogOptions options = new()
            {
                DisableBackdropClick = true,
                FullWidth = true,
                MaxWidth = MaxWidth.ExtraSmall,
            };
            DialogParameters parameters = new()
            {
                ["AuditableAction"] = new PatientDetailsActions.BlockAccessAction(this.Hdid, this.blockedDataSources),
            };

            IDialogReference dialog = await this.Dialog
                .ShowAsync<AuditReasonDialog<
                    PatientDetailsActions.BlockAccessAction,
                    PatientDetailsActions.BlockAccessFailureAction,
                    PatientDetailsActions.BlockAccessSuccessAction>>(
                    title,
                    parameters,
                    options)
                .ConfigureAwait(true);

            DialogResult result = await dialog.Result.ConfigureAwait(true);
            if (!result.Canceled)
            {
                this.Snackbar.Add("Patient's dataset access has been updated.", Severity.Success);
            }
        }

        private bool IsBlocked(DataSource dataSource)
        {
            return this.blockedDataSources.Contains(dataSource);
        }

        private void ToggleDataSource(DataSource dataSource)
        {
            this.blockedDataSources = this.IsBlocked(dataSource)
                ? this.blockedDataSources.Where(x => x != dataSource)
                : this.blockedDataSources.Append(dataSource);
        }
    }
}
