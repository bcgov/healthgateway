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
namespace HealthGateway.Admin.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using HealthGateway.Common.Constants;
    using HealthGateway.Common.Data.Constants;
    using HealthGateway.Common.Data.Models;
    using HealthGateway.Common.Data.Models.ErrorHandling;
    using HealthGateway.Common.Data.ViewModels;
    using HealthGateway.Common.Models;
    using HealthGateway.Common.Services;
    using HealthGateway.Database.Constants;
    using HealthGateway.Database.Delegates;
    using HealthGateway.Database.Wrapper;
    using Microsoft.IdentityModel.Tokens;
    using UserQueryType = HealthGateway.Admin.Constants.UserQueryType;

    /// <inheritdoc />
    public class SupportService : ISupportService
    {
        private readonly IMessagingVerificationDelegate messagingVerificationDelegate;
        private readonly IPatientService patientService;

        /// <summary>
        /// Initializes a new instance of the <see cref="SupportService" /> class.
        /// </summary>
        /// <param name="messagingVerificationDelegate">The Messaging verification delegate to interact with the DB.</param>
        /// <param name="patientService">The patient service to lookup HDIDs by PHN.</param>
        public SupportService(
            IMessagingVerificationDelegate messagingVerificationDelegate,
            IPatientService patientService)
        {
            this.messagingVerificationDelegate = messagingVerificationDelegate;
            this.patientService = patientService;
        }

        /// <inheritdoc />
        public RequestResult<IEnumerable<MessagingVerificationModel>> GetMessageVerifications(UserQueryType queryType, string queryString)
        {
            RequestResult<IEnumerable<MessagingVerificationModel>> retVal = new()
            {
                ResultStatus = ResultType.Error,
                ResourcePayload = Enumerable.Empty<MessagingVerificationModel>(),
            };

            DBResult<IEnumerable<MessagingVerification>>? dbResult = null;
            string phn = string.Empty;
            switch (queryType)
            {
                case UserQueryType.Phn:
                    RequestResult<PatientModel> patientResult = Task.Run(async () => await this.patientService.GetPatient(queryString, PatientIdentifierType.PHN).ConfigureAwait(true)).Result;
                    if (patientResult.ResultStatus == ResultType.Success && patientResult.ResourcePayload != null)
                    {
                        string hdid = patientResult.ResourcePayload.HdId;
                        phn = patientResult.ResourcePayload.PersonalHealthNumber;
                        dbResult = this.messagingVerificationDelegate.GetUserMessageVerifications(Database.Constants.UserQueryType.HDID, hdid);
                        if (!patientResult.ResourcePayload.ResponseCode.IsNullOrEmpty())
                        {
                            retVal.ResultError = GetWarningMessageForResponseCode(patientResult.ResourcePayload.ResponseCode);
                        }
                    }
                    else
                    {
                        retVal.ResultError = patientResult.ResultError;
                    }

                    break;
                case UserQueryType.Email:
                    dbResult = this.messagingVerificationDelegate.GetUserMessageVerifications(Database.Constants.UserQueryType.Email, queryString);
                    break;
                case UserQueryType.Sms:
                    dbResult = this.messagingVerificationDelegate.GetUserMessageVerifications(Database.Constants.UserQueryType.SMS, queryString);
                    break;
                case UserQueryType.Hdid:
                    RequestResult<PatientModel> patientResultHdid = Task.Run(async () => await this.patientService.GetPatient(queryString).ConfigureAwait(true)).Result;
                    if (patientResultHdid.ResultStatus == ResultType.Success && patientResultHdid.ResourcePayload != null)
                    {
                        phn = patientResultHdid.ResourcePayload.PersonalHealthNumber;
                        dbResult = this.messagingVerificationDelegate.GetUserMessageVerifications(Database.Constants.UserQueryType.HDID, queryString);
                        if (!patientResultHdid.ResourcePayload.ResponseCode.IsNullOrEmpty())
                        {
                            retVal.ResultError = GetWarningMessageForResponseCode(patientResultHdid.ResourcePayload.ResponseCode);
                        }
                    }
                    else
                    {
                        retVal.ResultError = patientResultHdid.ResultError;
                    }

                    break;
            }

            if (dbResult != null && dbResult.Status == DBStatusCode.Read)
            {
                retVal.ResultStatus = ResultType.Success;
                if (retVal.ResultError != null)
                {
                    retVal.ResultStatus = ResultType.ActionRequired;
                }

                List<MessagingVerificationModel> results = new();
                results.AddRange(dbResult.Payload.Select(MessagingVerificationModel.CreateFromDbModel));
                if (queryType == UserQueryType.Hdid || queryType == UserQueryType.Phn)
                {
                    foreach (MessagingVerificationModel item in results)
                    {
                        item.PersonalHealthNumber = phn;
                    }
                }

                retVal.ResourcePayload = results;
            }

            return retVal;
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
    }
}
