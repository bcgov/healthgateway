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
namespace HealthGateway.EncounterTests.Delegates
{
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using HealthGateway.Common.AccessManagement.Authentication;
    using HealthGateway.Common.Data.Constants;
    using HealthGateway.Common.Data.ViewModels;
    using HealthGateway.Common.Models.PHSA;
    using HealthGateway.Common.Utils;
    using HealthGateway.Encounter.Api;
    using HealthGateway.Encounter.Delegates;
    using HealthGateway.Encounter.Models.PHSA;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Moq;
    using Refit;
    using Xunit;

    /// <summary>
    /// RestHospitalVisitDelegate's Unit Tests.
    /// </summary>
    public class RestHospitalVisitDelegateTests
    {
        private const string AccessToken = "access_token";
        private const string ConfigFetchSize = "25";

        /// <summary>
        /// GetHospitalVisits - api returns one row.
        /// </summary>
        [Fact]
        public void ShouldGetHospitalVisits()
        {
            // Arrange
            PhsaResult<IEnumerable<HospitalVisit>> phsaResponse = new()
            {
                Result = new List<HospitalVisit>
                {
                    new()
                    {
                        EncounterId = "123",
                        AdmitDateTime = null,
                        EndDateTime = null,
                    },
                    new()
                    {
                        EncounterId = "456",
                        AdmitDateTime = null,
                        EndDateTime = null,
                    },
                },
            };

            Mock<IAuthenticationDelegate> mockAuthDelegate = new();
            mockAuthDelegate.Setup(s => s.FetchAuthenticatedUserToken()).Returns(AccessToken);
            Mock<IHospitalVisitApi> mockHospitalVisitApi = new();
            mockHospitalVisitApi.Setup(s => s.GetHospitalVisitsAsync(It.IsAny<Dictionary<string, string?>>(), AccessToken)).ReturnsAsync(phsaResponse);
            IHospitalVisitDelegate hospitalVisitDelegate = new RestHospitalVisitDelegate(
                mockAuthDelegate.Object,
                mockHospitalVisitApi.Object,
                new Mock<ILogger<RestHospitalVisitDelegate>>().Object,
                GetIConfigurationRoot());

            // Act
            RequestResult<PhsaResult<IEnumerable<HospitalVisit>>> actualResult = hospitalVisitDelegate.GetHospitalVisitsAsync(It.IsAny<string>()).Result;

            // Assert
            Assert.Equal(ResultType.Success, actualResult.ResultStatus);
            Assert.NotNull(actualResult.ResourcePayload);
            Assert.True(actualResult.ResourcePayload.Result.Count() == 2);
            Assert.True(actualResult.TotalResultCount == 2);
            Assert.True(actualResult.PageSize == int.Parse(ConfigFetchSize, CultureInfo.InvariantCulture));
        }

        /// <summary>
        /// GetHospitalVisits - api throws Http Request Exception.
        /// </summary>
        [Fact]
        public void GetHospitalVisitShouldHandleApiException()
        {
            string expectedMessage = $"Status: {HttpStatusCode.Unauthorized}. Error while retrieving Hospital Visits";

            // Arrange
            ApiException mockException = MockRefitException.CreateApiException(HttpStatusCode.Unauthorized, HttpMethod.Post);

            Mock<IAuthenticationDelegate> mockAuthDelegate = new();
            mockAuthDelegate.Setup(s => s.FetchAuthenticatedUserToken()).Returns(AccessToken);
            Mock<IHospitalVisitApi> mockHospitalVisitApi = new();
            mockHospitalVisitApi.Setup(s => s.GetHospitalVisitsAsync(It.IsAny<Dictionary<string, string?>>(), AccessToken)).ThrowsAsync(mockException);
            IHospitalVisitDelegate hospitalVisitDelegate = new RestHospitalVisitDelegate(
                mockAuthDelegate.Object,
                mockHospitalVisitApi.Object,
                new Mock<ILogger<RestHospitalVisitDelegate>>().Object,
                GetIConfigurationRoot());

            // Act
            RequestResult<PhsaResult<IEnumerable<HospitalVisit>>> actualResult = hospitalVisitDelegate.GetHospitalVisitsAsync(It.IsAny<string>()).Result;

            // Assert
            Assert.Equal(ResultType.Error, actualResult.ResultStatus);
            Assert.Equal(expectedMessage, actualResult.ResultError?.ResultMessage);
            Assert.True(actualResult.TotalResultCount == 0);
            Assert.True(actualResult.PageSize == int.Parse(ConfigFetchSize, CultureInfo.InvariantCulture));
        }

        /// <summary>
        /// GetHospitalVisits - api throws Http Request Exception.
        /// </summary>
        [Fact]
        public void GetHospitalVisitShouldHandleHttpRequestException()
        {
            string expectedMessage = $"Status: {HttpStatusCode.InternalServerError}. Error while retrieving Hospital Visits";

            // Arrange
            HttpRequestException mockException = MockRefitException.CreateHttpRequestException("Internal Server Error", HttpStatusCode.InternalServerError);

            Mock<IAuthenticationDelegate> mockAuthDelegate = new();
            mockAuthDelegate.Setup(s => s.FetchAuthenticatedUserToken()).Returns(AccessToken);
            Mock<IHospitalVisitApi> mockHospitalVisitApi = new();
            mockHospitalVisitApi.Setup(s => s.GetHospitalVisitsAsync(It.IsAny<Dictionary<string, string?>>(), AccessToken)).ThrowsAsync(mockException);
            IHospitalVisitDelegate hospitalVisitDelegate = new RestHospitalVisitDelegate(
                mockAuthDelegate.Object,
                mockHospitalVisitApi.Object,
                new Mock<ILogger<RestHospitalVisitDelegate>>().Object,
                GetIConfigurationRoot());

            // Act
            RequestResult<PhsaResult<IEnumerable<HospitalVisit>>> actualResult = hospitalVisitDelegate.GetHospitalVisitsAsync(It.IsAny<string>()).Result;

            // Assert
            Assert.Equal(ResultType.Error, actualResult.ResultStatus);
            Assert.Equal(expectedMessage, actualResult.ResultError?.ResultMessage);
            Assert.True(actualResult.TotalResultCount == 0);
            Assert.True(actualResult.PageSize == int.Parse(ConfigFetchSize, CultureInfo.InvariantCulture));
        }

        private static IConfigurationRoot GetIConfigurationRoot()
        {
            Dictionary<string, string?> configuration = new()
            {
                { "PHSA:FetchSize", ConfigFetchSize },
            };

            return new ConfigurationBuilder()
                .AddInMemoryCollection(configuration.ToList())
                .Build();
        }
    }
}
