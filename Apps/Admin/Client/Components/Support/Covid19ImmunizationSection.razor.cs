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
    using HealthGateway.Admin.Client.Store.VaccineCard;
    using HealthGateway.Admin.Common.Models.CovidSupport;
    using HealthGateway.Common.Data.Models;
    using HealthGateway.Common.Data.Utils;
    using Microsoft.AspNetCore.Components;
    using Microsoft.Extensions.Configuration;
    using Microsoft.JSInterop;
    using MudBlazor;
    using MailVaccineCardAddressConfirmationDialog = AddressConfirmationDialog<
        Store.VaccineCard.VaccineCardActions.MailVaccineCardFailureAction,
        Store.VaccineCard.VaccineCardActions.MailVaccineCardSuccessAction>;

    /// <summary>
    /// The covid19 immunization section component.
    /// </summary>
    public partial class Covid19ImmunizationSection : FluxorComponent
    {
        /// <summary>
        /// Gets or sets the patient's PHN.
        /// </summary>
        [Parameter]
        [EditorRequired]
        public string Phn { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the data with which to populate the table.
        /// </summary>
        [Parameter]
        [EditorRequired]
        public IEnumerable<VaccineDose> Data { get; set; } = Enumerable.Empty<VaccineDose>();

        /// <summary>
        /// Gets or sets a value indicating whether data is loading.
        /// </summary>
        [Parameter]
        [EditorRequired]
        public bool IsLoading { get; set; } = true;

        /// <summary>
        /// Gets or sets the patient's address.
        /// </summary>
        [Parameter]
        [EditorRequired]
        public Address? MailAddress { get; set; }

        [Inject]
        private IActionSubscriber ActionSubscriber { get; set; } = default!;

        [Inject]
        private IConfiguration Configuration { get; set; } = default!;

        [Inject]
        private IDialogService Dialog { get; set; } = default!;

        [Inject]
        private IDispatcher Dispatcher { get; set; } = default!;

        [Inject]
        private IJSRuntime JsRuntime { get; set; } = default!;

        [Inject]
        private IState<VaccineCardState> VaccineCardState { get; set; } = default!;

        private bool MailVaccineCardIsLoading => this.VaccineCardState.Value.MailVaccineCard.IsLoading;

        private bool PrintVaccineCardIsLoading => this.VaccineCardState.Value.PrintVaccineCard.IsLoading;

        private IEnumerable<VaccineDoseRow> Rows => this.Data.Select(d => new VaccineDoseRow(d));

        private ReportModel VaccineCardStateData => this.VaccineCardState.Value.PrintVaccineCard.Result ?? default!;

        /// <inheritdoc/>
        protected override void OnInitialized()
        {
            base.OnInitialized();
            this.ResetVaccineCardState();
            this.ActionSubscriber.SubscribeToAction<VaccineCardActions.PrintVaccineCardSuccessAction>(this, this.PrintVaccineCard);
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            this.ActionSubscriber.UnsubscribeFromAllActions(this);
            base.Dispose(disposing);
        }

        private async Task DownloadReport(ReportModel report)
        {
            await this.JsRuntime.InvokeAsync<object>("saveAsFile", report.FileName, report.Data).ConfigureAwait(true);
        }

        private async Task OpenMailVaccineCardAddressConfirmationDialog()
        {
            const string title = "Confirm Address";
            DialogParameters parameters = new()
            {
                [nameof(MailVaccineCardAddressConfirmationDialog.ActionOnConfirm)] = (Action<Address>)this.MailVaccineCard,
                [nameof(MailVaccineCardAddressConfirmationDialog.DefaultAddress)] = this.MailAddress,
                [nameof(MailVaccineCardAddressConfirmationDialog.ConfirmButtonLabel)] = "Send",
            };
            DialogOptions options = new()
            {
                DisableBackdropClick = true,
                FullWidth = true,
                MaxWidth = MaxWidth.Small,
            };
            IDialogReference dialog = await this.Dialog
                .ShowAsync<MailVaccineCardAddressConfirmationDialog>(
                    title,
                    parameters,
                    options)
                .ConfigureAwait(true);
            await dialog.Result.ConfigureAwait(true);
        }

        private DateTime? ConvertDateTime(DateTime? utcDateTime)
        {
            if (utcDateTime != null)
            {
                return TimeZoneInfo.ConvertTimeFromUtc((DateTime)utcDateTime, this.GetTimeZone());
            }

            return null;
        }

        private void PrintVaccineCard(VaccineCardActions.PrintVaccineCardSuccessAction action)
        {
            Task.Run(async () => await this.DownloadReport(this.VaccineCardStateData).ConfigureAwait(true));
        }

        private void ResetVaccineCardState()
        {
            this.Dispatcher.Dispatch(new VaccineCardActions.ResetStateAction());
        }

        private TimeZoneInfo GetTimeZone()
        {
            return DateFormatter.GetLocalTimeZone(this.Configuration);
        }

        private void MailVaccineCard(Address address)
        {
            this.Dispatcher.Dispatch(new VaccineCardActions.MailVaccineCardAction(this.Phn, address));
        }

        private void Print()
        {
            this.Dispatcher.Dispatch(new VaccineCardActions.PrintVaccineCardAction(this.Phn));
        }

        private sealed record VaccineDoseRow
        {
            public VaccineDoseRow(VaccineDose model)
            {
                this.Product = model.Product;
                this.Lot = model.Lot;
                this.Date = model.Date;
                this.Location = model.Location;
            }

            public DateTime? Date { get; }

            public string? Product { get; }

            public string? Lot { get; }

            public string? Location { get; }
        }
    }
}
