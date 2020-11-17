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
namespace HealthGateway.WebClient.Services
{
    using System.Collections.Generic;
    using System.Text;
    using System.Text.Json;
    using System.Threading.Tasks;
    using HealthGateway.Common.Constants;
    using HealthGateway.Common.ErrorHandling;
    using HealthGateway.Common.Models;
    using HealthGateway.Common.Services;
    using HealthGateway.Database.Constants;
    using HealthGateway.Database.Delegates;
    using HealthGateway.Database.Models;
    using HealthGateway.Database.Wrapper;
    using HealthGateway.WebClient.Models;
    using Microsoft.Extensions.Logging;

    /// <inheritdoc />
    public class DependentService : IDependentService
    {
        private readonly ILogger logger;
        private readonly IPatientService patientService;
        private readonly IResourceDelegateDelegate resourceDelegateDelegate;
        private readonly INotificationSettingsService notificationSettingsService;
        private readonly IUserProfileDelegate userProfileDelegate;

        /// <summary>
        /// Initializes a new instance of the <see cref="DependentService"/> class.
        /// </summary>
        /// <param name="logger">Injected Logger Provider.</param>
        /// <param name="userProfileDelegate">The profile delegate to interact with the DB.</param>
        /// <param name="patientService">The injected patient registry provider.</param>
        /// <param name="notificationSettingsService">Notification settings service.</param>
        /// <param name="resourceDelegateDelegate">The ResourceDelegate delegate to interact with the DB.</param>
        public DependentService(
            ILogger<DependentService> logger,
            IUserProfileDelegate userProfileDelegate,
            IPatientService patientService,
            INotificationSettingsService notificationSettingsService,
            IResourceDelegateDelegate resourceDelegateDelegate)
        {
            this.logger = logger;
            this.patientService = patientService;
            this.resourceDelegateDelegate = resourceDelegateDelegate;
            this.notificationSettingsService = notificationSettingsService;
            this.userProfileDelegate = userProfileDelegate;
        }

        /// <inheritdoc />
        public RequestResult<DependentModel> AddDependent(string delegateHdId, AddDependentRequest addDependentRequest)
        {
            this.logger.LogTrace($"Dependent hdid: {delegateHdId}");
            this.logger.LogDebug("Getting dependent details...");
            RequestResult<PatientModel> patientResult = Task.Run(async () => await this.patientService.GetPatient(addDependentRequest.PHN, PatientIdentifierType.PHN).ConfigureAwait(true)).Result;
            if (patientResult.ResourcePayload == null)
            {
                return new RequestResult<DependentModel>()
                {
                    ResultStatus = ResultType.Error,
                    ResultError = new RequestResultError() { ResultMessage = "Communication Exception when trying to retrieve the Dependent", ErrorCode = ErrorTranslator.ServiceError(ErrorType.CommunicationExternal, ServiceType.Patient) },
                };
            }

            this.logger.LogError($"Getting dependent details...{JsonSerializer.Serialize(patientResult)}");
            // Verify dependent's details entered by user
            if (!addDependentRequest.Equals(patientResult.ResourcePayload))
            {

                return new RequestResult<DependentModel>()
                {
                    ResultStatus = ResultType.Error,
                    ResultError = new RequestResultError() { ResultMessage = "The information you entered did not match. Please try again.", ErrorCode = ErrorTranslator.ServiceError(ErrorType.InvalidState, ServiceType.Patient) },
                };
            }
            else
            {
                // Insert Dependent to database
                var dependent = new ResourceDelegate() { ResourceOwnerHdid = patientResult.ResourcePayload.HdId, ProfileHdid = delegateHdId, ReasonCode = ResourceDelegateReason.COVIDLaboratoryResult };
                DBResult<ResourceDelegate> dbDependent = this.resourceDelegateDelegate.Insert(dependent, true);

                if (dbDependent.Status == DBStatusCode.Created)
                {
                    this.logger.LogDebug("Finished adding dependent");
                    this.UpdateNotificationSettings(dependent.ResourceOwnerHdid, delegateHdId);

                    return new RequestResult<DependentModel>()
                    {
                        ResourcePayload = DependentModel.CreateFromModels(dbDependent.Payload, patientResult.ResourcePayload),
                        ResultStatus = ResultType.Success,
                    };
                }
                else
                {
                    this.logger.LogError("Error adding dependent");
                    return new RequestResult<DependentModel>()
                    {
                        ResourcePayload = new DependentModel(),
                        ResultStatus = ResultType.Error,
                        ResultError = new RequestResultError() { ResultMessage = dbDependent.Message, ErrorCode = ErrorTranslator.ServiceError(ErrorType.CommunicationInternal, ServiceType.Database) },
                    };
                }
            }
        }

