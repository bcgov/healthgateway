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
namespace HealthGateway.Admin.Common.Models.CovidSupport;

using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using HealthGateway.Admin.Common.Constants;

/// <summary>
/// Model representing a request for COVID-19 therapy assessment.
/// </summary>
public class CovidAssessmentRequest
{
    /// <summary>
    /// Gets or sets the patient's PHN.
    /// </summary>
    [JsonPropertyName("phn")]
    public string Phn { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the patient's first name.
    /// </summary>
    [JsonPropertyName("firstName")]
    public string FirstName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the patient's last name.
    /// </summary>
    [JsonPropertyName("lastName")]
    public string LastName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the patient's date of birth.
    /// </summary>
    [JsonPropertyName("dob")]
    public DateTime? Birthdate { get; set; }

    /// <summary>
    /// Gets or sets the patient's phone number.
    /// </summary>
    [JsonPropertyName("phoneNumber")]
    public string PhoneNumber { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the response to the family doctor or NP question.
    /// </summary>
    [JsonPropertyName("hasAFamilyDoctorOrNp")]
    public CovidTherapyAssessmentOption HasAFamilyDoctorOrNp { get; set; }

    /// <summary>
    /// Gets or sets the response to the over 12 confirmation question.
    /// </summary>
    [JsonPropertyName("confirmsOver12")]
    public CovidTherapyAssessmentOption ConfirmsOver12 { get; set; }

    /// <summary>
    /// Gets or sets the response to the tested positive in past 7 days question.
    /// </summary>
    [JsonPropertyName("testedPositiveInPast7Days")]
    public CovidTherapyAssessmentOption TestedPositiveInPast7Days { get; set; }

    /// <summary>
    /// Gets or sets the response to the severe COVID-19 symptoms question.
    /// </summary>
    [JsonPropertyName("hasSevereCovid19Symptoms")]
    public CovidTherapyAssessmentOption HasSevereCovid19Symptoms { get; set; }

    /// <summary>
    /// Gets or sets the response to the mild or moderate COVID-19 symptoms question.
    /// </summary>
    [JsonPropertyName("hasMildOrModerateCovid19Symptoms")]
    public CovidTherapyAssessmentOption HasMildOrModerateCovid19Symptoms { get; set; }

    /// <summary>
    /// Gets or sets the symptom onset date.
    /// </summary>
    [JsonPropertyName("symptomOnSetDate")]
    public DateTime? SymptomOnsetDate { get; set; }

    /// <summary>
    /// Gets or sets the response to the immunity-compromising medical condition question.
    /// </summary>
    [JsonPropertyName("hasImmunityCompromisingMedicalCondition")]
    public CovidTherapyAssessmentOption HasImmunityCompromisingMedicalCondition { get; set; }

    /// <summary>
    /// Gets or sets the response to the chronic condition diagnoses question.
    /// </summary>
    [JsonPropertyName("hasChronicConditionDiagnoses")]
    public CovidTherapyAssessmentOption HasChronicConditionDiagnoses { get; set; }

    /// <summary>
    /// Gets or sets the response to the consent to update CareConnect question.
    /// </summary>
    [JsonPropertyName("consentToSendCC")]
    public CovidTherapyAssessmentOption ConsentToSendCc { get; set; }

    /// <summary>
    /// Gets or sets the agent comments.
    /// </summary>
    [JsonPropertyName("agentComments")]
    public string AgentComments { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the lines of the patient's street address.
    /// </summary>
    [JsonPropertyName("streetAddresses")]
    public IEnumerable<string> StreetAddresses { get; set; } = [];

    /// <summary>
    /// Gets or sets the patient's city.
    /// </summary>
    [JsonPropertyName("city")]
    public string City { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the patient's province or state.
    /// </summary>
    [JsonPropertyName("provOrState")]
    public string ProvOrState { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the patient's postal code.
    /// </summary>
    [JsonPropertyName("postalCode")]
    public string PostalCode { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the patient's country.
    /// </summary>
    [JsonPropertyName("country")]
    public string Country { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether a change of address has been flagged.
    /// </summary>
    [JsonPropertyName("changeAddressFlag")]
    public bool ChangeAddressFlag { get; set; }

    /// <summary>
    /// Gets or sets positive COVID-19 lab data.
    /// </summary>
    [JsonPropertyName("positiveCovidLabData")]
    public string PositiveCovidLabData { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the COVID-19 vaccination history.
    /// </summary>
    [JsonPropertyName("covidVaccinationHistory")]
    public string CovidVaccinationHistory { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets CEV group details.
    /// </summary>
    [JsonPropertyName("cevGroupDetails")]
    public string CevGroupDetails { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the submission date.
    /// </summary>
    [JsonPropertyName("submitted")]
    public DateTime? Submitted { get; set; }
}
