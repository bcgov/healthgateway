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
    /// Gets or sets the patients date of birth.
    /// </summary>
    [JsonPropertyName("dob")]
    public DateTime? Birthdate { get; set; }

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
    /// Gets or sets the tested positive in past 7 days.
    /// </summary>
    [JsonPropertyName("testedPositiveInPast7Days")]
    public CovidTherapyAssessmentOption TestedPositiveInPast7Days { get; set; }

    /// <summary>
    /// Gets or sets the has severe covid 19 symptoms option.
    /// </summary>
    [JsonPropertyName("hasSevereCovid19Symptoms")]
    public CovidTherapyAssessmentOption HasSevereCovid19Symptoms { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether there has been mild or moderate covid 19 symptoms.
    /// </summary>
    [JsonPropertyName("hasMildOrModerateCovid19Symptoms")]
    public CovidTherapyAssessmentOption HasMildOrModerateCovid19Symptoms { get; set; }

    /// <summary>
    /// Gets or sets the symptom on set date.
    /// </summary>
    [JsonPropertyName("symptomOnSetDate")]
    public DateTime? SymptomOnSetDate { get; set; }

    /// <summary>
    /// Gets or sets the has immunity compromising medical condition option.
    /// </summary>
    [JsonPropertyName("hasImmunityCompromisingMedicalCondition")]
    public CovidTherapyAssessmentOption HasImmunityCompromisingMedicalCondition { get; set; }

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
    /// Gets or sets the has consented to share information option.
    /// </summary>
    [JsonPropertyName("consentToSendCC")]
    public CovidTherapyAssessmentOption ConsentToSendCC { get; set; }

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

    /// <summary>
    /// Gets or sets a value indicating whether address flag has changed.
    /// </summary>
    [JsonPropertyName("positiveCovidLabData")]
    public string? PositiveCovidLabData { get; set; }

    /// <summary>
    /// Gets or sets a value indicating the covid vaccination history.
    /// </summary>
    [JsonPropertyName("covidVaccinationHistory")]
    public string? CovidVaccinationHistory { get; set; }

    /// <summary>
    /// Gets or sets a value indicating the CEV group details.
    /// </summary>
    [JsonPropertyName("cevGroupDetails")]
    public string? CevGroupDetails { get; set; }

    /// <summary>
    /// Gets or sets a value indicating the submission date.
    /// </summary>
    [JsonPropertyName("submitted")]
    public DateTime? Submitted { get; set; }
}
