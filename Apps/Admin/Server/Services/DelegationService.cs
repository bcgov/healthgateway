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
    using System.Threading;
    using System.Threading.Tasks;
    using FluentValidation;
    using HealthGateway.AccountDataAccess.Audit;
    using HealthGateway.Admin.Common.Constants;
    using HealthGateway.Admin.Common.Models;
    using HealthGateway.Admin.Server.Validations;
    using HealthGateway.Common.AccessManagement.Authentication;
    using HealthGateway.Common.Constants;
    using HealthGateway.Common.Data.Constants;
    using HealthGateway.Common.Data.ErrorHandling;
    using HealthGateway.Common.Data.Models;
    using HealthGateway.Common.ErrorHandling.Exceptions;
    using HealthGateway.Common.Messaging;
    using HealthGateway.Common.Models;
    using HealthGateway.Common.Models.Events;
    using HealthGateway.Common.Services;
    using HealthGateway.Database.Delegates;
    using HealthGateway.Database.Models;
    using Microsoft.Extensions.Configuration;
    using ValidationException = System.ComponentModel.DataAnnotations.ValidationException;

    /// <inheritdoc/>
    /// <param name="configuration">The injected configuration provider.</param>
    /// <param name="patientService">The injected patient service.</param>
    /// <param name="resourceDelegateDelegate">The injected resource delegate delegate.</param>
    /// <param name="delegationDelegate">The injected delegation delegate.</param>
    /// <param name="authenticationDelegate">The injected authentication delegate.</param>
    /// <param name="messageSender">The change feed message sender.</param>
    /// <param name="auditRepository">The injected agent audit repository.</param>
    /// <param name="mappingService">The injected mapping service.</param>
