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
namespace HealthGateway.Common.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using AutoMapper;
    using HealthGateway.Common.Constants;
    using HealthGateway.Common.Data.Constants;
    using HealthGateway.Common.Data.ErrorHandling;
    using HealthGateway.Common.Data.Models;
    using HealthGateway.Common.Data.ViewModels;
    using HealthGateway.Common.Models;
    using HealthGateway.Database.Constants;
    using HealthGateway.Database.Delegates;
    using HealthGateway.Database.Models;
    using HealthGateway.Database.Wrapper;
    using Microsoft.IdentityModel.Tokens;
    using UserQueryType = HealthGateway.Common.Data.Constants.UserQueryType;

    /// <inheritdoc/>
    public class SupportService : ISupportService
    {
        private readonly IUserProfileDelegate userProfileDelegate;
        private readonly IMessagingVerificationDelegate messagingVerificationDelegate;
        private readonly IPatientService patientService;
        private readonly IMapper autoMapper;

        /// <summary>
        /// Initializes a new instance of the <see cref="SupportService"/> class.
        /// </summary>
        /// <param name="userProfileDelegate">The user profile delegate to interact with the DB.</param>
        /// <param name="messagingVerificationDelegate">The Messaging verification delegate to interact with the DB.</param>
        /// <param name="patientService">The patient service to lookup HDIDs by PHN.</param>
        /// <param name="autoMapper">The injected automapper provider.</param>
        public SupportService(
            IUserProfileDelegate userProfileDelegate,
            IMessagingVerificationDelegate messagingVerificationDelegate,
            IPatientService patientService,
            IMapper autoMapper)
        {
            this.userProfileDelegate = userProfileDelegate;
            this.messagingVerificationDelegate = messagingVerificationDelegate;
            this.patientService = patientService;
            this.autoMapper = autoMapper;
        }

        /// <inheritdoc/>
        public RequestResult<IEnumerable<MessagingVerificationModel>> GetMessageVerifications(string hdid)
        {
            DBResult<IEnumerable<MessagingVerification>> dbResult = this.messagingVerificationDelegate.GetUserMessageVerifications(hdid);
            IList<MessagingVerificationModel> verificationModels = this.autoMapper.Map<IList<MessagingVerificationModel>>(dbResult.Payload);
            RequestResult<IEnumerable<MessagingVerificationModel>> result = new()
            {
                ResultStatus = ResultType.Success,
                ResourcePayload = verificationModels,
            };
            return result;
        }

        /// <inheritdoc/>
        public RequestResult<IEnumerable<SupportUser>> GetUsers(UserQueryType queryType, string queryString)
        {
            RequestResult<IEnumerable<SupportUser>> result = new()
            {
                ResultStatus = ResultType.Error,
                ResourcePayload = Enumerable.Empty<SupportUser>(),
            };

            switch (queryType)
            {
                case UserQueryType.Phn:
                    this.PopulateSupportUser(result, PatientIdentifierType.PHN, queryString);
                    break;
                case UserQueryType.Email:
                    this.PopulateSupportUser(result, Database.Constants.UserQueryType.Email, queryString);
                    break;
                case UserQueryType.Sms:
                    this.PopulateSupportUser(result, Database.Constants.UserQueryType.SMS, queryString);
                    break;
                case UserQueryType.Hdid:
                    this.PopulateSupportUser(result, PatientIdentifierType.HDID, queryString);
                    break;
            }

            if (result.ResultError != null && result.ResultError.ActionCode != null && result.ResultError.ActionCode.Equals(ActionType.Warning))
            {
                result.ResultStatus = ResultType.ActionRequired;
            }

            return result;
        }

        private static RequestResultError GetWarningMessageForResponseCode(string resourcePayloadResponseCode)
        {
            string[] messageParts = resourcePayloadResponseCode.Split('|', StringSplitOptions.TrimEntries);

            RequestResultError error = new()
            {
                ErrorCode = messageParts[0],
                ResultMessage = messageParts[1],
                ActionCode = ActionType.Warning,
            };

            return error;
        }

        private void PopulateSupportUser(RequestResult<IEnumerable<SupportUser>> result, PatientIdentifierType patientIdentifierType, string queryString)
        {
            RequestResult<PatientModel> patientResult = Task.Run(async () => await this.patientService.GetPatient(queryString, patientIdentifierType).ConfigureAwait(true)).Result;
            if (patientResult.ResultStatus == ResultType.Success && patientResult.ResourcePayload != null)
            {
                List<SupportUser> supportUsers = new();
                DBResult<UserProfile> dbResult = this.userProfileDelegate.GetUserProfile(patientResult.ResourcePayload.HdId);
                if (dbResult.Status == DBStatusCode.Read)
                {
                    SupportUser supportUser = this.autoMapper.Map<SupportUser>(dbResult.Payload);
                    supportUser.PersonalHealthNumber = patientResult.ResourcePayload.PersonalHealthNumber;
                    supportUsers.Add(supportUser);
                    result.ResourcePayload = supportUsers;
                    result.ResultStatus = ResultType.Success;
                }
                else
                {
                    result.ResultError = new()
                    {
                        ResultMessage = $"Unable to find user profile for hdid: {patientResult.ResourcePayload.HdId}",
                    };
                }

                if (!patientResult.ResourcePayload.ResponseCode.IsNullOrEmpty())
                {
                    result.ResultError = GetWarningMessageForResponseCode(patientResult.ResourcePayload.ResponseCode);
                }
            }
            else
            {
                result.ResultError = patientResult.ResultError;
            }
        }

        private void PopulateSupportUser(RequestResult<IEnumerable<SupportUser>> result, Database.Constants.UserQueryType queryType, string queryString)
        {
            DBResult<List<UserProfile>> dbResult = this.userProfileDelegate.GetUserProfiles(queryType, queryString);
            result.ResourcePayload = this.autoMapper.Map<IEnumerable<SupportUser>>(dbResult.Payload);
            result.ResultStatus = ResultType.Success;
        }
    }
}
