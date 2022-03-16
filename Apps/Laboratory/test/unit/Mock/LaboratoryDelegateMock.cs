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
namespace HealthGateway.LaboratoryTests.Mock
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using HealthGateway.Common.Data.ViewModels;
    using HealthGateway.Common.Models.PHSA;
    using HealthGateway.Laboratory.Delegates;
    using HealthGateway.Laboratory.Models;
    using HealthGateway.Laboratory.Models.PHSA;
    using Moq;

    /// <summary>
    /// Class to mock ILaboratoryDelegate.
    /// </summary>
    public class LaboratoryDelegateMock : Mock<ILaboratoryDelegate>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LaboratoryDelegateMock"/> class.
        /// </summary>
        /// <param name="delegateResult">List of COVID-19 Orders.</param>
        public LaboratoryDelegateMock(RequestResult<PhsaResult<List<PhsaCovid19Order>>> delegateResult)
        {
            this.Setup(s => s.GetCovid19Orders(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>())).Returns(Task.FromResult(delegateResult));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LaboratoryDelegateMock"/> class.
        /// </summary>
        /// <param name="delegateResult">List of laboratory reports.</param>
        /// <param name="isCovid19">Indicates whether the COVID-19 report should be returned.</param>
        public LaboratoryDelegateMock(RequestResult<LaboratoryReport> delegateResult, bool isCovid19 = true)
        {
            this.Setup(s => s.GetLabReport(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>(), isCovid19)).Returns(Task.FromResult(delegateResult));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LaboratoryDelegateMock"/> class.
        /// </summary>
        /// <param name="delegateResult">list of COVID-19 Test Results.</param>
        /// <param name="token">token needed for authentication.</param>
        public LaboratoryDelegateMock(RequestResult<PhsaResult<IEnumerable<CovidTestResult>>> delegateResult, string token)
        {
            this.Setup(s => s.GetPublicTestResults(token, It.IsAny<string>(), It.IsAny<DateOnly>(), It.IsAny<DateOnly>())).Returns(Task.FromResult(delegateResult));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LaboratoryDelegateMock"/> class.
        /// </summary>
        /// <param name="delegateResult">List of Laboratory Orders.</param>
        public LaboratoryDelegateMock(RequestResult<PhsaResult<PhsaLaboratorySummary>> delegateResult)
        {
            this.Setup(s => s.GetLaboratorySummary(It.IsAny<string>(), It.IsAny<string>())).Returns(Task.FromResult(delegateResult));
        }
    }
}
