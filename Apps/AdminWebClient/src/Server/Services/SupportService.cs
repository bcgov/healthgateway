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
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using HealthGateway.Admin.MapUtils;
    using HealthGateway.Admin.Models;
    using HealthGateway.Common.Constants;
    using HealthGateway.Common.Data.Constants;
    using HealthGateway.Common.Data.ErrorHandling;
    using HealthGateway.Common.Data.Models;
    using HealthGateway.Common.Factories;
    using HealthGateway.Common.Models;
    using HealthGateway.Common.Services;
    using HealthGateway.Database.Constants;
    using HealthGateway.Database.Delegates;
    using HealthGateway.Database.Models;
    using Microsoft.IdentityModel.Tokens;

    /// <inheritdoc/>
    public class SupportService : ISupportService
    {
        private readonly IMapper autoMapper;
        private readonly IMessagingVerificationDelegate messagingVerificationDelegate;
        private readonly IPatientService patientService;
        private readonly IResourceDelegateDelegate resourceDelegateDelegate;
        private readonly IUserProfileDelegate userProfileDelegate;

        /// <summary>
        /// Initializes a new instance of the <see cref="SupportService"/> class.
        /// </summary>
        /// <param name="userProfileDelegate">The user profile delegate to interact with the DB.</param>
        /// <param name="messagingVerificationDelegate">The Messaging verification delegate to interact with the DB.</param>
        /// <param name="patientService">The patient service to lookup HDIDs by PHN.</param>
        /// <param name="resourceDelegateDelegate">The resource delegate used to lookup delegates and owners.</param>
        /// <param name="autoMapper">The injected automapper provider.</param>
        public SupportService(
            IUserProfileDelegate userProfileDelegate,
            IMessagingVerificationDelegate messagingVerificationDelegate,
            IPatientService patientService,
            IResourceDelegateDelegate resourceDelegateDelegate,
            IMapper autoMapper)
        {
            this.userProfileDelegate = userProfileDelegate;
            this.messagingVerificationDelegate = messagingVerificationDelegate;
            this.patientService = patientService;
            this.resourceDelegateDelegate = resourceDelegateDelegate;
            this.autoMapper = autoMapper;
        }

        /// <inheritdoc/>
        public async Task<RequestResult<IEnumerable<MessagingVerificationModel>>> GetMessageVerificationsAsync(string hdid, CancellationToken ct = default)
        {
            IEnumerable<MessagingVerification> dbResult = await this.messagingVerificationDelegate.GetUserMessageVerificationsAsync(hdid, ct);
            RequestResult<IEnumerable<MessagingVerificationModel>> result = new()
            {
                ResultStatus = ResultType.Success,
                ResourcePayload = dbResult.Select(m => this.autoMapper.Map<MessagingVerification, MessagingVerificationModel>(m)),
            };
            return result;
        }

        /// <inheritdoc/>
        public async Task<RequestResult<IEnumerable<PatientSupportDetails>>> GetPatientsAsync(PatientQueryType queryType, string queryString, CancellationToken ct = default)
        {
            RequestResult<IEnumerable<PatientSupportDetails>> result = new()
            {
                ResultStatus = ResultType.Error,
                ResourcePayload = Enumerable.Empty<PatientSupportDetails>(),
            };

            switch (queryType)
            {
                case PatientQueryType.Phn:
                    await this.PopulatePatientSupportDetailsAsync(result, PatientIdentifierType.Phn, queryString, ct);
                    break;
                case PatientQueryType.Email:
                    await this.PopulatePatientSupportDetailsAsync(result, UserQueryType.Email, queryString, ct);
                    break;
                case PatientQueryType.Sms:
                    await this.PopulatePatientSupportDetailsAsync(result, UserQueryType.Sms, queryString, ct);
                    break;
                case PatientQueryType.Hdid:
                    await this.PopulatePatientSupportDetailsAsync(result, PatientIdentifierType.Hdid, queryString, ct);
                    break;
                case PatientQueryType.Dependent:
                    result = await this.SearchDelegatesAsync(queryString, ct);
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

        private async Task PopulatePatientSupportDetailsAsync(
            RequestResult<IEnumerable<PatientSupportDetails>> result,
            PatientIdentifierType patientIdentifierType,
            string queryString,
            CancellationToken ct)
        {
            RequestResult<PatientModel> patientResult = await this.patientService.GetPatientAsync(queryString, patientIdentifierType, ct: ct);
            if (patientResult.ResultStatus == ResultType.Success && patientResult.ResourcePayload != null)
            {
                List<PatientSupportDetails> patients = [];
                HealthGateway.Database.Models.UserProfile? userProfile = await this.userProfileDelegate.GetUserProfileAsync(patientResult.ResourcePayload.HdId, ct);
                if (userProfile != null)
                {
                    PatientSupportDetails patientSupportDetails = PatientSupportDetailsMapUtils.ToUiModel(userProfile, patientResult.ResourcePayload, this.autoMapper);
                    patients.Add(patientSupportDetails);
                    result.ResourcePayload = patients;
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

        private async Task PopulatePatientSupportDetailsAsync(RequestResult<IEnumerable<PatientSupportDetails>> result, UserQueryType queryType, string queryString, CancellationToken ct)
        {
            IEnumerable<HealthGateway.Database.Models.UserProfile> profiles = await this.userProfileDelegate.GetUserProfilesAsync(queryType, queryString, ct);
            result.ResourcePayload = this.autoMapper.Map<IEnumerable<PatientSupportDetails>>(profiles);
            result.ResultStatus = ResultType.Success;
        }

        private async Task<RequestResult<IEnumerable<PatientSupportDetails>>> SearchDelegatesAsync(string forOwnerByPhn, CancellationToken ct)
        {
            string ownerHdid = await this.patientService.GetPatientHdidAsync(forOwnerByPhn, ct);

            ResourceDelegateQuery query = new() { ByOwnerHdid = ownerHdid };
            ResourceDelegateQueryResult result = await this.resourceDelegateDelegate.SearchAsync(query, ct);

            return RequestResultFactory.Success(this.autoMapper.Map<IEnumerable<PatientSupportDetails>>(result.Items.Select(r => r.ResourceDelegate)));
        }
    }
}
