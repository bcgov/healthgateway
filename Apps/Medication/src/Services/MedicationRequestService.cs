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
    using System.Threading.Tasks;
    using HealthGateway.AccountDataAccess.Patient;
    using HealthGateway.Common.Data.Constants;
    using HealthGateway.Common.Data.ViewModels;
    using HealthGateway.Common.Services;
    using HealthGateway.Medication.Delegates;
    using HealthGateway.Medication.Models;
    using PatientModel = HealthGateway.Common.Models.PatientModel;

    /// <summary>
    /// The Medication data service.
    /// </summary>
    public class MedicationRequestService : IMedicationRequestService
    {
        private readonly IMedicationRequestDelegate medicationRequestDelegate;
        private readonly IPatientService patientService;
        private readonly IPatientRepository patientRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="MedicationRequestService"/> class.
        /// </summary>
        /// <param name="patientService">The injected patient registry provider.</param>
        /// <param name="medicationRequestDelegate">Injected medication statement delegate.</param>
        /// <param name="patientRepository">Injected patient repository provider.</param>
        public MedicationRequestService(
            IPatientService patientService,
            IMedicationRequestDelegate medicationRequestDelegate,
            IPatientRepository patientRepository)
        {
            this.patientService = patientService;
            this.medicationRequestDelegate = medicationRequestDelegate;
            this.patientRepository = patientRepository;
        }

        /// <inheritdoc/>
        public async Task<RequestResult<IList<MedicationRequest>>> GetMedicationRequests(string hdid)
        {
            if (!await this.patientRepository.CanAccessDataSourceAsync(hdid, DataSource.SpecialAuthorityRequest).ConfigureAwait(true))
            {
                return new RequestResult<IList<MedicationRequest>>
                {
                    ResultStatus = ResultType.Success,
                    ResourcePayload = new List<MedicationRequest>(),
                    TotalResultCount = 0,
                };
            }

            // Retrieve the phn
            RequestResult<PatientModel> patientResult = await this.patientService.GetPatient(hdid).ConfigureAwait(true);
            if (patientResult.ResultStatus == ResultType.Success && patientResult.ResourcePayload != null)
            {
                PatientModel patient = patientResult.ResourcePayload;
                RequestResult<IList<MedicationRequest>> delegateResult = await this.medicationRequestDelegate.GetMedicationRequestsAsync(patient.PersonalHealthNumber).ConfigureAwait(true);
                if (delegateResult.ResultStatus == ResultType.Success)
                {
                    return new RequestResult<IList<MedicationRequest>>
                    {
                        ResultStatus = delegateResult.ResultStatus,
                        ResourcePayload = delegateResult.ResourcePayload,
                        PageIndex = delegateResult.PageIndex,
                        PageSize = delegateResult.PageSize,
                        TotalResultCount = delegateResult.TotalResultCount,
                    };
                }

                return new RequestResult<IList<MedicationRequest>>
                {
                    ResultStatus = delegateResult.ResultStatus,
                    ResultError = delegateResult.ResultError,
                };
            }

            return new RequestResult<IList<MedicationRequest>>
            {
                ResultError = patientResult.ResultError,
            };
        }
    }
}
