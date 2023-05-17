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
namespace HealthGateway.GatewayApi.Services
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using FluentValidation.Results;
    using HealthGateway.Common.Constants;
    using HealthGateway.Common.Data.Constants;
    using HealthGateway.Common.Data.ErrorHandling;
    using HealthGateway.Common.Data.Models;
    using HealthGateway.Common.Data.ViewModels;
    using HealthGateway.Common.ErrorHandling;
    using HealthGateway.Common.Factories;
    using HealthGateway.Common.Messaging;
    using HealthGateway.Common.Models;
    using HealthGateway.Common.Models.Events;
    using HealthGateway.Common.Services;
    using HealthGateway.Database.Constants;
    using HealthGateway.Database.Delegates;
    using HealthGateway.Database.Models;
    using HealthGateway.Database.Wrapper;
    using HealthGateway.GatewayApi.MapUtils;
    using HealthGateway.GatewayApi.Models;
    using HealthGateway.GatewayApi.Validations;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;

    /// <inheritdoc/>
    public class DependentService : IDependentService
    {
        private const string WebClientConfigSection = "WebClient";
        private const string MaxDependentAgeKey = "MaxDependentAge";
        private const string SmartApostrophe = "’";
        private const string RegularApostrophe = "'";
        private readonly IMapper autoMapper;
        private readonly IConfiguration configuration;
        private readonly ILogger logger;
        private readonly int maxDependentAge;
        private readonly INotificationSettingsService notificationSettingsService;
        private readonly IPatientService patientService;
        private readonly IDelegationDelegate delegationDelegate;
        private readonly IResourceDelegateDelegate resourceDelegateDelegate;
        private readonly IUserProfileDelegate userProfileDelegate;
        private readonly IMessageSender messageSender;

        /// <summary>
        /// Initializes a new instance of the <see cref="DependentService"/> class.
        /// </summary>
        /// <param name="configuration">The injected configuration provider.</param>
        /// <param name="logger">Injected Logger Provider.</param>
        /// <param name="patientService">The injected patient registry provider.</param>
        /// <param name="notificationSettingsService">Notification settings service.</param>
        /// <param name="delegationDelegate">The delegation delegate to interact with the DB.</param>
        /// <param name="resourceDelegateDelegate">The ResourceDelegate delegate to interact with the DB.</param>
        /// <param name="userProfileDelegate">The profile delegate to interact with the DB.</param>
        /// <param name="messageSender">The message sender.</param>
        /// <param name="autoMapper">The inject automapper provider.</param>
        public DependentService(
            IConfiguration configuration,
            ILogger<DependentService> logger,
            IPatientService patientService,
            INotificationSettingsService notificationSettingsService,
            IDelegationDelegate delegationDelegate,
            IResourceDelegateDelegate resourceDelegateDelegate,
            IUserProfileDelegate userProfileDelegate,
            IMessageSender messageSender,
            IMapper autoMapper)
        {
            this.configuration = configuration;
            this.logger = logger;
            this.patientService = patientService;
            this.notificationSettingsService = notificationSettingsService;
            this.delegationDelegate = delegationDelegate;
            this.resourceDelegateDelegate = resourceDelegateDelegate;
            this.userProfileDelegate = userProfileDelegate;
            this.messageSender = messageSender;
            this.maxDependentAge = configuration.GetSection(WebClientConfigSection).GetValue(MaxDependentAgeKey, 12);
            this.autoMapper = autoMapper;
        }

        /// <inheritdoc/>
        public async Task<RequestResult<DependentModel>> AddDependentAsync(string delegateHdId, AddDependentRequest addDependentRequest, CancellationToken ct = default)
        {
            ValidationResult? validationResults = new AddDependentRequestValidator(this.maxDependentAge).Validate(addDependentRequest);

            if (!validationResults.IsValid)
            {
                return RequestResultFactory.Error<DependentModel>(ErrorType.InvalidState, validationResults.Errors);
            }

            RequestResult<PatientModel> dependentResult = await this.patientService.GetPatient(addDependentRequest.Phn, PatientIdentifierType.Phn);
            switch (dependentResult.ResultStatus)
            {
                case ResultType.Error:
                    return RequestResultFactory.ServiceError<DependentModel>(ErrorType.CommunicationExternal, ServiceType.Patient, "Communication Exception when trying to retrieve the Dependent");

                case ResultType.ActionRequired when dependentResult.ResultError?.ActionCodeValue == ActionType.NoHdId.Value:
                    return RequestResultFactory.ActionRequired<DependentModel>(ActionType.NoHdId, ErrorMessages.InvalidServicesCard);

                case ResultType.ActionRequired:
                    return RequestResultFactory.ActionRequired<DependentModel>(ActionType.DataMismatch, ErrorMessages.DataMismatch);

                default:
                    if (!IsMatch(addDependentRequest, dependentResult.ResourcePayload))
                    {
                        return RequestResultFactory.ActionRequired<DependentModel>(ActionType.DataMismatch, ErrorMessages.DataMismatch);
                    }

                    // Verify dependent has HDID
                    if (string.IsNullOrEmpty(dependentResult.ResourcePayload?.HdId))
                    {
                        return RequestResultFactory.ActionRequired<DependentModel>(ActionType.NoHdId, ErrorMessages.InvalidServicesCard);
                    }

                    // Verify delegate is allowed to access dependent
                    Dependent? dependent = await this.delegationDelegate.GetDependentAsync(dependentResult.ResourcePayload?.HdId, true);
                    if (dependent is { Protected: true } && dependent.AllowedDelegations.All(d => d.DelegateHdId != delegateHdId))
                    {
                        return RequestResultFactory.ActionRequired<DependentModel>(ActionType.Protected, ErrorMessages.CannotPerformAction);
                    }

                    break;
            }

            var dependentHdid = dependentResult.ResourcePayload.HdId;
            var dbDependent = this.CreateDependent(dependentHdid, delegateHdId, dependentResult.ResourcePayload.Birthdate);
            this.UpdateNotificationSettings(dependentHdid, delegateHdId);
            await this.messageSender.SendAsync(new[] { new MessageEnvelope(new DependentAddedEvent(delegateHdId, dependentHdid), delegateHdId) }, ct);

            DbResult<Dictionary<string, int>> totalDelegateCounts = await this.resourceDelegateDelegate.GetTotalDelegateCountsAsync(new List<string> { dependentHdid });
            int totalDelegateCount = totalDelegateCounts.Payload.GetValueOrDefault(dependentHdid);

            return RequestResultFactory.Success(DependentMapUtils.CreateFromDbModels(dbDependent.Payload, dependentResult.ResourcePayload, totalDelegateCount, this.autoMapper));
        }

        /// <inheritdoc/>
        public async Task<RequestResult<IEnumerable<DependentModel>>> GetDependentsAsync(string hdId, int page = 0, int pageSize = 500)
        {
            RequestResult<IEnumerable<DependentModel>> result = new()
            {
                ResultStatus = ResultType.Success,
            };

            // Get Dependents from database
            DbResult<IEnumerable<ResourceDelegate>> dbResourceDelegates = this.resourceDelegateDelegate.Get(hdId, page, pageSize);

            DbResult<Dictionary<string, int>> totalDelegateCounts = await this.resourceDelegateDelegate.GetTotalDelegateCountsAsync(dbResourceDelegates.Payload.Select(d => d.ResourceOwnerHdid));

            // Get Dependents Details from Patient service
            List<DependentModel> dependentModels = new();
            StringBuilder resultErrorMessage = new();
            foreach (ResourceDelegate resourceDelegate in dbResourceDelegates.Payload)
            {
                this.logger.LogDebug("Getting dependent details for Dependent hdid: {DependentHdid}", resourceDelegate.ResourceOwnerHdid);
                RequestResult<PatientModel> patientResult = await this.patientService.GetPatient(resourceDelegate.ResourceOwnerHdid);

                if (patientResult.ResourcePayload != null)
                {
                    int totalDelegateCount = totalDelegateCounts.Payload.GetValueOrDefault(resourceDelegate.ResourceOwnerHdid);
                    dependentModels.Add(DependentMapUtils.CreateFromDbModels(resourceDelegate, patientResult.ResourcePayload, totalDelegateCount, this.autoMapper));
                }
                else
                {
                    if (result.ResultStatus != ResultType.Error)
                    {
                        result.ResultStatus = ResultType.Error;
                        resultErrorMessage.Append(CultureInfo.InvariantCulture, $"Communication Exception when trying to retrieve Dependent(s) - HdId: {resourceDelegate.ResourceOwnerHdid};");
                    }
                    else
                    {
                        resultErrorMessage.Append(CultureInfo.InvariantCulture, $" HdId: {resourceDelegate.ResourceOwnerHdid};");
                    }
                }
            }

            result.ResourcePayload = dependentModels;
            if (result.ResultStatus != ResultType.Error)
            {
                result.ResultError = null;
                result.TotalResultCount = dependentModels.Count;
            }
            else
            {
                result.ResultError = new RequestResultError
                {
                    ResultMessage = resultErrorMessage.ToString(),
                    ErrorCode = ErrorTranslator.ServiceError(ErrorType.CommunicationExternal, ServiceType.Patient),
                };
            }

            return result;
        }

        /// <inheritdoc/>
        public RequestResult<IEnumerable<GetDependentResponse>> GetDependents(DateTime fromDateUtc, DateTime? toDateUtc, int page, int pageSize)
        {
            DbResult<IEnumerable<ResourceDelegate>> dbResourceDelegates = this.resourceDelegateDelegate.Get(
                fromDateUtc,
                toDateUtc,
                page,
                pageSize);

            if (dbResourceDelegates.Status != DbStatusCode.Read)
            {
                return RequestResultFactory.ServiceError<IEnumerable<GetDependentResponse>>(ErrorType.CommunicationInternal, ServiceType.Database, dbResourceDelegates.Message);
            }

            IEnumerable<GetDependentResponse> getDependentResponses =
                dbResourceDelegates.Payload
                    .Select(
                        g => new GetDependentResponse
                        {
                            DelegateId = g.ProfileHdid,
                            OwnerId = g.ResourceOwnerHdid,
                            CreationDateTime = g.CreatedDateTime,
                        })
                    .ToList();

            return RequestResultFactory.Success(getDependentResponses, getDependentResponses.Count(), page, pageSize);
        }

        /// <inheritdoc/>
        public async Task<RequestResult<DependentModel>> RemoveAsync(DependentModel dependent, CancellationToken ct = default)
        {
            var resourceDelegate = this.resourceDelegateDelegate.Get(dependent.DelegateId).Payload.FirstOrDefault(d => d.ResourceOwnerHdid == dependent.OwnerId);
            if (resourceDelegate == null)
            {
                throw new ProblemDetailsException(ExceptionUtility.CreateNotFoundError($"Dependent {dependent.OwnerId} not found for delegate {dependent.DelegateId}"));
            }

            this.RemoveDependent(resourceDelegate);

            this.UpdateNotificationSettings(dependent.OwnerId, dependent.DelegateId, true);
            await this.messageSender.SendAsync(new[] { new MessageEnvelope(new DependentRemovedEvent(dependent.DelegateId, dependent.OwnerId), dependent.DelegateId) }, ct);

            return RequestResultFactory.Success(new DependentModel());
        }

        private static bool IsMatch(AddDependentRequest? dependent, PatientModel? patientModel)
        {
            if (dependent == null || patientModel == null)
            {
                return false;
            }

            if (!patientModel.LastName.Equals(ReplaceSmartApostrophe(dependent.LastName), StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            if (!patientModel.FirstName.Equals(ReplaceSmartApostrophe(dependent.FirstName), StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            if (patientModel.Birthdate.Year != dependent.DateOfBirth.Year ||
                patientModel.Birthdate.Month != dependent.DateOfBirth.Month ||
                patientModel.Birthdate.Day != dependent.DateOfBirth.Day)
            {
                return false;
            }

            return true;
        }

        private static string ReplaceSmartApostrophe(string value)
        {
            string replacedValue = value.Replace(SmartApostrophe, RegularApostrophe, StringComparison.Ordinal);
            return replacedValue;
        }

        private DbResult<ResourceDelegate> CreateDependent(string dependentHdid, string delegateHdid, DateTime dateOfBirth)
        {
            // Insert Dependent to database
            ResourceDelegate resourceDelegate = new()
            {
                ResourceOwnerHdid = dependentHdid,
                ProfileHdid = delegateHdid,
                ExpiryDate = DateOnly.FromDateTime(dateOfBirth.AddYears(this.maxDependentAge)),
                ReasonCode = ResourceDelegateReason.Guardian,
                ReasonObjectType = null,
                ReasonObject = null,
            };
            DbResult<ResourceDelegate> dbDependent = this.resourceDelegateDelegate.Insert(resourceDelegate, false);
            if (dbDependent.Status == DbStatusCode.Error)
            {
                throw new ProblemDetailsException(ExceptionUtility.CreateServerError($"{ServiceType.Database}:{ErrorType.CommunicationInternal}", dbDependent.Message));
            }

            return dbDependent;
        }

        private void RemoveDependent(ResourceDelegate dependent)
        {
            DbResult<ResourceDelegate> dbDependent = this.resourceDelegateDelegate.Delete(dependent, false);
            if (dbDependent.Status == DbStatusCode.Error)
            {
                throw new ProblemDetailsException(ExceptionUtility.CreateServerError($"{ServiceType.Database}:{ErrorType.CommunicationInternal}", dbDependent.Message));
            }
        }

        private void UpdateNotificationSettings(string dependentHdid, string delegateHdid, bool isDelete = false)
        {
            DbResult<UserProfile> dbResult = this.userProfileDelegate.GetUserProfile(delegateHdid);
            UserProfile delegateUserProfile = dbResult.Payload;

            // Update the notification settings
            NotificationSettingsRequest request = new(delegateUserProfile, delegateUserProfile.Email, delegateUserProfile.SmsNumber)
            {
                SubjectHdid = dependentHdid,
            };

            if (isDelete)
            {
                request.EmailAddress = null;
                request.EmailEnabled = false;
                request.SmsNumber = null;
                request.SmsEnabled = false;
                request.SmsVerified = false;
            }

            this.notificationSettingsService.QueueNotificationSettings(request);
        }
    }
}
