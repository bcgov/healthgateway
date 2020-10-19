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
    using System;
    using System.Collections.Generic;
<<<<<<< HEAD
    using System.Text;
=======
    using System.Threading.Tasks;
>>>>>>> e1182ee320d2100e41815da38f5b743087dfbc07
    using HealthGateway.Common.Constants;
    using HealthGateway.Common.ErrorHandling;
    using HealthGateway.Common.Models;
    using HealthGateway.Common.Services;
    using HealthGateway.Database.Constants;
    using HealthGateway.Database.Delegates;
    using HealthGateway.Database.Models;
    using HealthGateway.Database.Wrapper;
    using HealthGateway.WebClient.Models;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Logging;

    /// <inheritdoc />
    public class DependentService : IDependentService
    {
        private readonly ILogger logger;
        private readonly IPatientService patientService;
        private readonly IUserDelegateDelegate userDelegateDelegate;

        /// <summary>
        /// Initializes a new instance of the <see cref="DependentService"/> class.
        /// </summary>
        /// <param name="logger">Injected Logger Provider.</param>
        /// <param name="patientService">The injected patient registry provider.</param>
        /// <param name="userDelegateDelegate">The User Delegate delegate to interact with the DB.</param>
        public DependentService(
            ILogger<DependentService> logger,
            IPatientService patientService,
            IUserDelegateDelegate userDelegateDelegate)
        {
            this.logger = logger;
            this.patientService = patientService;
            this.userDelegateDelegate = userDelegateDelegate;
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

            // Verify dependent's details entered by user
            if (!addDependentRequest.Equals(patientResult.ResourcePayload))
            {
                return new RequestResult<DependentModel>()
                {
                    ResultStatus = ResultType.Error,
                    ResultError = new RequestResultError() { ResultMessage = "The information you entered did not match. Please try again.", ErrorCode = ErrorTranslator.ServiceError(ErrorType.InvalidState, ServiceType.Patient) },
                };
            }

            // (2) Inserts Dependent to database
            var dependent = new UserDelegate() { OwnerId = patientResult.ResourcePayload.HdId, DelegateId = delegateHdId };

            DBResult<UserDelegate> dbDependent = this.userDelegateDelegate.Insert(dependent, true);
            RequestResult<DependentModel> result = new RequestResult<DependentModel>()
            {
                ResourcePayload = new DependentModel() { Name = patientResult.ResourcePayload.FirstName + " " + patientResult.ResourcePayload.LastName },
                ResultStatus = dbDependent.Status == DBStatusCode.Created ? ResultType.Success : ResultType.Error,
                ResultError = dbDependent.Status == DBStatusCode.Read ? null : new RequestResultError() { ResultMessage = dbDependent.Message, ErrorCode = ErrorTranslator.ServiceError(ErrorType.CommunicationInternal, ServiceType.Database) },
            };
            return result;
        }

        /// <inheritdoc />
        public RequestResult<IEnumerable<DependentModel>> GetDependents(string hdId, int page, int pageSize)
        {
<<<<<<< HEAD
            // (1) Get Dependents from database
            int offset = page * pageSize;
            DBResult<IEnumerable<UserDelegate>> dbUserDelegates = this.userDelegateDelegate.Get(hdId, offset, pageSize);

            // (2) Get Dependents Details from the Patient service
            string jwtString = this.httpContextAccessor.HttpContext.Request.Headers["Authorization"][0];

            List<DependentModel> dependentModels = new List<DependentModel>();
            RequestResult<IEnumerable<DependentModel>> result = new RequestResult<IEnumerable<DependentModel>>()
            {
                ResultStatus = ResultType.Success,
            };
            StringBuilder resultErrorMessage = new StringBuilder();
            foreach (UserDelegate userDelegate in dbUserDelegates.Payload)
            {
                this.logger.LogDebug($"Getting dependent details for Dependent hdid: {userDelegate.OwnerId} ...");
                ResourceIdentifier identifier = new ResourceIdentifier("hdid", userDelegate.OwnerId);
                RequestResult<PatientModel> patientResult = this.patientDelegate.GetPatientByIdentifier(identifier, jwtString);

                if (patientResult.ResourcePayload != null)
                {
                    dependentModels.Add(ModelConverter.PatientModelToDependentModel(patientResult.ResourcePayload));
                }
                else
                {
                    if (result.ResultStatus != ResultType.Error)
                    {
                        result.ResultStatus = ResultType.Error;
                        resultErrorMessage.Append($"Communication Exception when trying to retrieve Dependent(s) - HdId: {userDelegate.OwnerId};");
                    }
                    else
                    {
                        resultErrorMessage.Append($" HdId: {userDelegate.OwnerId};");
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
=======
            throw new NotImplementedException();
>>>>>>> e1182ee320d2100e41815da38f5b743087dfbc07
        }
    }
}
