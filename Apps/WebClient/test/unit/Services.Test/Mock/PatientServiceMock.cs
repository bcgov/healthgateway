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
namespace HealthGateway.WebClientTests.Services.Test.Mock
{
    using HealthGateway.Common.Constants;
    using HealthGateway.Common.Models;
    using HealthGateway.Common.Services;
    using Moq;

    /// <summary>
    /// PatientServiceMock.
    /// </summary>
    public class PatientServiceMock : Mock<IPatientService>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PatientServiceMock"/> class.
        /// </summary>
        /// <param name="hdid">hdid.</param>
        /// <param name="patientModel">patient model.</param>
        public PatientServiceMock(string hdid, PatientModel patientModel)
        {
            this.Setup(s => s.GetPatient(hdid, PatientIdentifierType.HDID, false))
                .ReturnsAsync(new RequestResult<PatientModel>
                {
                    ResultStatus = ResultType.Success,
                    ResourcePayload = patientModel,
                });
        }
    }
}
