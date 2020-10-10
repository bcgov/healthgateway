//-------------------------------------------------------------------------
// Copyright © 2019 Province of British Columbia
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//-------------------------------------------------------------------------
namespace HealthGateway.Patient.Services
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.Linq;
    using System.ServiceModel;
    using System.Text.Json;
    using System.Threading.Tasks;
    using HealthGateway.Common.Constants;
    using HealthGateway.Common.ErrorHandling;
    using HealthGateway.Common.Models;
    using HealthGateway.Patient.Delegates;
    using Microsoft.Extensions.Logging;
    using ServiceReference;

    /// <summary>
    /// The Patient data service.
    /// </summary>
    public class PatientService : IPatientService
    {
        /// <summary>
        /// The injected logger delegate.
        /// </summary>
        private readonly ILogger<PatientService> logger;

        private readonly IPatientDelegate patientDelegate;

        /// <summary>
        /// Initializes a new instance of the <see cref="PatientService"/> class.
        /// </summary>
        /// <param name="logger">The service Logger.</param>
        /// <param name="patientDelegate">The injected client registries delegate.</param>
        public PatientService(ILogger<PatientService> logger, IPatientDelegate patientDelegate)
        {
            this.logger = logger;
            this.patientDelegate = patientDelegate;
        }

        /// <inheritdoc/>
        public async System.Threading.Tasks.Task<RequestResult<PatientModel>> GetPatient(string hdid)
        {
            return await this.patientDelegate.GetDemographicsByHDIDAsync(hdid);
        }

        /// <inheritdoc/>
        public async System.Threading.Tasks.Task<RequestResult<PatientModel>> SearchPatientByIdentifier(ResourceIdentifier identifier)
        {
            if (identifier.Key == "phn")
            {
                return await this.patientDelegate.GetDemographicsByPHNAsync(identifier.Value);
            }
            else if (identifier.Key == "hdid")
            {
                return await this.patientDelegate.GetDemographicsByHDIDAsync(identifier.Value);
            }
            else
            {
                return new RequestResult<PatientModel>()
                {
                    ResultStatus = ResultType.Error,
                    ResultError = new RequestResultError() { ResultMessage = $"Identifier not recognized '{identifier.Key}'", ErrorCode = ErrorTranslator.InternalError(ErrorType.InvalidState) },
                };
            }
        }
    }
}
