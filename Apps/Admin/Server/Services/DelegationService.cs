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
    using System.Linq;
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
        private readonly IDelegationDelegate delegationDelegate;
        private readonly IMapper autoMapper;
        private readonly int maxDependentAge;

        /// <summary>
        /// Initializes a new instance of the <see cref="DelegationService"/> class.
        /// </summary>
        /// <param name="configuration">The injected configuration provider.</param>
        /// <param name="patientService">The injected patient service.</param>
        /// <param name="resourceDelegateDelegate">The injected resource delegate delegate.</param>
        /// <param name="delegationDelegate">The injected delegation delegate.</param>
        /// <param name="autoMapper">The injected automapper provider.</param>
        public DelegationService(
            IConfiguration configuration,
            IPatientService patientService,
            IResourceDelegateDelegate resourceDelegateDelegate,
            IDelegationDelegate delegationDelegate,
            IMapper autoMapper)
        {
            this.patientService = patientService;
            this.resourceDelegateDelegate = resourceDelegateDelegate;
            this.delegationDelegate = delegationDelegate;
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

                // Get dependent
                Dependent? dependent = await this.delegationDelegate.GetDependent(dependentPatientInfo.HdId, true).ConfigureAwait(true);

                if (dependent != null)
                {
                    foreach (AllowedDelegation allowedDelegation in dependent.AllowedDelegations)
                    {
                        RequestResult<PatientModel> delegatePatientResult = await this.patientService.GetPatient(allowedDelegation.DelegateHdId).ConfigureAwait(true);

                        DelegateInfo delegateInfo = this.autoMapper.Map<DelegateInfo>(delegatePatientResult.ResourcePayload);
                        delegateInfo.DelegationStatus = DelegationStatus.Allowed;

                        // If delegate info already exists in current list, then update delegation status to allowed.
                        delegates.Where(x => x.Hdid == delegateInfo.Hdid).ToList().ForEach(x => x.DelegationStatus = DelegationStatus.Allowed);

                        // If delegate info does not exist in current list, then add delegate info.
                        if (!delegates.Exists(x => x.Hdid == delegateInfo.Hdid))
                        {
                            delegates.Add(delegateInfo);
                        }
                    }
                }

                delegationInfo.Delegates = delegates;
            }

            return delegationInfo;
        }

        /// <inheritdoc/>
        public async Task ProtectDependentAsync(string dependentHdid, IList<string> delegateHdids)
        {
            Dependent? dependent = await this.delegationDelegate.GetDependent(dependentHdid, true).ConfigureAwait(true);

            if (dependent == null)
            {
                dependent = new Dependent
                {
                    HdId = dependentHdid,
                };
            }

            dependent.Protected = true;
            await this.UpdateDelegationAsync(dependent, delegateHdids.ToList()).ConfigureAwait(true);
        }

        /// <inheritdoc/>
        public async Task UnprotectDependentAsync(string dependentHdid)
        {
            Dependent? dependent = await this.delegationDelegate.GetDependent(dependentHdid, true).ConfigureAwait(true);

            if (dependent == null)
            {
                throw new ProblemDetailsException(ExceptionUtility.CreateProblemDetails($"Dependent not found for hdid: {dependentHdid}", HttpStatusCode.NotFound, nameof(DelegationService)));
            }

            dependent.Protected = false;
            dependent.AllowedDelegations.Clear();
            await this.delegationDelegate.UpdateDelegation(dependent, Enumerable.Empty<ResourceDelegate>()).ConfigureAwait(true);
        }

        private async Task UpdateDelegationAsync(Dependent dependent, List<string> delegateHdids)
        {
            // Compare dependent allowed delegations in database with passed in delegate hdids to determine which allowed delegations to remove.
            IList<AllowedDelegation> allowedDelegationsToDelete =
                dependent.AllowedDelegations.Where(x => !delegateHdids.Exists(y => y == x.DelegateHdId)).ToList();

            foreach (AllowedDelegation delegation in allowedDelegationsToDelete)
            {
                dependent.AllowedDelegations.Remove(delegation);
            }

            // Compare passed in delegate hdids with dependent allowed delegations in database to determine what allowed delegations to add.
            IList<string> delegateHdidsToAdd =
                delegateHdids.Where(x => !dependent.AllowedDelegations.ToList().Exists(y => y.DelegateHdId == x)).ToList();

            foreach (string delegateHdid in delegateHdidsToAdd)
            {
                dependent.AllowedDelegations.Add(
                    new AllowedDelegation
                    {
                        DependentHdId = dependent.HdId,
                        DelegateHdId = delegateHdid,
                    });
            }

            IEnumerable<ResourceDelegate> resourceDelegates = await this.SearchDelegates(dependent.HdId).ConfigureAwait(true);

            // Compare resource delegates with passed in delegate hdids to determine which resource delegates to remove
            IEnumerable<ResourceDelegate> resourceDelegatesToDelete = resourceDelegates.Where(r => r.ResourceOwnerHdid == dependent.HdId && !delegateHdids.Exists(a => a == r.ProfileHdid));

            // Update dependent, allow delegation and resource delegate in database
            await this.delegationDelegate.UpdateDelegation(dependent, resourceDelegatesToDelete).ConfigureAwait(true);
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
