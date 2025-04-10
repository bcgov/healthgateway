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
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using FluentValidation.Results;
    using HealthGateway.Common.Constants;
    using HealthGateway.Common.Data.Constants;
    using HealthGateway.Common.Data.ErrorHandling;
    using HealthGateway.Common.Data.Models;
    using HealthGateway.Common.ErrorHandling;
    using HealthGateway.Common.ErrorHandling.Exceptions;
    using HealthGateway.Common.Factories;
    using HealthGateway.Common.Messaging;
    using HealthGateway.Common.Models;
    using HealthGateway.Common.Models.Events;
    using HealthGateway.Common.Services;
    using HealthGateway.Database.Constants;
    using HealthGateway.Database.Delegates;
    using HealthGateway.Database.Models;
    using HealthGateway.Database.Wrapper;
    using HealthGateway.GatewayApi.Models;
    using HealthGateway.GatewayApi.Validations;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;

    /// <inheritdoc/>
    public class DependentService : IDependentService
    {
        private const string WebClientConfigSection = "WebClient";
        private const string MaxDependentAgeKey = "MaxDependentAge";
        private const string MonitoredHdidsKey = "MonitoredHdids";
        private const string SmartApostrophe = "’";
        private const string RegularApostrophe = "'";
        private readonly IGatewayApiMappingService mappingService;
        private readonly ILogger<DependentService> logger;
        private readonly int maxDependentAge;
        private readonly bool dependentsChangeFeedEnabled;
        private readonly string[] monitoredHdids;
        private readonly string adminEmailAddress;
        private readonly INotificationSettingsService notificationSettingsService;
        private readonly IPatientService patientService;
        private readonly IDelegationDelegate delegationDelegate;
        private readonly IResourceDelegateDelegate resourceDelegateDelegate;
        private readonly IUserProfileDelegate userProfileDelegate;
        private readonly IMessageSender messageSender;
        private readonly IEmailQueueService emailQueueService;

        /// <summary>
        /// Initializes a new instance of the <see cref="DependentService"/> class.
        /// </summary>
        /// <param name="configuration">The injected configuration.</param>
        /// <param name="logger">The injected logger.</param>
        /// <param name="patientService">The injected patient service.</param>
        /// <param name="notificationSettingsService">Notification settings service.</param>
        /// <param name="delegationDelegate">The delegation delegate.</param>
        /// <param name="resourceDelegateDelegate">The ResourceDelegate delegate.</param>
        /// <param name="userProfileDelegate">The user profile delegate.</param>
        /// <param name="messageSender">The message sender.</param>
        /// <param name="mappingService">The injected mapping service.</param>
        /// <param name="emailQueueService">The injected email queue service.</param>
#pragma warning disable S107 // The number of DI parameters should be ignored
        public DependentService(
            IConfiguration configuration,
            ILogger<DependentService> logger,
            IPatientService patientService,
            INotificationSettingsService notificationSettingsService,
            IDelegationDelegate delegationDelegate,
            IResourceDelegateDelegate resourceDelegateDelegate,
            IUserProfileDelegate userProfileDelegate,
            IMessageSender messageSender,
            IGatewayApiMappingService mappingService,
            IEmailQueueService emailQueueService)
        {
            this.logger = logger;
            this.patientService = patientService;
            this.notificationSettingsService = notificationSettingsService;
            this.delegationDelegate = delegationDelegate;
            this.resourceDelegateDelegate = resourceDelegateDelegate;
            this.userProfileDelegate = userProfileDelegate;
            this.messageSender = messageSender;
            this.maxDependentAge = configuration.GetSection(WebClientConfigSection).GetValue(MaxDependentAgeKey, 12);
            this.dependentsChangeFeedEnabled = configuration.GetSection(ChangeFeedOptions.ChangeFeed).Get<ChangeFeedOptions>()?.Dependents.Enabled ?? false;
            this.monitoredHdids = configuration.GetSection(MonitoredHdidsKey).Get<string[]>() ?? [];
            this.adminEmailAddress = configuration.GetSection(EmailTemplateConfig.ConfigurationSectionKey).Get<EmailTemplateConfig>()?.AdminEmail ?? string.Empty;
            this.mappingService = mappingService;
            this.emailQueueService = emailQueueService;
        }

        /// <inheritdoc/>
        public async Task<RequestResult<DependentModel>> AddDependentAsync(string delegateHdid, AddDependentRequest addDependentRequest, CancellationToken ct = default)
        {
            ValidationResult? validationResults = await new AddDependentRequestValidator(this.maxDependentAge).ValidateAsync(addDependentRequest, ct);

            if (!validationResults.IsValid)
            {
                this.logger.LogDebug("Dependent did not pass age validation");
                return RequestResultFactory.Error<DependentModel>(ErrorType.InvalidState, validationResults.Errors);
            }

            RequestResult<PatientModel> dependentResult = await this.GetDependentAsPatientAsync(addDependentRequest.Phn, ct);
            RequestResult<DependentModel>? validationResult = await this.ValidateDependentAsync(addDependentRequest, delegateHdid, dependentResult, ct);
            if (validationResult != null)
            {
                return validationResult;
            }

            string dependentHdid = dependentResult.ResourcePayload!.HdId;
            ResourceDelegate resourceDelegate = new()
            {
                ResourceOwnerHdid = dependentHdid,
                ProfileHdid = delegateHdid,
                ExpiryDate = DateOnly.FromDateTime(dependentResult.ResourcePayload.Birthdate.AddYears(this.maxDependentAge)),
                ReasonCode = ResourceDelegateReason.Guardian,
                ReasonObjectType = null,
                ReasonObject = null,
            };

            // commit to the database if change feed is disabled; if change feed enabled, commit will happen when message sender is called
            // this is temporary and will be changed when we introduce a proper unit of work to manage transactionality.
            DbResult<ResourceDelegate> dbDependent = await this.resourceDelegateDelegate.InsertAsync(resourceDelegate, !this.dependentsChangeFeedEnabled, ct);
            if (dbDependent.Status == DbStatusCode.Error)
            {
                throw new DatabaseException(dbDependent.Message);
            }

            await this.UpdateNotificationSettingsAsync(dependentHdid, delegateHdid, false, ct);
            if (this.dependentsChangeFeedEnabled)
            {
                await this.messageSender.SendAsync([new MessageEnvelope(new DependentAddedEvent(delegateHdid, dependentHdid), delegateHdid)], ct);
            }

            DbResult<Dictionary<string, int>> totalDelegateCounts = await this.resourceDelegateDelegate.GetTotalDelegateCountsAsync([dependentHdid], ct);
            int totalDelegateCount = totalDelegateCounts.Payload.GetValueOrDefault(dependentHdid);

            return RequestResultFactory.Success(this.mappingService.MapToDependentModel(dbDependent.Payload, dependentResult.ResourcePayload, totalDelegateCount));
        }

        /// <inheritdoc/>
        public async Task<RequestResult<IEnumerable<DependentModel>>> GetDependentsAsync(string hdid, int page = 0, int pageSize = 25, CancellationToken ct = default)
        {
            RequestResult<IEnumerable<DependentModel>> result = new()
            {
                ResultStatus = ResultType.Success,
            };

            // get dependents from database
            IList<ResourceDelegate> resourceDelegates = await this.resourceDelegateDelegate.GetAsync(hdid, page, pageSize, ct);

            DbResult<Dictionary<string, int>> totalDelegateCounts = await this.resourceDelegateDelegate.GetTotalDelegateCountsAsync(resourceDelegates.Select(d => d.ResourceOwnerHdid), ct);

            // get dependent details from patient service
            List<DependentModel> dependentModels = [];
            StringBuilder resultErrorMessage = new();
            foreach (ResourceDelegate resourceDelegate in resourceDelegates)
            {
                RequestResult<PatientModel> patientResult = await this.patientService.GetPatientAsync(resourceDelegate.ResourceOwnerHdid, ct: ct);

                if (patientResult.ResourcePayload != null)
                {
                    int totalDelegateCount = totalDelegateCounts.Payload.GetValueOrDefault(resourceDelegate.ResourceOwnerHdid);
                    dependentModels.Add(this.mappingService.MapToDependentModel(resourceDelegate, patientResult.ResourcePayload, totalDelegateCount));
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
        public async Task<RequestResult<IEnumerable<GetDependentResponse>>> GetDependentsAsync(DateTime fromDateUtc, DateTime? toDateUtc, int page, int pageSize, CancellationToken ct = default)
        {
            IList<ResourceDelegate> resourceDelegates = await this.resourceDelegateDelegate.GetAsync(
                fromDateUtc,
                toDateUtc,
                page,
                pageSize,
                ct);

            IEnumerable<GetDependentResponse> getDependentResponses =
                resourceDelegates
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
        public async Task<RequestResult<DependentModel>> RemoveAsync(string delegateHdid, string dependentHdid, CancellationToken ct = default)
        {
            ResourceDelegate resourceDelegate = (await this.resourceDelegateDelegate.GetAsync(delegateHdid, 0, 500, ct)).FirstOrDefault(d => d.ResourceOwnerHdid == dependentHdid) ??
                                                throw new NotFoundException($"Dependent {dependentHdid} not found for delegate {delegateHdid}");

            // commit to the database if change feed is disabled; if change feed enabled, commit will happen when message sender is called
            // this is temporary and will be changed when we introduce a proper unit of work to manage transactionality.
            DbResult<ResourceDelegate> dbDependent = await this.resourceDelegateDelegate.DeleteAsync(resourceDelegate, !this.dependentsChangeFeedEnabled, ct);
            if (dbDependent.Status == DbStatusCode.Error)
            {
                throw new DatabaseException(dbDependent.Message);
            }

            await this.UpdateNotificationSettingsAsync(dependentHdid, delegateHdid, true, ct);

            if (this.dependentsChangeFeedEnabled)
            {
                await this.messageSender.SendAsync([new MessageEnvelope(new DependentRemovedEvent(delegateHdid, dependentHdid), delegateHdid)], ct);
            }

            return RequestResultFactory.Success(new DependentModel());
        }

        private static bool IsMatch(AddDependentRequest? request, PatientModel? response)
        {
            return request != null &&
                   response != null &&
                   response.LastName.Equals(ReplaceSmartApostrophe(request.LastName), StringComparison.OrdinalIgnoreCase) &&
                   response.FirstName.Equals(ReplaceSmartApostrophe(request.FirstName), StringComparison.OrdinalIgnoreCase) &&
                   response.Birthdate.Year == request.DateOfBirth.Year &&
                   response.Birthdate.Month == request.DateOfBirth.Month &&
                   response.Birthdate.Day == request.DateOfBirth.Day;
        }

        private static string? ReplaceSmartApostrophe(string? value)
        {
            return string.IsNullOrEmpty(value) ? value : value.Replace(SmartApostrophe, RegularApostrophe, StringComparison.Ordinal);
        }

        private async Task NotifyAddDependentMismatch(string delegateHdid, AddDependentRequest request, PatientModel? response, CancellationToken ct)
        {
            if (string.IsNullOrEmpty(this.adminEmailAddress))
            {
                return;
            }

            Dictionary<string, string> keyValues = new()
            {
                ["delegateHdid"] = FormatValue(delegateHdid),
                ["requestPhn"] = FormatValue(request.Phn),
                ["responsePhn"] = FormatValue(response?.PersonalHealthNumber),
                ["requestFirstName"] = FormatValue(ReplaceSmartApostrophe(request.FirstName)),
                ["responseFirstName"] = FormatValue(response?.FirstName),
                ["requestLastName"] = FormatValue(ReplaceSmartApostrophe(request.LastName)),
                ["responseLastName"] = FormatValue(response?.LastName),
                ["requestBirthdate"] = FormatValue(request.DateOfBirth.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)),
                ["responseBirthdate"] = FormatValue(response?.Birthdate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)),
            };

            this.logger.LogDebug("Sending notification for add dependent mismatch");
            await this.emailQueueService.QueueNewEmailAsync(this.adminEmailAddress, "AdminAddDependentMismatch", keyValues, ct: ct);
            return;

            static string FormatValue(string? s)
            {
                return s == null ? "null" : $"\"{s}\"";
            }
        }

        [SuppressMessage("ReSharper", "CognitiveComplexity", Justification = "Team decision")]
        [SuppressMessage("Style", "IDE0010:Populate switch", Justification = "Team decision")]
        private async Task<RequestResult<DependentModel>?> ValidateDependentAsync(
            AddDependentRequest addDependentRequest,
            string delegateHdid,
            RequestResult<PatientModel> dependentResult,
            CancellationToken ct)
        {
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
                        this.logger.LogDebug("Dependent data does not match");
                        if (Array.Exists(this.monitoredHdids, hdid => string.Equals(hdid, delegateHdid, StringComparison.OrdinalIgnoreCase)))
                        {
                            await this.NotifyAddDependentMismatch(delegateHdid, addDependentRequest, dependentResult.ResourcePayload, ct);
                        }

                        return RequestResultFactory.ActionRequired<DependentModel>(ActionType.DataMismatch, ErrorMessages.DataMismatch);
                    }

                    // Verify dependent has HDID
                    if (string.IsNullOrEmpty(dependentResult.ResourcePayload?.HdId))
                    {
                        return RequestResultFactory.ActionRequired<DependentModel>(ActionType.NoHdId, ErrorMessages.InvalidServicesCard);
                    }

                    // Verify delegate is allowed to access dependent
                    Dependent? dependent = await this.delegationDelegate.GetDependentAsync(dependentResult.ResourcePayload?.HdId, true, ct);
                    if (dependent is { Protected: true } && dependent.AllowedDelegations.All(d => d.DelegateHdId != delegateHdid))
                    {
                        this.logger.LogDebug("Dependent is protected");
                        return RequestResultFactory.ActionRequired<DependentModel>(ActionType.Protected, ErrorMessages.CannotPerformAction);
                    }

                    return null;
            }
        }

        private async Task<RequestResult<PatientModel>> GetDependentAsPatientAsync(string phn, CancellationToken ct)
        {
            return await this.patientService.GetPatientAsync(phn, PatientIdentifierType.Phn, ct: ct);
        }

        private async Task UpdateNotificationSettingsAsync(string dependentHdid, string delegateHdid, bool isDelete = false, CancellationToken ct = default)
        {
            UserProfile delegateUserProfile = await this.userProfileDelegate.GetUserProfileAsync(delegateHdid, ct: ct) ??
                                              throw new NotFoundException(ErrorMessages.DelegateUserProfileNotFound);

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

            await this.notificationSettingsService.QueueNotificationSettingsAsync(request, ct);
        }
    }
}
