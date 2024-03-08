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
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using HealthGateway.Common.Data.Models;
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
            this.Setup(s => s.GetCovid19OrdersAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(delegateResult));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LaboratoryDelegateMock"/> class.
        /// </summary>
        /// <param name="delegateResult">List of laboratory reports.</param>
        /// <param name="isCovid19">Indicates whether the COVID-19 report should be returned.</param>
        public LaboratoryDelegateMock(RequestResult<LaboratoryReport> delegateResult, bool isCovid19 = true)
        {
            this.Setup(s => s.GetLabReportAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), isCovid19, It.IsAny<CancellationToken>())).Returns(Task.FromResult(delegateResult));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LaboratoryDelegateMock"/> class.
        /// </summary>
        /// <param name="delegateResult">List of Laboratory Orders.</param>
        public LaboratoryDelegateMock(RequestResult<PhsaResult<PhsaLaboratorySummary>> delegateResult)
        {
            this.Setup(s => s.GetLaboratorySummaryAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(delegateResult));
        }
    }
}
