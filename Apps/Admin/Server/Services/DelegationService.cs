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
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Threading.Tasks;
    using AutoMapper;
    using FluentValidation.Results;
    using HealthGateway.Admin.Common.Constants;
    using HealthGateway.Admin.Common.Models;
    using HealthGateway.Admin.Server.Validations;
    using HealthGateway.Common.Constants;
    using HealthGateway.Common.Data.Constants;
    using HealthGateway.Common.Data.ErrorHandling;
    using HealthGateway.Common.Data.ViewModels;
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
        public async Task<DelegationInfo> GetDelegationInformationAsync(string phn)
        {
            // Get dependent patient information
            RequestResult<PatientModel> dependentPatientResult = await this.patientService.GetPatient(phn, PatientIdentifierType.Phn).ConfigureAwait(true);
            this.ValidatePatientResult(dependentPatientResult);

            ValidationResult? validationResults = await new DelegationValidator(this.maxDependentAge).ValidateAsync(dependentPatientResult.ResourcePayload).ConfigureAwait(true);
            if (!validationResults.IsValid)
            {
                throw new ProblemDetailsException(
                    ExceptionUtility.CreateProblemDetails($"Dependent age exceeds the maximum limit of {this.maxDependentAge}", HttpStatusCode.BadRequest, nameof(DelegationService)));
            }

            DelegationInfo delegationInfo = new();
            if (dependentPatientResult.ResourcePayload != null)
            {
                PatientModel dependentPatientInfo = dependentPatientResult.ResourcePayload;
                DependentInfo dependentInfo = this.autoMapper.Map<DependentInfo>(dependentPatientInfo);
                delegationInfo.Dependent = dependentInfo;

                // Get delegates from database
                IEnumerable<ResourceDelegate> dbResourceDelegates = await this.SearchDelegates(dependentPatientInfo.HdId).ConfigureAwait(true);

                List<DelegateInfo> delegates = new();
                foreach (ResourceDelegate resourceDelegate in dbResourceDelegates)
                {
                    RequestResult<PatientModel> delegatePatientResult = await this.patientService.GetPatient(resourceDelegate.ProfileHdid).ConfigureAwait(true);
                    this.ValidatePatientResult(delegatePatientResult);

                    DelegateInfo delegateInfo = this.autoMapper.Map<DelegateInfo>(delegatePatientResult.ResourcePayload);
                    delegateInfo.DelegationStatus = DelegationStatus.Added;
                    delegates.Add(delegateInfo);
                }

                delegationInfo.Delegates = delegates;
            }

            return delegationInfo;
        }

        private async Task<IEnumerable<ResourceDelegate>> SearchDelegates(string ownerHdid)
        {
            ResourceDelegateQuery query = new() { ByOwnerHdid = ownerHdid };
            ResourceDelegateQueryResult result = await this.resourceDelegateDelegate.Search(query).ConfigureAwait(true);
            return result.Items;
        }

        private void ValidatePatientResult(RequestResult<PatientModel> patientResult)
        {
            switch (patientResult)
            {
                case { ResultStatus: ResultType.ActionRequired, ResultError: { ActionCode: { } } error } when error.ActionCode.Equals(ActionType.Validation):
                    throw new ProblemDetailsException(ExceptionUtility.CreateProblemDetails(error.ResultMessage, HttpStatusCode.BadRequest, nameof(DelegationService)));
                case { ResultStatus: ResultType.Error, ResultError: { } error } when error.ResultMessage.StartsWith("Communication Exception", StringComparison.InvariantCulture):
                    throw new ProblemDetailsException(ExceptionUtility.CreateProblemDetails(error.ResultMessage, HttpStatusCode.BadGateway, nameof(DelegationService)));
                case { ResultStatus: ResultType.Error or ResultType.ActionRequired } or { ResourcePayload: null }:
                    throw new ProblemDetailsException(ExceptionUtility.CreateProblemDetails("Patient not found", HttpStatusCode.NotFound, nameof(DelegationService)));
            }
        }
    }
}
