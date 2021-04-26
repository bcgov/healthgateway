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
namespace HealthGateway.CommonTests.Services
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using HealthGateway.Common.Constants;
    using HealthGateway.Common.Delegates;
    using HealthGateway.Common.Models;
    using HealthGateway.Common.Services;
    using HealthGateway.Database.Delegates;
    using HealthGateway.Database.Models;
    using HealthGateway.Database.Wrapper;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Moq;
    using Xunit;

    /// <summary>
    /// PatientService's Unit Tests.
    /// </summary>
    public class PatientServiceTests
    {
        private const string Hdid = "abc123";
        private const string Phn = "123";

        /// <summary>
        /// GetPatientPHN - Happy Path.
        /// </summary>
        [Fact]
        public void ShouldGetPatientPHN()
        {
            var actual = GetPatientPHN(new Dictionary<string, string>(), Database.Constants.DBStatusCode.Created, false);
            Assert.Equal(Phn, actual.ResourcePayload);
        }

        /// <summary>
        /// GetPatient - Happy Path.
        /// </summary>
        [Fact]
        public void ShouldGetPatient()
        {
            GetPatient(PatientIdentifierType.HDID, new Dictionary<string, string>(), Database.Constants.DBStatusCode.Created);
        }

        /// <summary>
        /// GetPatient - Happy Path (Cached).
        /// </summary>
        [Fact]
        public void ShouldGetPatientFromCache()
        {
            var configDictionary = new Dictionary<string, string>();
            configDictionary.Add("PatientService:CacheTTL", "90");
            GetPatient(PatientIdentifierType.HDID, configDictionary, Database.Constants.DBStatusCode.Created, true);
        }

        /// <summary>
        /// GetPatient - Happy Path (Using PHN).
        /// </summary>
        [Fact]
        public void ShouldGetPatientFromCacheWithPHN()
        {
            var configDictionary = new Dictionary<string, string>();
            configDictionary.Add("PatientService:CacheTTL", "90");
            GetPatient(PatientIdentifierType.PHN, configDictionary, Database.Constants.DBStatusCode.Created);
        }

        /// <summary>
        /// GetPatient - DB Error.
        /// </summary>
        [Fact]
        public void ShouldGetPatientFromCacheWithDBError()
        {
            var configDictionary = new Dictionary<string, string>();
            configDictionary.Add("PatientService:CacheTTL", "90");
            GetPatient(PatientIdentifierType.HDID, configDictionary, Database.Constants.DBStatusCode.Error);
        }

        /// <summary>
        /// GetPatient - Valid ID.
        /// </summary>
        [Fact]
        public void ShoulSearchByValidIdentifier()
        {
            string phn = "abc123";

            RequestResult<PatientModel> requestResult = new RequestResult<PatientModel>()
            {
                ResultStatus = Common.Constants.ResultType.Success,
                TotalResultCount = 1,
                PageSize = 1,
                ResourcePayload = new PatientModel()
                {
                    FirstName = "John",
                    LastName = "Doe",
                    PersonalHealthNumber = phn,
                },
            };

            Mock<IClientRegistriesDelegate> patientDelegateMock = new Mock<IClientRegistriesDelegate>();
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>())
                .Build();
            patientDelegateMock.Setup(p => p.GetDemographicsByPHNAsync(It.IsAny<string>())).ReturnsAsync(requestResult);

            IPatientService service = new PatientService(
                new Mock<ILogger<PatientService>>().Object,
                configuration,
                patientDelegateMock.Object,
                new Mock<IGenericCacheDelegate>().Object);

            // Act
            RequestResult<PatientModel> actual = Task.Run(async () => await service.GetPatient("abc123", PatientIdentifierType.PHN).ConfigureAwait(true)).Result;

            // Verify
            Assert.Equal(ResultType.Success, actual.ResultStatus);
            Assert.Equal(phn, actual?.ResourcePayload?.PersonalHealthNumber);
        }

        /// <summary>
        /// GetPatient - Invalid Id.
        /// </summary>
        [Fact]
        public void ShoulBeEmptyIfInvalidIdentifier()
        {
            string phn = "abc123";

            RequestResult<PatientModel> requestResult = new RequestResult<PatientModel>()
            {
                ResultStatus = Common.Constants.ResultType.Success,
                TotalResultCount = 1,
                PageSize = 1,
                ResourcePayload = new PatientModel()
                {
                    FirstName = "John",
                    LastName = "Doe",
                    PersonalHealthNumber = phn,
                },
            };

            Mock<IClientRegistriesDelegate> patientDelegateMock = new Mock<IClientRegistriesDelegate>();
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>())
                .Build();
            patientDelegateMock.Setup(p => p.GetDemographicsByHDIDAsync(It.IsAny<string>())).ReturnsAsync(requestResult);

            IPatientService service = new PatientService(
                new Mock<ILogger<PatientService>>().Object,
                configuration,
                patientDelegateMock.Object,
                new Mock<IGenericCacheDelegate>().Object);

            // Act
            RequestResult<PatientModel> actual = Task.Run(async () => await service.GetPatient("abc123", (PatientIdentifierType)23).ConfigureAwait(true)).Result;

            // Verify
            Assert.Equal(Common.Constants.ResultType.Error, actual.ResultStatus);
        }

        private static RequestResult<string> GetPatientPHN(Dictionary<string, string> configDictionary, Database.Constants.DBStatusCode mockDBStatusCode, bool returnNullPatientResult)
        {
            RequestResult<PatientModel> requestResult = new RequestResult<PatientModel>()
            {
                ResultStatus = Common.Constants.ResultType.Success,
                TotalResultCount = 1,
                PageSize = 1,
                ResourcePayload = new PatientModel()
                {
                    FirstName = "John",
                    LastName = "Doe",
                    HdId = Hdid,
                    PersonalHealthNumber = Phn,
                },
            };
            Mock<IClientRegistriesDelegate> patientDelegateMock = new Mock<IClientRegistriesDelegate>();
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(configDictionary)
                .Build();

            patientDelegateMock.Setup(p => p.GetDemographicsByHDIDAsync(It.IsAny<string>())).ReturnsAsync(requestResult);

            DBResult<GenericCache> dbResult = new DBResult<GenericCache>()
            {
                Status = mockDBStatusCode,
            };
            Mock<IGenericCacheDelegate> genericCacheDelegateMock = new Mock<IGenericCacheDelegate>();
            genericCacheDelegateMock.Setup(p => p.CacheObject(It.IsAny<object>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), true)).Returns(dbResult);

            genericCacheDelegateMock.Setup(p => p.GetCacheObject<PatientModel>(It.IsAny<string>(), It.IsAny<string>())).Returns(returnNullPatientResult ? null : requestResult.ResourcePayload);

            IPatientService service = new PatientService(
                new Mock<ILogger<PatientService>>().Object,
                config,
                patientDelegateMock.Object,
                genericCacheDelegateMock.Object);

            // Act
            RequestResult<string> actual = Task.Run(async () => await service.GetPatientPHN(Hdid).ConfigureAwait(true)).Result;
            return actual;
        }

        private static void GetPatient(PatientIdentifierType identifierType, Dictionary<string, string> configDictionary, Database.Constants.DBStatusCode mockDBStatusCode, bool returnValidCache = false)
        {
            RequestResult<PatientModel> requestResult = new RequestResult<PatientModel>()
            {
                ResultStatus = Common.Constants.ResultType.Success,
                TotalResultCount = 1,
                PageSize = 1,
                ResourcePayload = new PatientModel()
                {
                    FirstName = "John",
                    LastName = "Doe",
                    HdId = Hdid,
                    PersonalHealthNumber = Phn,
                },
            };

            Mock<IClientRegistriesDelegate> patientDelegateMock = new Mock<IClientRegistriesDelegate>();
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(configDictionary)
                .Build();

            patientDelegateMock.Setup(p => p.GetDemographicsByHDIDAsync(It.IsAny<string>())).ReturnsAsync(requestResult);
            patientDelegateMock.Setup(p => p.GetDemographicsByPHNAsync(It.IsAny<string>())).ReturnsAsync(requestResult);

            DBResult<GenericCache> dbResult = new DBResult<GenericCache>()
            {
                Status = mockDBStatusCode,
            };
            Mock<IGenericCacheDelegate> genericCacheDelegateMock = new Mock<IGenericCacheDelegate>();
            genericCacheDelegateMock.Setup(p => p.CacheObject(It.IsAny<object>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), true)).Returns(dbResult);
            if (returnValidCache)
            {
                genericCacheDelegateMock.Setup(p => p.GetCacheObject<PatientModel>(It.IsAny<string>(), It.IsAny<string>())).Returns(requestResult.ResourcePayload);
            }

            IPatientService service = new PatientService(
                new Mock<ILogger<PatientService>>().Object,
                config,
                patientDelegateMock.Object,
                genericCacheDelegateMock.Object);

            // Act
            RequestResult<PatientModel> actual = Task.Run(async () => await service.GetPatient(Hdid, identifierType).ConfigureAwait(true)).Result;

            // Verify
            Assert.Equal(ResultType.Success, actual.ResultStatus);
            Assert.Equal(Hdid, actual?.ResourcePayload?.HdId);
        }
    }
}
