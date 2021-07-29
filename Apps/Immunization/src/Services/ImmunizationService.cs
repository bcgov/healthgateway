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
namespace HealthGateway.Immunization.Services
{
    using System.Threading.Tasks;
    using HealthGateway.Common.Constants;
    using HealthGateway.Common.Models;
    using HealthGateway.Common.Models.Immunization;
    using HealthGateway.Common.Models.PHSA;
    using HealthGateway.Common.Services;
    using HealthGateway.Immunization.Delegates;
    using HealthGateway.Immunization.Models;
    using HealthGateway.Immunization.Models.PHSA;
    using HealthGateway.Immunization.Parser;

    /// <summary>
    /// The Immunization data service.
    /// </summary>
    public class ImmunizationService : IImmunizationService
    {
        private readonly IImmunizationDelegate immunizationDelegate;
        private readonly IPatientService patientService;

        /// <summary>
        /// Initializes a new instance of the <see cref="ImmunizationService"/> class.
        /// </summary>
        /// <param name="immunizationDelegate">The factory to create immunization delegates.</param>
        /// <param name="patientService">The injected patient registry provider.</param>
        public ImmunizationService(
            IImmunizationDelegate immunizationDelegate,
            IPatientService patientService)
        {
            this.immunizationDelegate = immunizationDelegate;
            this.patientService = patientService;
        }

        /// <inheritdoc/>
        public async Task<RequestResult<string>> GetCovidCard(string identifier, PatientIdentifierType identifierType)
        {
            RequestResult<string> retVal = new ()
            {
                ResultStatus = ResultType.Error,
            };
            string? phn = null;
            if (identifierType == PatientIdentifierType.HDID)
            {
                RequestResult<PatientModel> patientResult = await this.patientService.GetPatient(identifier).ConfigureAwait(true);
                if (patientResult.ResultStatus == ResultType.Success && patientResult.ResourcePayload != null)
                {
                    phn = patientResult.ResourcePayload.PersonalHealthNumber;
                }
                else
                {
                    retVal.ResultError = patientResult.ResultError;
                }
            }
            else
            {
                phn = identifier;
            }

            return retVal;
        }

        /// <inheritdoc/>
        public async Task<RequestResult<ImmunizationEvent>> GetImmunization(string immunizationId)
        {
            RequestResult<PHSAResult<ImmunizationViewResponse>> delegateResult = await this.immunizationDelegate.GetImmunization(immunizationId).ConfigureAwait(true);
            if (delegateResult.ResultStatus == ResultType.Success)
            {
                return new RequestResult<ImmunizationEvent>()
                {
                    ResultStatus = delegateResult.ResultStatus,
                    ResourcePayload = EventParser.FromPHSAModel(delegateResult.ResourcePayload!.Result),
                    PageIndex = delegateResult.PageIndex,
                    PageSize = delegateResult.PageSize,
                    TotalResultCount = delegateResult.TotalResultCount,
                };
            }
            else
            {
                return new RequestResult<ImmunizationEvent>()
                {
                    ResultStatus = delegateResult.ResultStatus,
                    ResultError = delegateResult.ResultError,
                };
            }
        }

        /// <inheritdoc/>
        public async Task<RequestResult<ImmunizationResult>> GetImmunizations(int pageIndex = 0)
        {
            RequestResult<PHSAResult<ImmunizationResponse>> delegateResult = await this.immunizationDelegate.GetImmunizations(pageIndex).ConfigureAwait(true);
            if (delegateResult.ResultStatus == ResultType.Success)
            {
                return new RequestResult<ImmunizationResult>()
                {
                    ResultStatus = delegateResult.ResultStatus,
                    ResourcePayload = new ImmunizationResult(
                        LoadStateModel.FromPHSAModel(delegateResult.ResourcePayload!.LoadState),
                        EventParser.FromPHSAModelList(delegateResult.ResourcePayload!.Result!.ImmunizationViews),
                        ImmunizationRecommendation.FromPHSAModelList(delegateResult.ResourcePayload.Result.Recommendations)),
                    PageIndex = delegateResult.PageIndex,
                    PageSize = delegateResult.PageSize,
                    TotalResultCount = delegateResult.TotalResultCount,
                };
            }
            else
            {
                return new RequestResult<ImmunizationResult>()
                {
                    ResultStatus = delegateResult.ResultStatus,
                    ResultError = delegateResult.ResultError,
                };
            }
        }
    }
}
