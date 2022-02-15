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
namespace HealthGateway.Admin.Server.Models.CovidSupport;

using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using HealthGateway.Admin.Server.Constants;

/// <summary>
/// Model object representing covid therapy assessment submission request.
/// </summary>
public class CovidAssessmentRequest
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CovidAssessmentRequest"/> class.
    /// </summary>
    public CovidAssessmentRequest()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="CovidAssessmentRequest"/> class.
    /// </summary>
    /// <param name="streetAddresses">The list of address street lines.</param>
    [JsonConstructor]
    public CovidAssessmentRequest(
        IList<string> streetAddresses)
    {
        this.StreetAddresses = streetAddresses;
    }

    /// <summary>
    /// Gets or sets the phn used.
    /// </summary>
    [JsonPropertyName("phn")]
    public string? Phn { get; set; }

    /// <summary>
    /// Gets or sets the first name.
    /// </summary>
    [JsonPropertyName("firstName")]
    public string? FirstName { get; set; }

    /// <summary>
    /// Gets or sets the last name.
    /// </summary>
    [JsonPropertyName("lastName")]
    public string? LastName { get; set; }

    /// <summary>
    /// Gets or sets the phone number used.
    /// </summary>
    [JsonPropertyName("phoneNumber")]
    public string? PhoneNumber { get; set; }

    /// <summary>
    /// Gets or sets the identifies indigenous option.
    /// </summary>
    [JsonPropertyName("identifiesIndigenous")]
    public CovidTherapyAssessmentOption IdentifiesIndigenous { get; set; }

    /// <summary>
    /// Gets or sets the identifies has a family doctor or np option.
    /// </summary>
    [JsonPropertyName("hasAFamilyDoctorOrNp")]
    public CovidTherapyAssessmentOption HasAFamilyDoctorOrNp { get; set; }

    /// <summary>
    /// Gets or sets the identifies the confirms over 12 option.
    /// </summary>
    [JsonPropertyName("confirmsOver12")]
    public CovidTherapyAssessmentOption ConfirmsOver12 { get; set; }

    /// <summary>
    /// Gets or sets the has severe covid 19 symptoms option.
    /// </summary>
    [JsonPropertyName("hasSevereCovid19Symptoms")]
    public CovidTherapyAssessmentOption HasSevereCovid19Symptoms { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether there has been mild or moderate covid 19 symptoms.
    /// </summary>
    [JsonPropertyName("hasMildOrModerateCovid19Symptoms")]
    public bool HasMildOrModerateCovid19Symptoms { get; set; }

    /// <summary>
    /// Gets or sets the symptom on set date.
    /// </summary>
    [JsonPropertyName("symptomOnSetDate")]
    public DateTime SymptomOnSetDate { get; set; }

    /// <summary>
    /// Gets or sets the has immunity compromising medical condition antiviral tri option.
    /// </summary>
    [JsonPropertyName("hasImmunityCompromisingMedicalConditionAntiViralTri")]
    public CovidTherapyAssessmentOption HasImmunityCompromisingMedicalConditionAntiViralTri { get; set; }

    /// <summary>
    /// Gets or sets the reports 3 doses of covid 19 vaccine option.
    /// </summary>
    [JsonPropertyName("reports3DosesC19Vaccine")]
    public CovidTherapyAssessmentOption Reports3DosesC19Vaccine { get; set; }

    /// <summary>
    /// Gets or sets the has chronic condition diagnoses option.
    /// </summary>
    [JsonPropertyName("hasChronicConditionDiagnoses")]
    public CovidTherapyAssessmentOption HasChronicConditionDiagnoses { get; set; }

    /// <summary>
    /// Gets or sets the agent comments.
    /// </summary>
    [JsonPropertyName("agentComments")]
    public string? AgentComments { get; set; }

    /// <summary>
    /// Gets street address.
    /// </summary>
    [JsonPropertyName("streetAddresses")]
    public IList<string>? StreetAddresses { get; } = new List<string>();

    /// <summary>
    /// Gets or sets the city.
    /// </summary>
    [JsonPropertyName("city")]
    public string? City { get; set; }

    /// <summary>
    /// Gets or sets the province or state.
    /// </summary>
    [JsonPropertyName("provOrState")]
    public string? ProvOrState { get; set; }

    /// <summary>
    /// Gets or sets the postal code.
    /// </summary>
    [JsonPropertyName("postalCode")]
    public string? PostalCode { get; set; }

    /// <summary>
    /// Gets or sets the country.
    /// </summary>
    [JsonPropertyName("country")]
    public string? Country { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether address flag has changed.
    /// </summary>
    [JsonPropertyName("changeAddressFlag")]
    public bool ChangeAddressFlag { get; set; }
}
