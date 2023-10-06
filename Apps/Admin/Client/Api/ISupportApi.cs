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
namespace HealthGateway.Admin.Client.Api;

using System.Collections.Generic;
using System.Threading.Tasks;
using HealthGateway.Admin.Common.Constants;
using HealthGateway.Admin.Common.Models;
using HealthGateway.Admin.Common.Models.CovidSupport;
using HealthGateway.Common.Data.Constants;
using HealthGateway.Common.Data.Models;
using Refit;

/// <summary>
/// APIs to fetch support-related data from the server.
/// </summary>
public interface ISupportApi
{
    /// <summary>
    /// Retrieves the collection of patients that match the query.
    /// </summary>
    /// <param name="queryType">The type of query to perform.</param>
    /// <param name="queryString">The value to query on.</param>
    /// <returns>The collection of patient support results that match the query.</returns>
    [Get("/Users?queryType={queryType}&queryString={queryString}")]
    Task<IList<PatientSupportResult>> GetPatientsAsync(PatientQueryType queryType, string queryString);

    /// <summary>
    /// Gets the patient support details model from the server.
    /// </summary>
    /// <param name="queryType">The type of query to be performed when searching for patient support details.</param>
    /// <param name="queryString">The string value associated with the query type when searching for patient support details.</param>
    /// <returns>The patient support details object.</returns>
    [Get("/PatientSupportDetails?queryType={queryType}&queryString={queryString}")]
    Task<PatientSupportDetails> GetPatientSupportDetailsAsync(ClientRegistryType queryType, string queryString);

    /// <summary>
    /// Creates, updates, or deletes block access configuration for the passed HDID.
    /// </summary>
    /// <param name="hdid">HDID of the patient to restrict access.</param>
    /// <param name="request">The request containing all datasources to block for the patient.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [Put("/{hdid}/BlockAccess")]
    Task BlockAccessAsync(string hdid, BlockAccessRequest request);

    /// <summary>
    /// Triggers the process to physically mail the Vaccine Card document.
    /// </summary>
    /// <param name="request">The mail document request.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [Post("/Patient/Document")]
    Task MailVaccineCard(MailDocumentRequest request);

    /// <summary>
    /// Gets the COVID-19 Vaccine Record document that includes the Vaccine Card and Vaccination History.
    /// </summary>
    /// <param name="phn">The personal health number that matches the document to retrieve.</param>
    /// <returns>The encoded immunization document.</returns>
    [Get("/Patient/Document?phn={phn}")]
    Task<ReportModel> RetrieveVaccineRecord(string phn);

    /// <summary>
    /// Submitting a completed anti viral screening form.
    /// </summary>
    /// <param name="request">The covid therapy assessment request to use for submission.</param>
    /// <returns>A CovidAssessmentResponse.</returns>
    [Post("/CovidAssessment")]
    Task<CovidAssessmentResponse> SubmitCovidAssessment(CovidAssessmentRequest request);
}
