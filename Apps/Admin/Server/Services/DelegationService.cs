// -------------------------------------------------------------------------
//  Copyright © 2019 Province of British Columbia
//
//  Licensed under the Apache License, Version 2.0 (the "License");
//  you may not use this file except in compliance with the License.
//  You may obtain a copy of the License at
//
//  http://www.apache.org/licenses/LICENSE-2.0
//
//  Unless required by applicable law or agreed to in writing, software
//  distributed under the License is distributed on an "AS IS" BASIS,
//  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  See the License for the specific language governing permissions and
//  limitations under the License.
// -------------------------------------------------------------------------
namespace HealthGateway.Admin.Server.Services
{
    using System.Collections.Generic;
    using System.Net;
    using System.Threading.Tasks;
    using AutoMapper;
    using FluentValidation.Results;
    using HealthGateway.Admin.Common.Constants;
    using HealthGateway.Admin.Common.Models;
    using HealthGateway.Admin.Server.Validations;
    using HealthGateway.Common.Constants;
    using HealthGateway.Common.Data.ErrorHandling;
    using HealthGateway.Common.Data.ViewModels;
    using HealthGateway.Common.Factories;
    using HealthGateway.Common.Models;
    using HealthGateway.Common.Services;
    using HealthGateway.Database.Delegates;
    using HealthGateway.Database.Models;
    using Microsoft.Extensions.Configuration;

    /// <inheritdoc/>
    public class DelegationService : IDelegationService
    {
        private const string DelegationConfigSection = "Delegation";
        private const string MaxDependentAgeKey = "MaxDependentAge";
        private readonly IPatientService patientService;
        private readonly IResourceDelegateDelegate resourceDelegateDelegate;
        private readonly IMapper autoMapper;
        private readonly int maxDependentAge;

        /// <summary>
        /// Initializes a new instance of the <see cref="DelegationService"/> class.
        /// </summary>
        /// <param name="configuration">The injected configuration provider.</param>
        /// <param name="patientService">The injected patient service.</param>
        /// <param name="resourceDelegateDelegate">The injected resource delegate delegate.</param>
        /// <param name="autoMapper">The injected automapper provider.</param>
        public DelegationService(
            IConfiguration configuration,
            IPatientService patientService,
            IResourceDelegateDelegate resourceDelegateDelegate,
            IMapper autoMapper)
        {
            this.patientService = patientService;
            this.resourceDelegateDelegate = resourceDelegateDelegate;
            this.autoMapper = autoMapper;
            this.maxDependentAge = configuration.GetSection(DelegationConfigSection).GetValue(MaxDependentAgeKey, 12);
        }

        /// <inheritdoc/>
        public async Task<DelegationResponse> GetDelegationInformationAsync(string phn)
        {
            // Get dependent patient information
            RequestResult<PatientModel> dependentPatientResult = await this.patientService.GetPatient(phn, PatientIdentifierType.Phn).ConfigureAwait(true);

            ValidationResult? validationResults = await new DelegationValidator(this.maxDependentAge).ValidateAsync(dependentPatientResult.ResourcePayload).ConfigureAwait(true);
            if (!validationResults.IsValid)
            {
                throw new ProblemDetailsException(ExceptionUtility.CreateProblemDetails(MaxDependentAgeKey, HttpStatusCode.BadRequest, nameof(DelegationService)));
            }

            DelegationResponse delegationResponse = new();
            if (dependentPatientResult.ResourcePayload != null)
            {
                PatientModel dependentPatientInfo = dependentPatientResult.ResourcePayload;
                DependentInfo dependentInfo = this.autoMapper.Map<DependentInfo>(dependentPatientInfo);
                dependentInfo.Protected = null;
                delegationResponse.DependentInfo = dependentInfo;

                // Get delegates from database
                RequestResult<IEnumerable<ResourceDelegate>> dbResourceDelegates = await this.SearchDelegates(dependentPatientInfo.HdId).ConfigureAwait(true);

                if (dbResourceDelegates.ResourcePayload != null)
                {
                    List<DelegateInfo> delegateInfos = new();
                    foreach (ResourceDelegate resourceDelegate in dbResourceDelegates.ResourcePayload)
                    {
                        RequestResult<PatientModel> delegatePatientResult = await this.patientService.GetPatient(resourceDelegate.ResourceOwnerHdid).ConfigureAwait(true);

                        DelegateInfo delegateInfo = this.autoMapper.Map<DelegateInfo>(delegatePatientResult);
                        delegateInfo.DelegationStatus = DelegationStatus.Added;
                        delegateInfos.Add(delegateInfo);
                    }

                    delegationResponse.DelegateInfos = delegateInfos;
                }
            }

            return delegationResponse;
        }

        private async Task<RequestResult<IEnumerable<ResourceDelegate>>> SearchDelegates(string ownerHdid)
        {
            IEnumerable<ResourceDelegate> results = (await this.resourceDelegateDelegate.Search(
                    new ResourceDelegateQuery
                    {
                        ByOwnerHdid = ownerHdid,
                    })
                .ConfigureAwait(true)).Items;

            return RequestResultFactory.Success(results);
        }
    }
}
