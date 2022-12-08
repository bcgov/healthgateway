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
namespace HealthGateway.LaboratoryTests.Services
{
    using System;
    using System.Net;
    using System.Net.Http;
    using HealthGateway.Common.AccessManagement.Authentication;
    using HealthGateway.Common.AccessManagement.Authentication.Models;
    using HealthGateway.Common.Data.Constants;
    using HealthGateway.Common.Data.ErrorHandling;
    using HealthGateway.Common.Data.ViewModels;
    using HealthGateway.Laboratory.Api;
    using HealthGateway.Laboratory.Models.PHSA;
    using HealthGateway.Laboratory.Services;
    using Microsoft.Extensions.Logging;
    using Moq;
    using Xunit;

    /// <summary>
    /// Unit Tests for the Lab Test Kit registration service.
    /// </summary>
    public class LabTestKitServiceTests
    {
        private readonly string accessToken = "access_token";

        /// <summary>
        /// Tests a valid auth test kit registration.
        /// </summary>
        [Fact]
        public void RegisterLabTestOk()
        {
            using HttpResponseMessage httpResponse = new()
            {
                StatusCode = HttpStatusCode.OK,
            };
            RequestResult<LabTestKit> actualResult = this.GetLabTestKitService(httpResponse).RegisterLabTestKitAsync("hdid", new LabTestKit()).Result;
            Assert.True(actualResult.ResultStatus == ResultType.Success);
        }

        /// <summary>
        /// Tests a valid public test kit registration which results in an HTTP exception.
        /// </summary>
        [Fact]
        public void RegisterPublicLabTestOk()
        {
            using HttpResponseMessage httpResponse = new()
            {
                StatusCode = HttpStatusCode.OK,
            };
            RequestResult<PublicLabTestKit> actualResult = this.GetLabTestKitService(httpResponse).RegisterLabTestKitAsync(CreatePublicLabTestKit("9735353315")).Result;
            Assert.True(actualResult.ResultStatus == ResultType.Success);
        }

        /// <summary>
        /// Tests a valid public test kit registration.
        /// </summary>
        [Fact]
        public void RegisterPublicLabTestHttpException()
        {
            RequestResult<PublicLabTestKit> actualResult = this.GetLabTestKitServiceException().RegisterLabTestKitAsync(CreatePublicLabTestKit("9735353315")).Result;
            Assert.True(actualResult.ResultStatus == ResultType.Error);
        }

        /// <summary>
        /// Tests an authenticated test kit registration which results in an HTTP exception.
        /// </summary>
        [Fact]
        public void RegisterLabTestHttpException()
        {
            RequestResult<LabTestKit> actualResult = this.GetLabTestKitServiceException().RegisterLabTestKitAsync("hdid", new LabTestKit()).Result;
            Assert.True(actualResult.ResultStatus == ResultType.Error);
        }

        /// <summary>
        /// Tests when Keycloak auth fails.
        /// </summary>
        [Fact]
        public void RegisterPublicLabTestKeycloakFail()
        {
            using HttpResponseMessage httpResponse = new()
            {
                StatusCode = HttpStatusCode.OK,
            };
            RequestResult<PublicLabTestKit> actualResult = this.GetLabTestKitService(httpResponse, true).RegisterLabTestKitAsync(CreatePublicLabTestKit("9735353315")).Result;
            Assert.True(actualResult.ResultStatus == ResultType.Error);
        }

        /// <summary>
        /// Tests when a lab kit is already registered.
        /// </summary>
        [Fact]
        public void RegisterPublicLabTestConflict()
        {
            using HttpResponseMessage httpResponse = new()
            {
                StatusCode = HttpStatusCode.Conflict,
            };
            RequestResult<PublicLabTestKit> actualResult = this.GetLabTestKitService(httpResponse).RegisterLabTestKitAsync(CreatePublicLabTestKit("9735353315")).Result;
            Assert.True(
                actualResult.ResultStatus == ResultType.ActionRequired &&
                actualResult.ResultError != null &&
                actualResult.ResultError.ActionCodeValue == ActionType.Processed.Value);
        }

        /// <summary>
        /// Tests when bad data is sent.
        /// </summary>
        [Fact]
        public void RegisterPublicLabTestUnprocessable()
        {
            using HttpResponseMessage httpResponse = new()
            {
                StatusCode = HttpStatusCode.UnprocessableEntity,
            };
            RequestResult<PublicLabTestKit> actualResult = this.GetLabTestKitService(httpResponse).RegisterLabTestKitAsync(CreatePublicLabTestKit("9735353315")).Result;
            Assert.True(
                actualResult.ResultStatus == ResultType.ActionRequired &&
                actualResult.ResultError != null &&
                actualResult.ResultError.ActionCodeValue == ActionType.Validation.Value);
        }

        /// <summary>
        /// Tests when bad data is sent.
        /// </summary>
        [Fact]
        public void RegisterPublicLabTestBadPhn()
        {
            using HttpResponseMessage httpResponse = new()
            {
                StatusCode = HttpStatusCode.OK,
            };
            RequestResult<PublicLabTestKit> actualResult = this.GetLabTestKitService(httpResponse).RegisterLabTestKitAsync(CreatePublicLabTestKit("BADPHN")).Result;
            Assert.True(
                actualResult.ResultStatus == ResultType.ActionRequired &&
                actualResult.ResultError != null &&
                actualResult.ResultError.ActionCodeValue == ActionType.Validation.Value);
        }

