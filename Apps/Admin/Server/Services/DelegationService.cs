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
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using FluentValidation.Results;
    using HealthGateway.AccountDataAccess.Audit;
    using HealthGateway.Admin.Common.Constants;
    using HealthGateway.Admin.Common.Models;
    using HealthGateway.Admin.Server.Validations;
    using HealthGateway.Common.AccessManagement.Authentication;
    using HealthGateway.Common.Constants;
    using HealthGateway.Common.Data.Constants;
    using HealthGateway.Common.Data.ErrorHandling;
    using HealthGateway.Common.Data.ViewModels;
    using HealthGateway.Common.Messaging;
    using HealthGateway.Common.Models;
    using HealthGateway.Common.Models.Events;
    using HealthGateway.Common.Services;
    using HealthGateway.Database.Delegates;
    using HealthGateway.Database.Models;
    using Microsoft.Extensions.Configuration;

    /// <inheritdoc/>
    /// <param name="configuration">The injected configuration provider.</param>
    /// <param name="patientService">The injected patient service.</param>
    /// <param name="resourceDelegateDelegate">The injected resource delegate delegate.</param>
    /// <param name="delegationDelegate">The injected delegation delegate.</param>
    /// <param name="authenticationDelegate">The injected authentication delegate.</param>
    /// <param name="messageSender">The change feed message sender</param>
    /// <param name="auditRepository">The injected agent audit repository.</param>
    /// <param name="autoMapper">The injected automapper provider.</param>
    public class DelegationService(
        IConfiguration configuration,
        IPatientService patientService,
        IResourceDelegateDelegate resourceDelegateDelegate,
        IDelegationDelegate delegationDelegate,
        IAuthenticationDelegate authenticationDelegate,
        IMessageSender messageSender,
        IAuditRepository auditRepository,
        IMapper autoMapper) : IDelegationService
    {
        private const string DelegationConfigSection = "Delegation";
        private const string MaxDependentAgeKey = "MaxDependentAge";
        private const string MinDelegateAgeKey = "MinDelegateAge";
        private const string ChangeFeedConfigSection = "ChangeFeed";
        private const string DependentChangeFeedKey = "Dependents";
        private readonly int maxDependentAge = configuration.GetSection(DelegationConfigSection).GetValue(MaxDependentAgeKey, 12);
        private readonly int minDelegateAge = configuration.GetSection(DelegationConfigSection).GetValue(MinDelegateAgeKey, 12);
        private readonly bool changeFeedEnabled = configuration.GetSection(ChangeFeedConfigSection).GetValue($"{DependentChangeFeedKey}:Enabled", false);

        /// <inheritdoc/>
        public async Task<DelegationInfo> GetDelegationInformationAsync(string phn, CancellationToken ct = default)
        {
            // Get dependent patient information
            RequestResult<PatientModel> dependentPatientResult = await patientService.GetPatient(phn, PatientIdentifierType.Phn);
            this.ValidatePatientResult(dependentPatientResult);

            ValidationResult? validationResults = await new DependentPatientValidator(this.maxDependentAge).ValidateAsync(dependentPatientResult.ResourcePayload, ct).ConfigureAwait(true);
            if (!validationResults.IsValid)
            {
                throw new ProblemDetailsException(ExceptionUtility.CreateProblemDetails($"Dependent age is above {this.maxDependentAge}", HttpStatusCode.BadRequest, nameof(DelegationService)));
            }

            DelegationInfo delegationInfo = new();
            if (dependentPatientResult.ResourcePayload != null)
            {
                PatientModel dependentPatientInfo = dependentPatientResult.ResourcePayload;
                DependentInfo dependentInfo = autoMapper.Map<DependentInfo>(dependentPatientInfo);
                delegationInfo.Dependent = dependentInfo;

                // Get delegates from database
                IEnumerable<ResourceDelegate> dbResourceDelegates = await this.SearchDelegates(dependentPatientInfo.HdId);

                List<DelegateInfo> delegates = new();
                foreach (ResourceDelegate resourceDelegate in dbResourceDelegates)
                {
                    RequestResult<PatientModel> delegatePatientResult = await patientService.GetPatient(resourceDelegate.ProfileHdid);
                    this.ValidatePatientResult(delegatePatientResult);

                    DelegateInfo delegateInfo = autoMapper.Map<DelegateInfo>(delegatePatientResult.ResourcePayload);
                    delegateInfo.DelegationStatus = DelegationStatus.Added;
                    delegates.Add(delegateInfo);
                }

                // Get dependent
                Dependent? dependent = await delegationDelegate.GetDependentAsync(dependentPatientInfo.HdId, true, ct);

                if (dependent != null)
                {
                    delegationInfo.Dependent.Protected = dependent.Protected;

                    foreach (AllowedDelegation allowedDelegation in dependent.AllowedDelegations.Where(ad => delegates.TrueForAll(d => d.Hdid != ad.DelegateHdId)))
                    {
                        RequestResult<PatientModel> delegatePatientResult = await patientService.GetPatient(allowedDelegation.DelegateHdId);

                        DelegateInfo delegateInfo = autoMapper.Map<DelegateInfo>(delegatePatientResult.ResourcePayload);
                        delegateInfo.DelegationStatus = DelegationStatus.Allowed;
                        delegates.Add(delegateInfo);
                    }
                }

                delegationInfo.Delegates = delegates;

                // Get agent audits
                AgentAuditQuery agentAuditQuery = new(dependentPatientInfo.HdId, AuditGroup.Dependent);
                IEnumerable<AgentAudit> agentAudits = await auditRepository.Handle(agentAuditQuery, ct).ConfigureAwait(true);
                delegationInfo.AgentActions = agentAudits.Select(a => autoMapper.Map<AgentAction>(a));
            }

            return delegationInfo;
        }

        /// <inheritdoc/>
        public async Task<DelegateInfo> GetDelegateInformationAsync(string phn)
        {
            RequestResult<PatientModel> delegatePatientResult = await patientService.GetPatient(phn, PatientIdentifierType.Phn);
            this.ValidatePatientResult(delegatePatientResult);

            ValidationResult? validationResults = await new DelegatePatientValidator(this.minDelegateAge).ValidateAsync(delegatePatientResult.ResourcePayload);
            if (!validationResults.IsValid)
            {
                throw new ProblemDetailsException(ExceptionUtility.CreateProblemDetails($"Delegate age is below {this.minDelegateAge}", HttpStatusCode.BadRequest, nameof(DelegationService)));
            }

            DelegateInfo delegateInfo = autoMapper.Map<DelegateInfo>(delegatePatientResult.ResourcePayload);
            return delegateInfo;
        }

        /// <inheritdoc/>
        public async Task<AgentAction> ProtectDependentAsync(string dependentHdid, IEnumerable<string> delegateHdids, string reason, CancellationToken ct = default)
        {
            string authenticatedUserId = authenticationDelegate.FetchAuthenticatedUserId() ?? UserId.DefaultUser;
            Dependent? dependent = await delegationDelegate.GetDependentAsync(dependentHdid, true, ct);
            dependent ??= new() { HdId = dependentHdid, CreatedBy = authenticatedUserId };

            dependent.Protected = true;
            dependent.UpdatedBy = authenticatedUserId;

            AgentAudit agentAudit = new()
            {
                Hdid = dependentHdid,
                Reason = reason,
                OperationCode = AuditOperation.ProtectDependent,
                GroupCode = AuditGroup.Dependent,
                AgentUsername = authenticationDelegate.FetchAuthenticatedPreferredUsername() ?? authenticatedUserId,
                TransactionDateTime = DateTime.UtcNow,
                CreatedBy = authenticatedUserId,
                UpdatedBy = authenticatedUserId,
            };

            IList<string> delegateHdidList = delegateHdids.ToList();

            // Compare dependent allowed delegations in database with passed in delegate hdids to determine which allowed delegations to remove.
            IEnumerable<AllowedDelegation> allowedDelegationsToDelete =
                dependent.AllowedDelegations.Where(x => delegateHdidList.All(y => y != x.DelegateHdId));

            foreach (AllowedDelegation delegation in allowedDelegationsToDelete.ToList())
            {
                dependent.AllowedDelegations.Remove(delegation);
            }

            // Compare passed in delegate hdids with dependent allowed delegations in database to determine what allowed delegations to add.
            IEnumerable<string> delegateHdidsToAdd =
                delegateHdidList.Where(x => dependent.AllowedDelegations.All(y => y.DelegateHdId != x));

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

            IEnumerable<ResourceDelegate> resourceDelegates = await this.SearchDelegates(dependent.HdId);

            // Compare resource delegates with passed in delegate hdids to determine which resource delegates to remove
            IEnumerable<ResourceDelegate> resourceDelegatesToDelete = resourceDelegates.Where(r => delegateHdidList.All(a => a != r.ProfileHdid)).ToList();

            // Update dependent, allow delegation and resource delegate in database
            if (this.changeFeedEnabled)
            {
                await delegationDelegate.UpdateDelegationAsync(dependent, resourceDelegatesToDelete, agentAudit, false);
                IEnumerable<MessageEnvelope> events = new MessageEnvelope[]
                {
                    new(new DependentProtectionAddedEvent(dependentHdid), dependentHdid),
                }.Concat(resourceDelegatesToDelete.Select(rd => new MessageEnvelope(new DependentRemovedEvent(rd.ProfileHdid, dependentHdid), rd.ProfileHdid)));

                await messageSender.SendAsync(events, ct);
            }
            else
            {
                await delegationDelegate.UpdateDelegationAsync(dependent, resourceDelegatesToDelete, agentAudit);
            }

            return autoMapper.Map<AgentAudit, AgentAction>(agentAudit);
        }

        /// <inheritdoc/>
        public async Task<AgentAction> UnprotectDependentAsync(string dependentHdid, string reason, CancellationToken ct = default)
        {
            string authenticatedUserId = authenticationDelegate.FetchAuthenticatedUserId() ?? UserId.DefaultUser;
            Dependent? dependent = await delegationDelegate.GetDependentAsync(dependentHdid, true, ct);

            if (dependent == null)
            {
                throw new ProblemDetailsException(ExceptionUtility.CreateProblemDetails($"Dependent not found for hdid: {dependentHdid}", HttpStatusCode.NotFound, nameof(DelegationService)));
            }

            dependent.Protected = false;
            dependent.UpdatedBy = authenticatedUserId;
            dependent.AllowedDelegations.Clear();

            AgentAudit agentAudit = new()
            {
                Hdid = dependentHdid,
                Reason = reason,
                OperationCode = AuditOperation.UnprotectDependent,
                GroupCode = AuditGroup.Dependent,
                AgentUsername = authenticationDelegate.FetchAuthenticatedPreferredUsername() ?? authenticatedUserId,
                TransactionDateTime = DateTime.UtcNow,
                CreatedBy = authenticatedUserId,
                UpdatedBy = authenticatedUserId,
            };

            if (this.changeFeedEnabled)
            {
                await delegationDelegate.UpdateDelegationAsync(dependent, [], agentAudit, false);

                MessageEnvelope[] events =
                {
                    new(new DependentProtectionRemovedEvent(dependentHdid), dependentHdid),
                };

                await messageSender.SendAsync(events, ct);
            }
            else
            {
                await delegationDelegate.UpdateDelegationAsync(dependent, [], agentAudit);
            }

            return autoMapper.Map<AgentAudit, AgentAction>(agentAudit);
        }

        private async Task<IEnumerable<ResourceDelegate>> SearchDelegates(string ownerHdid)
        {
            ResourceDelegateQuery query = new() { ByOwnerHdid = ownerHdid };
            ResourceDelegateQueryResult result = await resourceDelegateDelegate.SearchAsync(query);
            return result.Items.Select(c => c.ResourceDelegate);
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
