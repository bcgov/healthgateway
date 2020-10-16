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
    using HealthGateway.Common.Constants;
    using HealthGateway.Common.Delegates;
    using HealthGateway.Common.ErrorHandling;
    using HealthGateway.Common.Models;
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
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly IPatientDelegate patientDelegate;
        private readonly IUserDelegateDelegate userDelegateDelegate;

        /// <summary>
        /// Initializes a new instance of the <see cref="DependentService"/> class.
        /// </summary>
        /// <param name="logger">Injected Logger Provider.</param>
        /// <param name="httpAccessor">The injected http context accessor provider.</param>
        /// <param name="patientDelegate">The injected patient registry provider.</param>
        /// <param name="userDelegateDelegate">The User Delegate delegate to interact with the DB.</param>
        public DependentService(
            ILogger<DependentService> logger,
            IHttpContextAccessor httpAccessor,
            IPatientDelegate patientDelegate,
            IUserDelegateDelegate userDelegateDelegate)
        {
            this.logger = logger;
            this.httpContextAccessor = httpAccessor;
            this.patientDelegate = patientDelegate;
            this.userDelegateDelegate = userDelegateDelegate;
        }

        /// <inheritdoc />
        public RequestResult<DependentModel> AddDependent(string delegateHdId, AddDependentRequest addDependentRequest)
        {
            this.logger.LogTrace($"Dependent hdid: {delegateHdId}");
            this.logger.LogDebug("Getting dependent details...");
            string jwtString = this.httpContextAccessor.HttpContext.Request.Headers["Authorization"][0];
            ResourceIdentifier identifier = new ResourceIdentifier("phn", addDependentRequest.PHN);
            RequestResult<PatientModel> patientResult = this.patientDelegate.GetPatientByIdentifier(identifier, jwtString);
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
                ResourcePayload = new DependentModel() { HdId = dbDependent.Payload.OwnerId, Name = patientResult.ResourcePayload.FirstName + " " + patientResult.ResourcePayload.LastName },
                ResultStatus = dbDependent.Status == DBStatusCode.Created ? ResultType.Success : ResultType.Error,
                ResultError = dbDependent.Status == DBStatusCode.Read ? null : new RequestResultError() { ResultMessage = dbDependent.Message, ErrorCode = ErrorTranslator.ServiceError(ErrorType.CommunicationInternal, ServiceType.Database) },
            };
            return result;
        }

        /// <inheritdoc />
        public RequestResult<IEnumerable<DependentModel>> GetDependents(string hdId)
        {


            throw new NotImplementedException();
        }
    }
}