        /// <inheritdoc />
        public RequestResult<IEnumerable<DependentModel>> GetDependents(string hdId, int page, int pageSize)
        {
            // Get Dependents from database
            int offset = page * pageSize;
            DBResult<IEnumerable<ResourceDelegate>> dbResourceDelegates = this.resourceDelegateDelegate.Get(hdId, offset, pageSize);

            // Get Dependents Details from Patient service
            List<DependentModel> dependentModels = new List<DependentModel>();
            RequestResult<IEnumerable<DependentModel>> result = new RequestResult<IEnumerable<DependentModel>>()
            {
                ResultStatus = ResultType.Success,
            };
            StringBuilder resultErrorMessage = new StringBuilder();
            foreach (ResourceDelegate resourceDelegate in dbResourceDelegates.Payload)
            {
                this.logger.LogDebug($"Getting dependent details for Dependent hdid: {resourceDelegate.ResourceOwnerHdid} ...");
                RequestResult<PatientModel> patientResult = Task.Run(async () => await this.patientService.GetPatient(resourceDelegate.ResourceOwnerHdid, PatientIdentifierType.HDID).ConfigureAwait(true)).Result;

                if (patientResult.ResourcePayload != null)
                {
                    dependentModels.Add(DependentModel.CreateFromModels(resourceDelegate, patientResult.ResourcePayload));
                }
                else
                {
                    if (result.ResultStatus != ResultType.Error)
                    {
                        result.ResultStatus = ResultType.Error;
                        resultErrorMessage.Append($"Communication Exception when trying to retrieve Dependent(s) - HdId: {resourceDelegate.ResourceOwnerHdid};");
                    }
                    else
                    {
                        resultErrorMessage.Append($" HdId: {resourceDelegate.ResourceOwnerHdid};");
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
                result.ResultError = new RequestResultError() { ResultMessage = resultErrorMessage.ToString(), ErrorCode = ErrorTranslator.ServiceError(ErrorType.CommunicationExternal, ServiceType.Patient) };
            }

            return result;
        }

        /// <inheritdoc />
        public RequestResult<DependentModel> Remove(DependentModel dependent)
        {
            DBResult<ResourceDelegate> dbDependent = this.resourceDelegateDelegate.Delete(dependent.ToDBModel(), true);

            if (dbDependent.Status == DBStatusCode.Deleted)
            {
                this.UpdateNotificationSettings(dependent.OwnerId, dependent.DelegateId, isDelete: true);
            }

            RequestResult<DependentModel> result = new RequestResult<DependentModel>()
            {
                ResourcePayload = new DependentModel(),
                ResultStatus = dbDependent.Status == DBStatusCode.Deleted ? ResultType.Success : ResultType.Error,
                ResultError = dbDependent.Status == DBStatusCode.Deleted ? null : new RequestResultError() { ResultMessage = dbDependent.Message, ErrorCode = ErrorTranslator.ServiceError(ErrorType.CommunicationInternal, ServiceType.Database) },
            };
            return result;
        }

        private NotificationSettingsRequest UpdateNotificationSettings(string dependentHdid, string delegateHdid, bool isDelete = false)
        {
            DBResult<UserProfile> dbResult = this.userProfileDelegate.GetUserProfile(delegateHdid);
            UserProfile delegateUserProfile = dbResult.Payload;

            // Update the notification settings
            NotificationSettingsRequest request = new NotificationSettingsRequest(delegateUserProfile, delegateUserProfile.Email, delegateUserProfile.SMSNumber);
            request.SubjectHdid = dependentHdid;
            if (isDelete)
            {
                request.EmailAddress = null;
                request.EmailEnabled = false;
                request.SMSNumber = null;
                request.SMSEnabled = false;
                request.SMSVerified = false;
            }

            this.notificationSettingsService.QueueNotificationSettings(request);
            return request;
        }
    }
}
