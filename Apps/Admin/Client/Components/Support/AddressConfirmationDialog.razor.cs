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
namespace HealthGateway.Admin.Client.Components.Support;

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Fluxor;
using Fluxor.Blazor.Web.Components;
using HealthGateway.Admin.Client.Store;
using HealthGateway.Common.Data.Constants;
using HealthGateway.Common.Data.Models;
using HealthGateway.Common.Data.Utils;
using HealthGateway.Common.Ui.Constants;
using Microsoft.AspNetCore.Components;
using MudBlazor;

/// <summary>
/// Backing logic for the AddressConfirmationDialog component.
/// If the Confirmation button is pressed, the dialog's Result will have the Data property populated with an Address.
/// If the Cancel button is pressed, the dialog's Result will have the Canceled property set to true.
/// </summary>
/// <typeparam name="TErrorAction">
/// An action that indicates an error was encountered while performing the action on
/// confirmation.
/// </typeparam>
/// <typeparam name="TSuccessAction">An action that indicates the action on confirmation completed successfully.</typeparam>
public partial class AddressConfirmationDialog<TErrorAction, TSuccessAction> : FluxorComponent
    where TErrorAction : BaseFailureAction
{
    private string country = string.Empty;

    /// <summary>
    /// Gets or sets the default address to populate the dialog with.
    /// </summary>
    [Parameter]
    public Address? DefaultAddress { get; set; }

    /// <summary>
    /// Gets or sets an Action to execute when an address has been confirmed.
    /// </summary>
    [Parameter]
    [EditorRequired]
    public Action<Address> ActionOnConfirm { get; set; } = _ => { };

    /// <summary>
    /// Gets or sets the text to display on the dialog's confirmation button.
    /// </summary>
    [Parameter]
    public string ConfirmButtonLabel { get; set; } = "Confirm";

    /// <summary>
    /// Gets or sets a value indicating whether the country property in the resulting address should contain a country code
    /// instead of a country name.
    /// </summary>
    [Parameter]
    public bool OutputCountryCodeFormat { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the country property in the resulting address should be set to the empty string
    /// when Canada is selected.
    /// </summary>
    [Parameter]
    public bool OutputCanadaAsEmptyString { get; set; }

    [CascadingParameter]
    private IMudDialogInstance MudDialog { get; set; } = default!;

    [Inject]
    private IActionSubscriber ActionSubscriber { get; set; } = default!;

    private bool IsLoading { get; set; }

    private RequestError? Error { get; set; }

    private MudForm Form { get; set; } = default!;

    private string AddressLines { get; set; } = string.Empty;

    private string City { get; set; } = string.Empty;

    private string CanadianProvince { get; set; } = string.Empty;

    private ProvinceAbbreviation CanadianProvinceAbbreviation => EnumUtility.ToEnumOrDefault<ProvinceAbbreviation>(this.CanadianProvince, true);

    private string AmericanState { get; set; } = string.Empty;

    private StateAbbreviation AmericanStateAbbreviation => EnumUtility.ToEnumOrDefault<StateAbbreviation>(this.AmericanState, true);

    private string OtherState { get; set; } = string.Empty;

    private string PostalCode { get; set; } = string.Empty;

    private IMask? PostalCodeMask { get; set; }

    [SuppressMessage("Style", "IDE0072:Populate switch", Justification = "Team decision")]
    private string Country
    {
        get => this.country;

        set
        {
            this.country = value;
            this.PostalCodeMask = this.SelectedCountryCode switch
            {
                CountryCode.CA => Mask.PostalCodeMask,
                CountryCode.US => Mask.ZipCodeMask,
                _ => null,
            };
        }
    }

    private CountryCode SelectedCountryCode
    {
        get
        {
            CountryCode countryCode = AddressUtility.GetCountryCode(this.Country);
            return countryCode == CountryCode.Unknown ? CountryCode.CA : countryCode;
        }
    }

    private string? ErrorMessage => this.Error?.Message;

    private bool HasError => this.Error is not null;

    /// <inheritdoc/>
    protected override void OnInitialized()
    {
        base.OnInitialized();
        this.ActionSubscriber.SubscribeToAction<TErrorAction>(this, this.HandleActionFailed);
        this.ActionSubscriber.SubscribeToAction<TSuccessAction>(this, this.HandleActionSuccess);
        if (this.DefaultAddress != null)
        {
            this.PopulateInputs(this.DefaultAddress);
        }
    }

    /// <inheritdoc/>
    protected override async ValueTask DisposeAsyncCore(bool disposing)
    {
        if (disposing)
        {
            this.ActionSubscriber.UnsubscribeFromAllActions(this);
        }

        await base.DisposeAsyncCore(disposing);
    }

    [SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "CancellationToken required for MudAutocomplete.SearchFunc")]
    private static Task<IEnumerable<string>> SearchCountriesAsync(string value, CancellationToken ct)
    {
        return Task.FromResult(
            string.IsNullOrWhiteSpace(value)
                ? AddressUtility.CountriesWithAliases
                : AddressUtility.CountriesWithAliases.Where(c => c.ToUpperInvariant().StartsWith(value.ToUpperInvariant(), StringComparison.InvariantCulture)));
    }

    private static string GetCountryNameFromInput(string input)
    {
        if (AddressUtility.CountryCodes.Any(c => c.ToString() == input))
        {
            // country code
            return EnumUtility.ToEnumString(EnumUtility.ToEnum<CountryCode>(input), true);
        }

        if (AddressUtility.GetCountryCode(input) != CountryCode.Unknown)
        {
            // recognized country name
            return input;
        }

        // unrecognized country
        return string.Empty;
    }

    private static string? ValidateCanadianPostalCode(string postalCode)
    {
        return !AddressUtility.PostalCodeRegex().IsMatch(postalCode) ? "Incomplete postal code" : null;
    }

    private static string? ValidateUsPostalCode(string postalCode)
    {
        return !AddressUtility.ZipCodeRegex().IsMatch(postalCode) ? "Incomplete zip code" : null;
    }

    private void PopulateInputs(Address address)
    {
        this.AddressLines = string.Join(Environment.NewLine, address.StreetLines);

        this.City = address.City;

        ProvinceAbbreviation provinceAbbreviation = EnumUtility.ToEnumOrDefault<ProvinceAbbreviation>(address.State);
        this.CanadianProvince = provinceAbbreviation == ProvinceAbbreviation.Unknown ? string.Empty : EnumUtility.ToEnumString(provinceAbbreviation, true);

        StateAbbreviation stateAbbreviation = EnumUtility.ToEnumOrDefault<StateAbbreviation>(address.State);
        this.AmericanState = stateAbbreviation == StateAbbreviation.Unknown ? string.Empty : EnumUtility.ToEnumString(stateAbbreviation, true);

        this.OtherState = this.CanadianProvince.Length > 0 || this.AmericanState.Length > 0 ? string.Empty : address.State;

        this.Country = GetCountryNameFromInput(address.Country);

        this.PostalCode = this.SelectedCountryCode == CountryCode.CA && address.PostalCode.Length == 6
            ? $"{address.PostalCode[..3]} {address.PostalCode[^3..]}"
            : address.PostalCode;
    }

    private string GetCountryToOutput()
    {
        string? canadaOutput = this.SelectedCountryCode == CountryCode.CA && this.OutputCanadaAsEmptyString
            ? string.Empty
            : null;

        return canadaOutput ?? (this.OutputCountryCodeFormat
            ? this.SelectedCountryCode.ToString()
            : AddressUtility.GetCountryName(this.SelectedCountryCode));
    }

    [SuppressMessage("Style", "IDE0072:Populate switch", Justification = "Team decision")]
    private Address GetAddressModel()
    {
        return new()
        {
            StreetLines = this.AddressLines.Split(Environment.NewLine),
            City = this.City,
            State = this.SelectedCountryCode switch
            {
                CountryCode.CA => this.CanadianProvinceAbbreviation.ToString(),
                CountryCode.US => this.AmericanStateAbbreviation.ToString(),
                _ => this.OtherState,
            },
            PostalCode = this.PostalCode,
            Country = this.GetCountryToOutput(),
        };
    }

    private void HandleCountryChanged(string value)
    {
        this.Country = value;
        this.PostalCode = string.Empty;
        this.CanadianProvince = string.Empty;
        this.AmericanState = string.Empty;
        this.OtherState = string.Empty;
    }

    [SuppressMessage("Style", "IDE0072:Populate switch", Justification = "Team decision")]
    private string? ValidatePostalCode(string postalCode)
    {
        return string.IsNullOrWhiteSpace(postalCode)
            ? "Required"
            : this.SelectedCountryCode switch
            {
                CountryCode.CA => ValidateCanadianPostalCode(postalCode),
                CountryCode.US => ValidateUsPostalCode(postalCode),
                _ => null,
            };
    }

    private void HandleActionSuccess(TSuccessAction successAction)
    {
        this.IsLoading = false;
        this.MudDialog.Close(DialogResult.Ok(this.GetAddressModel()));
    }

    private void HandleActionFailed(BaseFailureAction failureAction)
    {
        this.IsLoading = false;
        this.Error = failureAction.Error;
        this.StateHasChanged();
    }

    private void HandleClickCancel()
    {
        this.MudDialog.Cancel();
    }

    private async Task HandleClickConfirmAsync()
    {
        await this.Form.Validate();
        if (!this.Form.IsValid)
        {
            return;
        }

        this.IsLoading = true;
        this.ActionOnConfirm(this.GetAddressModel());
    }
}
