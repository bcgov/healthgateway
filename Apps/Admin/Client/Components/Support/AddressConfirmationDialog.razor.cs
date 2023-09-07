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
using System.Linq;
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
/// If the Cancel button is pressed, the dialog's Result will have the Cancelled property set to true.
/// </summary>
/// <typeparam name="TAction">An action to dispatch when the confirmation button is pressed.</typeparam>
/// <typeparam name="TErrorAction">An action that indicates <typeparamref name="TAction"/> encountered an error.</typeparam>
/// <typeparam name="TSuccessAction">An action that indicates <typeparamref name="TAction"/> completed successfully.</typeparam>
public partial class AddressConfirmationDialog<TAction, TErrorAction, TSuccessAction> : FluxorComponent
    where TErrorAction : BaseFailAction
    where TAction : IAddressAction
{
    private string country = string.Empty;

    /// <summary>
    /// Gets or sets the default address to populate the dialog with.
    /// </summary>
    [Parameter]
    [EditorRequired]
    public Address DefaultAddress { get; set; } = default!;

    /// <summary>
    /// Gets or sets the action to dispatch when an address has been confirmed.
    /// </summary>
    [Parameter]
    [EditorRequired]
    public TAction ActionOnConfirm { get; set; } = default!;

    /// <summary>
    /// Gets or sets the action to dispatch when the dialog is cancelled.
    /// </summary>
    [Parameter]
    public object? ActionOnCancel { get; set; }

    /// <summary>
    /// Gets or sets the text to display on the dialog's confirmation button.
    /// </summary>
    [Parameter]
    public string ConfirmButtonLabel { get; set; } = "Confirm";

    [CascadingParameter]
    private MudDialogInstance MudDialog { get; set; } = default!;

    [Inject]
    private IActionSubscriber ActionSubscriber { get; set; } = default!;

    [Inject]
    private IDispatcher Dispatcher { get; set; } = default!;

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
        this.PopulateInputs(this.DefaultAddress);
    }

    /// <inheritdoc/>
    protected override void Dispose(bool disposing)
    {
        this.ActionSubscriber.UnsubscribeFromAllActions(this);
        base.Dispose(disposing);
    }

    private static Task<IEnumerable<string>> SearchCountriesAsync(string value)
    {
        return Task.FromResult(
            string.IsNullOrWhiteSpace(value)
                ? AddressUtility.CountriesWithAliases
                : AddressUtility.CountriesWithAliases.Where(c => c.ToUpperInvariant().StartsWith(value.ToUpperInvariant(), StringComparison.InvariantCulture)));
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

        this.Country = AddressUtility.GetCountryCode(address.Country) == CountryCode.Unknown ? string.Empty : address.Country;

        this.PostalCode = this.SelectedCountryCode == CountryCode.CA && address.PostalCode.Length == 6
            ? $"{address.PostalCode[..3]} {address.PostalCode[^3..]}"
            : address.PostalCode;
    }

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
            Country = this.SelectedCountryCode == CountryCode.CA
                ? string.Empty
                : AddressUtility.GetCountryName(this.SelectedCountryCode),
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

    private string? ValidatePostalCode(string postalCode)
    {
        if (string.IsNullOrWhiteSpace(postalCode))
        {
            return "Required";
        }

        switch (this.SelectedCountryCode)
        {
            case CountryCode.CA:
                return !AddressUtility.PostalCodeRegex().IsMatch(postalCode) ? "Incomplete postal code" : null;
            case CountryCode.US:
                return !AddressUtility.ZipCodeRegex().IsMatch(postalCode) ? "Incomplete zip code" : null;
            default:
                return null;
        }
    }

    private void HandleActionSuccess(TSuccessAction successAction)
    {
        this.IsLoading = false;
        this.MudDialog.Close(DialogResult.Ok(this.GetAddressModel()));
    }

    private void HandleActionFailed(BaseFailAction failAction)
    {
        this.IsLoading = false;
        this.Error = failAction.Error;
    }

    private void HandleClickCancel()
    {
        if (this.ActionOnCancel is not null)
        {
            this.Dispatcher.Dispatch(this.ActionOnCancel);
        }

        this.MudDialog.Cancel();
    }

    private async Task HandleClickConfirm()
    {
        await this.Form.Validate().ConfigureAwait(true);
        if (!this.Form.IsValid)
        {
            return;
        }

        this.IsLoading = true;
        this.ActionOnConfirm.Address = this.GetAddressModel();
        this.Dispatcher.Dispatch(this.ActionOnConfirm);
    }
}
