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
namespace HealthGateway.Medication.Services
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Threading.Tasks;
    using HealthGateway.Common.Data.Constants;
    using HealthGateway.Common.Data.ViewModels;
    using HealthGateway.Common.Models;
    using HealthGateway.Common.Services;
    using HealthGateway.Medication.Delegates;
    using HealthGateway.Medication.Models;

    /// <summary>
    /// The Medication data service.
    /// </summary>
    public class MedicationRequestService : IMedicationRequestService
    {
        private readonly IPatientService patientService;
        private readonly IMedicationRequestDelegate medicationRequestDelegate;

        /// <summary>
        /// Initializes a new instance of the <see cref="MedicationRequestService"/> class.
        /// </summary>
        /// <param name="patientService">The injected patient registry provider.</param>
        /// <param name="medicationRequestDelegate">Injected medication statement delegate.</param>
        public MedicationRequestService(
            IPatientService patientService,
            IMedicationRequestDelegate medicationRequestDelegate)
        {
            this.patientService = patientService;
            this.medicationRequestDelegate = medicationRequestDelegate;
        }

        private static ActivitySource Source { get; } = new ActivitySource(nameof(MedicationRequestService));

        /// <inheritdoc/>
        public async Task<RequestResult<IList<MedicationRequest>>> GetMedicationRequests(string hdid)
        {
            // Retrieve the phn
            RequestResult<PatientModel> patientResult = await this.patientService.GetPatient(hdid).ConfigureAwait(true);
            if (patientResult.ResultStatus == ResultType.Success && patientResult.ResourcePayload != null)
            {
                PatientModel patient = patientResult.ResourcePayload;
                RequestResult<IList<MedicationRequest>> delegateResult = await this.medicationRequestDelegate.GetMedicationRequestsAsync(patient.PersonalHealthNumber).ConfigureAwait(true);
                if (delegateResult.ResultStatus == ResultType.Success)
                {
                    return new RequestResult<IList<MedicationRequest>>()
                    {
                        ResultStatus = delegateResult.ResultStatus,
                        ResourcePayload = delegateResult.ResourcePayload,
                        PageIndex = delegateResult.PageIndex,
                        PageSize = delegateResult.PageSize,
                        TotalResultCount = delegateResult.TotalResultCount,
                    };
                }
                else
                {
                    return new RequestResult<IList<MedicationRequest>>()
                    {
                        ResultStatus = delegateResult.ResultStatus,
                        ResultError = delegateResult.ResultError,
                    };
                }
            }
            else
            {
                return new RequestResult<IList<MedicationRequest>>()
                {
                    ResultError = patientResult.ResultError,
                };
            }
        }
    }
}
