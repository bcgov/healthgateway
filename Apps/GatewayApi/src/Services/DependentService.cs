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
    using System.Text;
    using System.Threading.Tasks;
    using AutoMapper;
    using HealthGateway.Common.Constants;
    using HealthGateway.Common.Data.Constants;
    using HealthGateway.Common.Data.ErrorHandling;
    using HealthGateway.Common.Data.Models;
    using HealthGateway.Common.Data.ViewModels;
    using HealthGateway.Common.ErrorHandling;
    using HealthGateway.Common.Factories;
    using HealthGateway.Common.Models;
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
        private readonly IMapper autoMapper;
        private readonly ILogger logger;
        private readonly int maxDependentAge;
        private readonly INotificationSettingsService notificationSettingsService;
        private readonly IPatientService patientService;
        private readonly IResourceDelegateDelegate resourceDelegateDelegate;
        private readonly IUserProfileDelegate userProfileDelegate;

        /// <summary>
        /// Initializes a new instance of the <see cref="DependentService"/> class.
        /// </summary>
        /// <param name="configuration">The injected configuration provider.</param>
        /// <param name="logger">Injected Logger Provider.</param>
        /// <param name="userProfileDelegate">The profile delegate to interact with the DB.</param>
        /// <param name="patientService">The injected patient registry provider.</param>
        /// <param name="notificationSettingsService">Notification settings service.</param>
        /// <param name="resourceDelegateDelegate">The ResourceDelegate delegate to interact with the DB.</param>
        /// <param name="autoMapper">The inject automapper provider.</param>
        public DependentService(
            IConfiguration configuration,
            ILogger<DependentService> logger,
            IUserProfileDelegate userProfileDelegate,
            IPatientService patientService,
            INotificationSettingsService notificationSettingsService,
            IResourceDelegateDelegate resourceDelegateDelegate,
            IMapper autoMapper)
        {
            this.logger = logger;
            this.patientService = patientService;
            this.resourceDelegateDelegate = resourceDelegateDelegate;
            this.notificationSettingsService = notificationSettingsService;
            this.userProfileDelegate = userProfileDelegate;
            this.maxDependentAge = configuration.GetSection(WebClientConfigSection).GetValue(MaxDependentAgeKey, 12);
            this.autoMapper = autoMapper;
        }

        /// <inheritdoc/>
        public RequestResult<DependentModel> AddDependent(string delegateHdId, AddDependentRequest addDependentRequest)
        {
            var validationResults = new AddDependentRequestValidator(this.maxDependentAge).Validate(addDependentRequest);

            if (!validationResults.IsValid)
            {
                return RequestResultFactory.Error<DependentModel>(ErrorType.InvalidState, validationResults.Errors);
            }

            var patientResult = Task.Run(async () => await this.patientService.GetPatient(addDependentRequest.Phn, PatientIdentifierType.Phn).ConfigureAwait(true)).Result;
            switch (patientResult.ResultStatus)
            {
                case ResultType.Error:
                    return RequestResultFactory.ServiceError<DependentModel>(ErrorType.CommunicationExternal, ServiceType.Patient, "Communication Exception when trying to retrieve the Dependent");

                case ResultType.ActionRequired:
                    return RequestResultFactory.ActionRequired<DependentModel>(ActionType.DataMismatch, ErrorMessages.DataMismatch);

                default:
                    if (!IsMatch(addDependentRequest, patientResult.ResourcePayload))
                    {
                        return RequestResultFactory.ActionRequired<DependentModel>(ActionType.DataMismatch, ErrorMessages.DataMismatch);
                    }

                    // Verify dependent has HDID
                    else if (string.IsNullOrEmpty(patientResult.ResourcePayload.HdId))
                    {
                        return RequestResultFactory.ActionRequired<DependentModel>(ActionType.NoHdId, ErrorMessages.InvalidServicesCard);
                    }

                    break;
            }

            // Insert Dependent to database
            var dependent = new ResourceDelegate()
            {
                ResourceOwnerHdid = patientResult.ResourcePayload.HdId,
                ProfileHdid = delegateHdId,
                ReasonCode = ResourceDelegateReason.Guardian,
                ReasonObjectType = null,
                ReasonObject = null,
            };
            var dbDependent = this.resourceDelegateDelegate.Insert(dependent, true);

            if (dbDependent.Status != DbStatusCode.Created)
            {
                return RequestResultFactory.ServiceError<DependentModel>(ErrorType.CommunicationInternal, ServiceType.Database, dbDependent.Message);
            }

            this.UpdateNotificationSettings(dependent.ResourceOwnerHdid, delegateHdId);

            return RequestResultFactory.Success<DependentModel>(this.FromModels(dbDependent.Payload, patientResult.ResourcePayload));
        }

        /// <inheritdoc/>
        public RequestResult<IEnumerable<DependentModel>> GetDependents(string hdId, int page = 0, int pageSize = 500)
        {
            // Get Dependents from database
            int offset = page * pageSize;
            DbResult<IEnumerable<ResourceDelegate>> dbResourceDelegates = this.resourceDelegateDelegate.Get(hdId, offset, pageSize);

            // Get Dependents Details from Patient service
            List<DependentModel> dependentModels = new();
            RequestResult<IEnumerable<DependentModel>> result = new()
            {
                ResultStatus = ResultType.Success,
            };
            StringBuilder resultErrorMessage = new();
            foreach (ResourceDelegate resourceDelegate in dbResourceDelegates.Payload)
            {
                this.logger.LogDebug("Getting dependent details for Dependent hdid: {DependentHdid}", resourceDelegate.ResourceOwnerHdid);
                RequestResult<PatientModel> patientResult = Task.Run(async () => await this.patientService.GetPatient(resourceDelegate.ResourceOwnerHdid).ConfigureAwait(true)).Result;

                if (patientResult.ResourcePayload != null)
                {
                    dependentModels.Add(this.FromModels(resourceDelegate, patientResult.ResourcePayload));
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
                result.ResultStatus = ResultType.Success;
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
        public RequestResult<DependentModel> Remove(DependentModel dependent)
        {
            DbResult<ResourceDelegate> dbDependent = this.resourceDelegateDelegate.Delete(this.autoMapper.Map<ResourceDelegate>(dependent), true);

            if (dbDependent.Status == DbStatusCode.Deleted)
            {
                this.UpdateNotificationSettings(dependent.OwnerId, dependent.DelegateId, true);
            }

            RequestResult<DependentModel> result = new()
            {
                ResourcePayload = new DependentModel(),
                ResultStatus = dbDependent.Status == DbStatusCode.Deleted ? ResultType.Success : ResultType.Error,
                ResultError = dbDependent.Status == DbStatusCode.Deleted
                    ? null
                    : new RequestResultError
                    {
                        ResultMessage = dbDependent.Message,
                        ErrorCode = ErrorTranslator.ServiceError(ErrorType.CommunicationInternal, ServiceType.Database),
                    },
            };
            return result;
        }

        private static bool IsMatch(AddDependentRequest dependent, PatientModel patientModel)
        {
            if (dependent == null || patientModel == null)
            {
                return false;
            }

            if (!patientModel.LastName.Equals(dependent.LastName, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            if (!patientModel.FirstName.Equals(dependent.FirstName, StringComparison.OrdinalIgnoreCase))
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

        private DependentModel FromModels(ResourceDelegate resourceDelegate, PatientModel patientModel)
        {
            DependentModel dependent = this.autoMapper.Map<DependentModel>(resourceDelegate);
            dependent.DependentInformation = this.autoMapper.Map<DependentInformation>(patientModel);

            return dependent;
        }
    }
}
