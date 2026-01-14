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
    using System.Collections.Immutable;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Threading.Tasks;
    using Fluxor;
    using Fluxor.Blazor.Web.Components;
    using HealthGateway.Admin.Client.Authorization;
    using HealthGateway.Admin.Client.Store.PatientSupport;
    using HealthGateway.Admin.Client.Utils;
    using HealthGateway.Admin.Common.Constants;
    using HealthGateway.Admin.Common.Models;
    using HealthGateway.Common.Data.Constants;
    using HealthGateway.Common.Data.Extensions;
    using HealthGateway.Common.Data.Utils;
    using HealthGateway.Common.Data.Validations;
    using Microsoft.AspNetCore.Components;
    using Microsoft.AspNetCore.Components.Authorization;
    using Microsoft.AspNetCore.WebUtilities;
    using Microsoft.Extensions.Primitives;
    using Microsoft.JSInterop;
    using MudBlazor;
    using MudBlazor.Services;

    /// <summary>
    /// Backing logic for the Support page.
    /// </summary>
    public partial class SupportPage : FluxorComponent
    {
        private static readonly PhnValidator PhnValidator = new();

        [Inject]
        private IDispatcher Dispatcher { get; set; } = default!;

        [Inject]
        private IActionSubscriber ActionSubscriber { get; set; } = default!;

        [Inject]
        private IState<PatientSupportState> PatientSupportState { get; set; } = default!;

        [Inject]
        private NavigationManager NavigationManager { get; set; } = default!;

        [Inject]
        private AuthenticationStateProvider AuthenticationStateProvider { get; set; } = default!;

        [Inject]
        private IKeyInterceptorService KeyInterceptorService { get; set; } = default!;

        [Inject]
        private IJSRuntime JsRuntime { get; set; } = default!;

        private List<PatientQueryType> QueryTypes => (this.AuthenticationState?.User).IsInAnyRole(Roles.Admin, Roles.Reviewer)
            ? [PatientQueryType.Phn, PatientQueryType.Email, PatientQueryType.Sms, PatientQueryType.Hdid, PatientQueryType.Dependent]
            : [PatientQueryType.Phn];

        private PatientQueryType QueryType { get; set; } = PatientQueryType.Phn;

        private PatientQueryType SelectedQueryType
        {
            get => this.QueryType;

            set
            {
                if (!this.IsPreviousPagePatientDetails())
                {
                    this.Dispatcher.Dispatch(new PatientSupportActions.ResetStateAction());
                }

                this.QueryParameter = string.Empty;
                this.QueryType = value;
            }
        }

        private string QueryParameter { get; set; } = string.Empty;

        private MudForm Form { get; set; } = default!;

        private bool PhnOrDependentSelected => this.SelectedQueryType is PatientQueryType.Phn or PatientQueryType.Dependent;

        private bool PatientsLoading => this.PatientSupportState.Value.IsLoading;

        private bool PatientsLoaded => this.PatientSupportState.Value.Loaded;

        private bool HasPatientsError => this.PatientSupportState.Value.Error is { Message.Length: > 0 };

        private bool HasPatientsWarning => this.PatientSupportState.Value.WarningMessages.Any();

        private IImmutableList<PatientSupportResult> Patients => this.PatientSupportState.Value.Result ?? [];

        private IEnumerable<PatientRow> PatientRows => this.Patients.Select(v => new PatientRow(v));

        private Func<string, string?> ValidateQueryParameter => parameter =>
        {
            string? result = ValidateParameterInput(parameter) ?? ValidatePhn(parameter, this.PhnOrDependentSelected);
            return result ?? ValidateQueryType(parameter, this.SelectedQueryType);
        };

        private AuthenticationState? AuthenticationState { get; set; }

        /// <inheritdoc/>
        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();
            this.ResetPatientSupportState();
            await this.RepopulateQueryAndResultsAsync();
            this.AuthenticationState = await this.AuthenticationStateProvider.GetAuthenticationStateAsync();
            this.ActionSubscriber.SubscribeToAction<PatientSupportActions.LoadSuccessAction>(this, this.CheckForSingleResult);
        }

        /// <inheritdoc/>
        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                await this.KeyInterceptorService.SubscribeAsync(
                    "query-controls",
                    new("query-input", new KeyOptions("Enter", true)),
                    _ => this.SearchAsync());
            }

            await base.OnAfterRenderAsync(firstRender);
        }

        /// <inheritdoc/>
        protected override async ValueTask DisposeAsyncCore(bool disposing)
        {
            if (disposing)
            {
                await this.KeyInterceptorService.UnsubscribeAsync("query-controls");
            }

            await base.DisposeAsyncCore(disposing);
        }

        private static string? ValidateParameterInput(string parameter)
        {
            return string.IsNullOrWhiteSpace(parameter) ? "Search parameter is required" : null;
        }

        private static string? ValidatePhn(string parameter, bool phnOrDependentSelected)
        {
            return phnOrDependentSelected && !PhnValidator.Validate(StringManipulator.StripWhitespace(parameter)).IsValid
                ? "Invalid PHN"
                : null;
        }

        [SuppressMessage("Style", "IDE0072:Populate switch", Justification = "Team decision")]
        private static string? ValidateQueryType(string parameter, PatientQueryType selectedQueryType)
        {
            return selectedQueryType switch
            {
                PatientQueryType.Email or PatientQueryType.Sms when StringManipulator.StripWhitespace(parameter).Length < 5
                    => "Email/SMS must be minimum 5 characters",
                PatientQueryType.Sms when !StringManipulator.IsPositiveNumeric(parameter)
                    => "SMS must contain digits only",
                _ => null,
            };
        }

        private void Clear()
        {
            this.Dispatcher.Dispatch(new PatientSupportActions.ResetStateAction());
            this.QueryParameter = string.Empty;
        }

        private bool IsPreviousPagePatientDetails()
        {
            Uri uri = this.NavigationManager.ToAbsoluteUri(this.NavigationManager.Uri);
            return QueryHelpers.ParseQuery(uri.Query).TryGetValue("details", out StringValues _);
        }

        private void ResetPatientSupportState()
        {
            this.Dispatcher.Dispatch(new PatientSupportActions.ResetStateAction());
        }

        private async Task RepopulateQueryAndResultsAsync()
        {
            if (this.IsPreviousPagePatientDetails())
            {
                string queryTypeString = await SessionUtility.GetSessionStorageItem(this.JsRuntime, SessionUtility.SupportQueryType);
                string queryString = await SessionUtility.GetSessionStorageItem(this.JsRuntime, SessionUtility.SupportQueryString);

                if (Enum.TryParse(queryTypeString, out PatientQueryType queryType))
                {
                    this.SelectedQueryType = queryType;
                }

                this.QueryParameter = queryString;
                await StoreUtility.LoadPatientSupportAction(this.Dispatcher, this.JsRuntime, this.SelectedQueryType, this.QueryParameter, false);
            }
        }

        private async Task SearchAsync()
        {
            await this.Form.Validate();
            if (this.Form.IsValid)
            {
                this.ResetPatientSupportState();
                await StoreUtility.LoadPatientSupportAction(this.Dispatcher, this.JsRuntime, this.SelectedQueryType, StringManipulator.StripWhitespace(this.QueryParameter));
            }
        }

        private void CheckForSingleResult(PatientSupportActions.LoadSuccessAction action)
        {
            if (action.Data.Count == 1 &&
                action.ShouldNavigateToPatientDetails &&
                !string.IsNullOrEmpty(action.Data.Single().PersonalHealthNumber))
            {
                this.NavigateToPatientDetails(action.Data.Single().PersonalHealthNumber);
            }
        }

        private void NavigateToPatientDetails(string phn)
        {
            this.NavigationManager.NavigateTo($"patient-details?phn={phn}");
        }

        private void RowClickEvent(TableRowClickEventArgs<PatientRow> tableRowClickEventArgs)
        {
            if (!string.IsNullOrEmpty(tableRowClickEventArgs.Item?.PersonalHealthNumber))
            {
                this.NavigateToPatientDetails(tableRowClickEventArgs.Item.PersonalHealthNumber);
            }
        }

        private sealed record PatientRow
        {
            public PatientRow(PatientSupportResult model)
            {
                this.Status = model.Status;
                this.Name = StringManipulator.JoinWithoutBlanks([model.PreferredName?.GivenName, model.PreferredName?.Surname]);
                this.SortName = $"{model.PreferredName?.Surname}, {model.PreferredName?.GivenName}";
                this.Birthdate = model.Birthdate;
                this.PersonalHealthNumber = model.PersonalHealthNumber;
                this.Hdid = model.Hdid;
            }

            public PatientStatus Status { get; }

            public string Name { get; }

            public string SortName { get; }

            public DateOnly? Birthdate { get; }

            public string PersonalHealthNumber { get; }

            public string Hdid { get; }
        }
    }
}
