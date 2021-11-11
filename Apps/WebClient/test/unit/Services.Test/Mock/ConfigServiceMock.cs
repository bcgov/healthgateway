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
    using HealthGateway.WebClient.Models;
    using HealthGateway.WebClient.Services;
    using Moq;

    /// <summary>
    /// ConfigServiceMock.
    /// </summary>
    public class ConfigServiceMock : Mock<IConfigurationService>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigServiceMock"/> class.
        /// </summary>
        /// <param name="minPatientAge">minPatientAge.</param>
        public ConfigServiceMock(int minPatientAge)
        {
            this.Setup(s => s.GetConfiguration()).Returns(new ExternalConfiguration() { WebClient = new WebClientConfiguration() { MinPatientAge = minPatientAge } });
        }
    }
}
