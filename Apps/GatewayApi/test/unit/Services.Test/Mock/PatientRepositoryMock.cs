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
namespace HealthGateway.GatewayApiTests.Services.Test.Mock
{
    using System.Collections.Generic;
    using System.Threading;
    using HealthGateway.AccountDataAccess.Patient;
    using HealthGateway.Common.Data.Constants;
    using Moq;

    /// <summary>
    /// PatientRepositoryMock.
    /// </summary>
    public class PatientRepositoryMock : Mock<IPatientRepository>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PatientRepositoryMock"/> class.
        /// </summary>
        /// <param name="hdid">hdid.</param>
        /// <param name="dataSources">A list of data sources.</param>
        public PatientRepositoryMock(string hdid, IEnumerable<DataSource> dataSources)
        {
            this.Setup(s => s.GetDataSourcesAsync(hdid, It.IsAny<CancellationToken>())).ReturnsAsync(dataSources);
        }
    }
}