        /// <summary>
        /// Tests when bad data is sent.
        /// </summary>
        [Fact]
        public void RegisterPublicLabTestAddrReqd()
        {
            using HttpResponseMessage httpResponse = new()
            {
                StatusCode = HttpStatusCode.OK,
            };
            RequestResult<PublicLabTestKit> actualResult = this.GetLabTestKitService(httpResponse).RegisterLabTestKitAsync(CreatePublicLabTestKit(null)).Result;
            Assert.True(
                actualResult.ResultStatus == ResultType.ActionRequired &&
                actualResult.ResultError != null &&
                actualResult.ResultError.ActionCodeValue == ActionType.Validation.Value);
        }

        /// <summary>
        /// Tests for authorization failure.
        /// </summary>
        [Fact]
        public void RegisterPublicLabTestUnauthorized()
        {
            using HttpResponseMessage httpResponse = new()
            {
                StatusCode = HttpStatusCode.Unauthorized,
            };
            RequestResult<PublicLabTestKit> actualResult = this.GetLabTestKitService(httpResponse).RegisterLabTestKitAsync(CreatePublicLabTestKit("9735353315")).Result;
            Assert.True(actualResult.ResultStatus == ResultType.Error);
        }

        /// <summary>
        /// Tests for a forbidden error.
        /// </summary>
        [Fact]
        public void RegisterPublicLabTestForbidden()
        {
            using HttpResponseMessage httpResponse = new()
            {
                StatusCode = HttpStatusCode.Forbidden,
            };
            RequestResult<PublicLabTestKit> actualResult = this.GetLabTestKitService(httpResponse).RegisterLabTestKitAsync(CreatePublicLabTestKit("9735353315")).Result;
            Assert.True(actualResult.ResultStatus == ResultType.Error);
        }

        /// <summary>
        /// Tests for any other status code returned.
        /// </summary>
        [Fact]
        public void RegisterPublicLabTestDefault()
        {
            using HttpResponseMessage httpResponse = new()
            {
                StatusCode = HttpStatusCode.BadGateway,
            };
            RequestResult<PublicLabTestKit> actualResult = this.GetLabTestKitService(httpResponse).RegisterLabTestKitAsync(CreatePublicLabTestKit("9735353315")).Result;
            Assert.True(actualResult.ResultStatus == ResultType.Error);
        }

        private static PublicLabTestKit CreatePublicLabTestKit(string? phn)
        {
            PublicLabTestKit testKit = new()
            {
                Phn = phn,
            };

            return testKit;
        }

        private LabTestKitService GetLabTestKitServiceException()
        {
            HttpRequestException httpRequestException = new("Error with HTTP Request");
            Mock<ILabTestKitApi> mockLabTestKitApi = new();
            mockLabTestKitApi.Setup(s => s.RegisterLabTestAsync(It.IsAny<PublicLabTestKit>(), It.IsAny<string>(), It.IsAny<string>()))
                .ThrowsAsync(httpRequestException);
            mockLabTestKitApi.Setup(s => s.RegisterLabTestAsync(It.IsAny<string>(), It.IsAny<LabTestKit>(), It.IsAny<string>()))
                .ThrowsAsync(httpRequestException);

            JwtModel jwt = new()
            {
                AccessToken = this.accessToken,
            };
            Mock<IAuthenticationDelegate> mockAuthDelegate = new();
            mockAuthDelegate.Setup(s => s.AuthenticateAsSystem(It.IsAny<Uri>(), It.IsAny<ClientCredentialsTokenRequest>(), It.IsAny<bool>())).Returns(jwt);

            LabTestKitService labTestKitService = new(
                new Mock<ILogger<LabTestKitService>>().Object,
                mockAuthDelegate.Object,
                mockLabTestKitApi.Object,
                null);

            return labTestKitService;
        }

        private LabTestKitService GetLabTestKitService(HttpResponseMessage responseMessage, bool nullToken = false)
        {
            Mock<ILabTestKitApi> mockLabTestKitApi = new();
            mockLabTestKitApi.Setup(s => s.RegisterLabTestAsync(It.IsAny<PublicLabTestKit>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(responseMessage);
            mockLabTestKitApi.Setup(s => s.RegisterLabTestAsync(It.IsAny<string>(), It.IsAny<LabTestKit>(), It.IsAny<string>()))
                .ReturnsAsync(responseMessage);

            JwtModel jwt = new()
            {
                AccessToken = !nullToken ? this.accessToken : null,
            };
            Mock<IAuthenticationDelegate> mockAuthDelegate = new();
            mockAuthDelegate.Setup(s => s.AuthenticateAsSystem(It.IsAny<Uri>(), It.IsAny<ClientCredentialsTokenRequest>(), It.IsAny<bool>()))
                .Returns(jwt);

            LabTestKitService labTestKitService = new(
                new Mock<ILogger<LabTestKitService>>().Object,
                mockAuthDelegate.Object,
                mockLabTestKitApi.Object,
                null);

            return labTestKitService;
        }
    }
}
