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
    using HealthGateway.Common.AccessManagement.Authentication;
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
        private const string MinDelegateAgeKey = "MinDelegateAge";
        private readonly IPatientService patientService;
        private readonly IResourceDelegateDelegate resourceDelegateDelegate;
        private readonly IDelegationDelegate delegationDelegate;
        private readonly IAuthenticationDelegate authenticationDelegate;
        private readonly IMapper autoMapper;
        private readonly int maxDependentAge;
        private readonly int minDelegateAge;

        /// <summary>
        /// Initializes a new instance of the <see cref="DelegationService"/> class.
        /// </summary>
        /// <param name="configuration">The injected configuration provider.</param>
        /// <param name="patientService">The injected patient service.</param>
        /// <param name="resourceDelegateDelegate">The injected resource delegate delegate.</param>
        /// <param name="delegationDelegate">The injected delegation delegate.</param>
        /// <param name="authenticationDelegate">The injected authentication delegate.</param>
        /// <param name="autoMapper">The injected automapper provider.</param>
        public DelegationService(
            IConfiguration configuration,
            IPatientService patientService,
            IResourceDelegateDelegate resourceDelegateDelegate,
            IDelegationDelegate delegationDelegate,
            IAuthenticationDelegate authenticationDelegate,
            IMapper autoMapper)
        {
            this.patientService = patientService;
            this.resourceDelegateDelegate = resourceDelegateDelegate;
            this.delegationDelegate = delegationDelegate;
            this.authenticationDelegate = authenticationDelegate;
            this.autoMapper = autoMapper;
            this.maxDependentAge = configuration.GetSection(DelegationConfigSection).GetValue(MaxDependentAgeKey, 12);
            this.minDelegateAge = configuration.GetSection(DelegationConfigSection).GetValue(MinDelegateAgeKey, 12);
        }

        /// <inheritdoc/>
        public async Task<DelegationInfo> GetDelegationInformationAsync(string phn)
        {
            // Get dependent patient information
            RequestResult<PatientModel> dependentPatientResult = await this.patientService.GetPatient(phn, PatientIdentifierType.Phn).ConfigureAwait(true);
            this.ValidatePatientResult(dependentPatientResult);

            ValidationResult? validationResults = await new DependentPatientValidator(this.maxDependentAge).ValidateAsync(dependentPatientResult.ResourcePayload).ConfigureAwait(true);
            if (!validationResults.IsValid)
            {
                throw new ProblemDetailsException(
                    ExceptionUtility.CreateProblemDetails($"Dependent age is above {this.maxDependentAge}", HttpStatusCode.BadRequest, nameof(DelegationService)));
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
                Dependent? dependent = await this.delegationDelegate.GetDependentAsync(dependentPatientInfo.HdId, true).ConfigureAwait(true);

                if (dependent != null)
                {
                    delegationInfo.Dependent.Protected = dependent.Protected;

                    foreach (AllowedDelegation allowedDelegation in dependent.AllowedDelegations.Where(ad => delegates.All(d => d.Hdid != ad.DelegateHdId)))
                    {
                        RequestResult<PatientModel> delegatePatientResult = await this.patientService.GetPatient(allowedDelegation.DelegateHdId).ConfigureAwait(true);

                        DelegateInfo delegateInfo = this.autoMapper.Map<DelegateInfo>(delegatePatientResult.ResourcePayload);
                        delegateInfo.DelegationStatus = DelegationStatus.Allowed;
                        delegates.Add(delegateInfo);
                    }
                }

                delegationInfo.Delegates = delegates;
            }

            return delegationInfo;
        }

        /// <inheritdoc/>
        public async Task<DelegateInfo> GetDelegateInformationAsync(string phn)
        {
            RequestResult<PatientModel> delegatePatientResult = await this.patientService.GetPatient(phn, PatientIdentifierType.Phn).ConfigureAwait(true);
            this.ValidatePatientResult(delegatePatientResult);

            ValidationResult? validationResults = await new DelegatePatientValidator(this.minDelegateAge).ValidateAsync(delegatePatientResult.ResourcePayload).ConfigureAwait(true);
            if (!validationResults.IsValid)
            {
                throw new ProblemDetailsException(ExceptionUtility.CreateProblemDetails($"Delegate age is below {this.minDelegateAge}", HttpStatusCode.BadRequest, nameof(DelegationService)));
            }

            DelegateInfo delegateInfo = this.autoMapper.Map<DelegateInfo>(delegatePatientResult.ResourcePayload);
            return delegateInfo;
        }

        /// <inheritdoc/>
        public async Task ProtectDependentAsync(string dependentHdid, IEnumerable<string> delegateHdids, string reason)
        {
            string authenticatedUserId = this.authenticationDelegate.FetchAuthenticatedUserId() ?? UserId.DefaultUser;
            Dependent? dependent = await this.delegationDelegate.GetDependentAsync(dependentHdid, true).ConfigureAwait(true);
            dependent ??= new() { HdId = dependentHdid, CreatedBy = authenticatedUserId };

            dependent.Protected = true;
            dependent.UpdatedBy = authenticatedUserId;

            DependentAudit dependentAudit = new()
            {
                DependentHdId = dependentHdid,
                ProtectedReason = reason,
                OperationCode = DependentAuditOperation.Protect,
                AgentUsername = this.authenticationDelegate.FetchAuthenticatedPreferredUsername() ?? authenticatedUserId,
                CreatedBy = authenticatedUserId,
                UpdatedBy = authenticatedUserId,
            };

            await this.UpdateDelegationAsync(dependent, delegateHdids.ToList(), authenticatedUserId, dependentAudit).ConfigureAwait(true);
        }

        /// <inheritdoc/>
        public async Task UnprotectDependentAsync(string dependentHdid, string reason)
        {
            string authenticatedUserId = this.authenticationDelegate.FetchAuthenticatedUserId() ?? UserId.DefaultUser;
            Dependent? dependent = await this.delegationDelegate.GetDependentAsync(dependentHdid, true).ConfigureAwait(true);

            if (dependent == null)
            {
                throw new ProblemDetailsException(ExceptionUtility.CreateProblemDetails($"Dependent not found for hdid: {dependentHdid}", HttpStatusCode.NotFound, nameof(DelegationService)));
            }

            dependent.Protected = false;
            dependent.UpdatedBy = authenticatedUserId;
            dependent.AllowedDelegations.Clear();

            DependentAudit dependentAudit = new()
            {
                DependentHdId = dependentHdid,
                ProtectedReason = reason,
                OperationCode = DependentAuditOperation.Unprotect,
                AgentUsername = this.authenticationDelegate.FetchAuthenticatedPreferredUsername() ?? authenticatedUserId,
                CreatedBy = authenticatedUserId,
                UpdatedBy = authenticatedUserId,
            };

            await this.delegationDelegate.UpdateDelegationAsync(dependent, Enumerable.Empty<ResourceDelegate>(), dependentAudit).ConfigureAwait(true);
        }

        private async Task UpdateDelegationAsync(Dependent dependent, IList<string> delegateHdids, string authenticatedUserId, DependentAudit dependentAudit)
        {
            // Compare dependent allowed delegations in database with passed in delegate hdids to determine which allowed delegations to remove.
            IEnumerable<AllowedDelegation> allowedDelegationsToDelete =
                dependent.AllowedDelegations.Where(x => delegateHdids.All(y => y != x.DelegateHdId));

            foreach (AllowedDelegation delegation in allowedDelegationsToDelete.ToList())
            {
                dependent.AllowedDelegations.Remove(delegation);
            }

            // Compare passed in delegate hdids with dependent allowed delegations in database to determine what allowed delegations to add.
            IEnumerable<string> delegateHdidsToAdd =
                delegateHdids.Where(x => dependent.AllowedDelegations.All(y => y.DelegateHdId != x));

            foreach (string delegateHdid in delegateHdidsToAdd)
            {
                dependent.AllowedDelegations.Add(
                    new()
                    {
                        DependentHdId = dependent.HdId,
                        DelegateHdId = delegateHdid,
                        CreatedBy = authenticatedUserId,
                        UpdatedBy = authenticatedUserId,
                    });
            }

            IEnumerable<ResourceDelegate> resourceDelegates = await this.SearchDelegates(dependent.HdId).ConfigureAwait(true);

            // Compare resource delegates with passed in delegate hdids to determine which resource delegates to remove
            IEnumerable<ResourceDelegate> resourceDelegatesToDelete = resourceDelegates.Where(r => delegateHdids.All(a => a != r.ProfileHdid));

            // Update dependent, allow delegation and resource delegate in database
            await this.delegationDelegate.UpdateDelegationAsync(dependent, resourceDelegatesToDelete, dependentAudit).ConfigureAwait(true);
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
