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
namespace HealthGateway.Immunization.Services
{
    using System;
    using System.Globalization;
    using System.Threading.Tasks;
    using HealthGateway.Common.Models;
    using HealthGateway.Common.Utils;
    using HealthGateway.Immunization.Models;

    /// <summary>
    /// The Vaccine Status data service.
    /// </summary>
    public class VaccineStatusService : IVaccineStatusService
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="VaccineStatusService"/> class.
        /// </summary>
        public VaccineStatusService()
        {
        }

        /// <inheritdoc/>
        public async Task<RequestResult<VaccineStatus>> GetVaccineStatus(string phn, string dateOfBirth)
        {
            RequestResult<VaccineStatus> retVal;
            DateTime? dob = null;
            try
            {
                dob = DateTime.ParseExact(dateOfBirth, "yyyyMMdd", CultureInfo.InvariantCulture);
            }
            catch (Exception e) when (e is FormatException || e is ArgumentNullException)
            {
                // do nothing, we're golden as we are.
            }

            if (dob != null && PHNValidator.IsValid(phn))
            {
                retVal = new ()
                {
                    ResultStatus = Common.Constants.ResultType.Success,
                    ResourcePayload = new ()
                    {
                    },
                    TotalResultCount = 1,
                };
            }
            else
            {
                retVal = new ()
                {
                    ResultStatus = Common.Constants.ResultType.Success,
                    ResourcePayload = new ()
                    {
                        PersonalHealthNumber = phn,
                    },
                    TotalResultCount = 1,
                };
            }

            return retVal;
        }

        /// <inheritdoc/>
        public async Task<RequestResult<VaccineStatus>> GetVaccineStatus(Guid id)
        {
            return new RequestResult<VaccineStatus>()
            {
                ResultStatus = Common.Constants.ResultType.Success,
                ResourcePayload = new ()
                {
                    Id = id,
                },
                TotalResultCount = 1,
            };
        }
    }
}