#pragma warning disable S107 // The number of DI parameters should be ignored
    public class DelegationService(
        IConfiguration configuration,
        IPatientService patientService,
        IResourceDelegateDelegate resourceDelegateDelegate,
        IDelegationDelegate delegationDelegate,
        IAuthenticationDelegate authenticationDelegate,
        IMessageSender messageSender,
        IAuditRepository auditRepository,
        IAdminServerMappingService mappingService) : IDelegationService
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
            RequestResult<PatientModel> dependentPatientResult = await patientService.GetPatientAsync(phn, PatientIdentifierType.Phn, ct: ct);
            ValidatePatientResult(dependentPatientResult);

            await new DependentPatientValidator(this.maxDependentAge, $"Dependent age is above {this.maxDependentAge}")
                .ValidateAndThrowAsync(dependentPatientResult.ResourcePayload!, ct);

            DelegationInfo delegationInfo = new();
            if (dependentPatientResult.ResourcePayload != null)
            {
                PatientModel dependentPatientInfo = dependentPatientResult.ResourcePayload;
                DependentInfo dependentInfo = mappingService.MapToDependentInfo(dependentPatientInfo);
                delegationInfo.Dependent = dependentInfo;

                // Get delegates from database
                IEnumerable<ResourceDelegate> dbResourceDelegates = await this.SearchDelegatesAsync(dependentPatientInfo.HdId, ct);

                List<DelegateInfo> delegates = [];
                foreach (ResourceDelegate resourceDelegate in dbResourceDelegates)
                {
                    RequestResult<PatientModel> delegatePatientResult = await patientService.GetPatientAsync(resourceDelegate.ProfileHdid, ct: ct);
                    ValidatePatientResult(delegatePatientResult);

                    DelegateInfo delegateInfo = mappingService.MapToDelegateInfo(delegatePatientResult.ResourcePayload);
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
                        RequestResult<PatientModel> delegatePatientResult = await patientService.GetPatientAsync(allowedDelegation.DelegateHdId, ct: ct);

                        DelegateInfo delegateInfo = mappingService.MapToDelegateInfo(delegatePatientResult.ResourcePayload);
                        delegateInfo.DelegationStatus = DelegationStatus.Allowed;
                        delegates.Add(delegateInfo);
                    }
                }

                delegationInfo.Delegates = delegates;

                // Get agent audits
                AgentAuditQuery agentAuditQuery = new(dependentPatientInfo.HdId, AuditGroup.Dependent);
                IEnumerable<AgentAudit> agentAudits = await auditRepository.HandleAsync(agentAuditQuery, ct);
                delegationInfo.AgentActions = agentAudits.Select(mappingService.MapToAgentAction).ToList();
            }

            return delegationInfo;
        }

        /// <inheritdoc/>
        public async Task<DelegateInfo> GetDelegateInformationAsync(string phn, CancellationToken ct = default)
        {
            RequestResult<PatientModel> delegatePatientResult = await patientService.GetPatientAsync(phn, PatientIdentifierType.Phn, false, ct);
            ValidatePatientResult(delegatePatientResult);

            await new DelegatePatientValidator(this.minDelegateAge, $"Delegate age is below {this.minDelegateAge}")
                .ValidateAndThrowAsync(delegatePatientResult.ResourcePayload!, ct);

            DelegateInfo delegateInfo = mappingService.MapToDelegateInfo(delegatePatientResult.ResourcePayload);
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
            List<AllowedDelegation> allowedDelegationsToDelete = dependent.AllowedDelegations.Where(x => delegateHdidList.All(y => y != x.DelegateHdId)).ToList();

            // Compare passed in delegate hdids with dependent allowed delegations in database to determine what allowed delegations to add.
            List<AllowedDelegation> allowedDelegationsToAdd = delegateHdidList.Where(x => dependent.AllowedDelegations.All(y => y.DelegateHdId != x))
                .Select(
                    delegateHdid => new AllowedDelegation
                    {
                        DependentHdId = dependent.HdId,
                        DelegateHdId = delegateHdid,
                        CreatedBy = authenticatedUserId,
                        UpdatedBy = authenticatedUserId,
                    })
                .ToList();

            dependent.AllowedDelegations = dependent.AllowedDelegations.Except(allowedDelegationsToDelete).Concat(allowedDelegationsToAdd).ToList();

            IEnumerable<ResourceDelegate> resourceDelegates = await this.SearchDelegatesAsync(dependent.HdId, ct);

            // Compare resource delegates with passed in delegate hdids to determine which resource delegates to remove
            IEnumerable<ResourceDelegate> resourceDelegatesToDelete = resourceDelegates.Where(r => delegateHdidList.All(a => a != r.ProfileHdid)).ToList();

            // Update dependent, allow delegation and resource delegate in database
            if (this.changeFeedEnabled)
            {
                await delegationDelegate.UpdateDelegationAsync(dependent, resourceDelegatesToDelete, agentAudit, false, ct);
                IEnumerable<MessageEnvelope> events =
                [
                    new(new DependentProtectionAddedEvent(dependentHdid), dependentHdid),
                    .. resourceDelegatesToDelete.Select(rd => new MessageEnvelope(new DependentRemovedEvent(rd.ProfileHdid, dependentHdid), rd.ProfileHdid)),
                ];

                await messageSender.SendAsync(events, ct);
            }
            else
            {
                await delegationDelegate.UpdateDelegationAsync(dependent, resourceDelegatesToDelete, agentAudit, ct: ct);
            }

            return mappingService.MapToAgentAction(agentAudit);
        }

        /// <inheritdoc/>
        public async Task<AgentAction> UnprotectDependentAsync(string dependentHdid, string reason, CancellationToken ct = default)
        {
            string authenticatedUserId = authenticationDelegate.FetchAuthenticatedUserId() ?? UserId.DefaultUser;
            Dependent? dependent = await delegationDelegate.GetDependentAsync(dependentHdid, true, ct);

            if (dependent == null)
            {
                throw new NotFoundException($"Dependent not found for hdid: {dependentHdid}");
            }

            dependent.Protected = false;
            dependent.UpdatedBy = authenticatedUserId;
            dependent.AllowedDelegations = [];

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
                await delegationDelegate.UpdateDelegationAsync(dependent, [], agentAudit, false, ct);

                MessageEnvelope[] events =
                [
                    new(new DependentProtectionRemovedEvent(dependentHdid), dependentHdid),
                ];

                await messageSender.SendAsync(events, ct);
            }
            else
            {
                await delegationDelegate.UpdateDelegationAsync(dependent, [], agentAudit, ct: ct);
            }

            return mappingService.MapToAgentAction(agentAudit);
        }

        private static void ValidatePatientResult(RequestResult<PatientModel> patientResult)
        {
            switch (patientResult)
            {
                case { ResultStatus: ResultType.ActionRequired, ResultError: { ActionCode: { } } error } when error.ActionCode.Equals(ActionType.Validation):
                    throw new ValidationException(error.ResultMessage);
                case { ResultStatus: ResultType.Error, ResultError: { } error } when error.ResultMessage.StartsWith("Communication Exception", StringComparison.InvariantCulture):
                    throw new UpstreamServiceException(error.ResultMessage);
                case { ResultStatus: ResultType.Error or ResultType.ActionRequired } or { ResourcePayload: null }:
                    throw new NotFoundException("Patient not found");
            }
        }

        private async Task<IEnumerable<ResourceDelegate>> SearchDelegatesAsync(string ownerHdid, CancellationToken ct)
        {
            ResourceDelegateQuery query = new() { ByOwnerHdid = ownerHdid };
            ResourceDelegateQueryResult result = await resourceDelegateDelegate.SearchAsync(query, ct);
            return result.Items.Select(c => c.ResourceDelegate);
        }
    }
}
