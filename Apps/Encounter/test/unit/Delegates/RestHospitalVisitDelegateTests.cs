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
        private const string ExpectedExceptionMessage = "Error while retrieving Hospital Visits";

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
            mockHospitalVisitApi.Setup(s => s.GetHospitalVisitsAsync(It.IsAny<string>(), It.IsAny<int?>(), AccessToken)).ReturnsAsync(phsaResponse);
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
            Assert.Equal(2, actualResult.ResourcePayload.Result.Count());
            Assert.Equal(2, actualResult.TotalResultCount);
            Assert.Equal(int.Parse(ConfigFetchSize, CultureInfo.InvariantCulture), actualResult.PageSize);
        }

        /// <summary>
        /// GetHospitalVisits - api throws Http Request Exception.
        /// </summary>
        [Fact]
        public void GetHospitalVisitShouldHandleApiException()
        {
            // Arrange
            ApiException mockException = MockRefitExceptionHelper.CreateApiException(HttpStatusCode.Unauthorized, HttpMethod.Post);

            Mock<IAuthenticationDelegate> mockAuthDelegate = new();
            mockAuthDelegate.Setup(s => s.FetchAuthenticatedUserToken()).Returns(AccessToken);
            Mock<IHospitalVisitApi> mockHospitalVisitApi = new();
            mockHospitalVisitApi.Setup(s => s.GetHospitalVisitsAsync(It.IsAny<string>(), It.IsAny<int?>(), AccessToken)).ThrowsAsync(mockException);
            IHospitalVisitDelegate hospitalVisitDelegate = new RestHospitalVisitDelegate(
                mockAuthDelegate.Object,
                mockHospitalVisitApi.Object,
                new Mock<ILogger<RestHospitalVisitDelegate>>().Object,
                GetIConfigurationRoot());

            // Act
            RequestResult<PhsaResult<IEnumerable<HospitalVisit>>> actualResult = hospitalVisitDelegate.GetHospitalVisitsAsync(It.IsAny<string>()).Result;

            // Assert
            Assert.Equal(ResultType.Error, actualResult.ResultStatus);
            Assert.Equal(ExpectedExceptionMessage, actualResult.ResultError?.ResultMessage);
            Assert.Null(actualResult.TotalResultCount);
            Assert.Null(actualResult.PageSize);
        }

        /// <summary>
        /// GetHospitalVisits - api throws Http Request Exception.
        /// </summary>
        [Fact]
        public void GetHospitalVisitShouldHandleHttpRequestException()
        {
            // Arrange
            HttpRequestException mockException = MockRefitExceptionHelper.CreateHttpRequestException("Internal Server Error", HttpStatusCode.InternalServerError);

            Mock<IAuthenticationDelegate> mockAuthDelegate = new();
            mockAuthDelegate.Setup(s => s.FetchAuthenticatedUserToken()).Returns(AccessToken);
            Mock<IHospitalVisitApi> mockHospitalVisitApi = new();
            mockHospitalVisitApi.Setup(s => s.GetHospitalVisitsAsync(It.IsAny<string>(), It.IsAny<int?>(), AccessToken)).ThrowsAsync(mockException);
            IHospitalVisitDelegate hospitalVisitDelegate = new RestHospitalVisitDelegate(
                mockAuthDelegate.Object,
                mockHospitalVisitApi.Object,
                new Mock<ILogger<RestHospitalVisitDelegate>>().Object,
                GetIConfigurationRoot());

            // Act
            RequestResult<PhsaResult<IEnumerable<HospitalVisit>>> actualResult = hospitalVisitDelegate.GetHospitalVisitsAsync(It.IsAny<string>()).Result;

            // Assert
            Assert.Equal(ResultType.Error, actualResult.ResultStatus);
            Assert.Equal(ExpectedExceptionMessage, actualResult.ResultError?.ResultMessage);
            Assert.Null(actualResult.TotalResultCount);
            Assert.Null(actualResult.PageSize);
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
