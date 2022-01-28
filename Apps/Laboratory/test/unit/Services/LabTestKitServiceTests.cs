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
namespace HealthGateway.LaboratoryTests
{
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Http;
    using System.Security.Claims;
    using HealthGateway.Common.AccessManagement.Authentication;
    using HealthGateway.Common.Data.Constants;
    using HealthGateway.Common.Data.Models.ErrorHandling;
    using HealthGateway.Common.Data.ViewModels;
    using HealthGateway.Laboratory.Delegates;
    using HealthGateway.Laboratory.Models.PHSA;
    using HealthGateway.Laboratory.Services;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Authentication.JwtBearer;
    using Microsoft.AspNetCore.Http;
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
            RequestResult<PublicLabTestKit> actualResult = this.GetLabTestKitService(httpResponse).RegisterLabTestKitAsync(new PublicLabTestKit()).Result;
            Assert.True(actualResult.ResultStatus == ResultType.Success);
        }

        /// <summary>
        /// Tests a valid public test kit registration.
        /// </summary>
        [Fact]
        public void RegisterPublicLabTestHttpException()
        {
            RequestResult<PublicLabTestKit> actualResult = this.GetLabTestKitServiceException().RegisterLabTestKitAsync(new PublicLabTestKit()).Result;
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
            RequestResult<PublicLabTestKit> actualResult = this.GetLabTestKitService(httpResponse, true).RegisterLabTestKitAsync(new PublicLabTestKit()).Result;
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
            RequestResult<PublicLabTestKit> actualResult = this.GetLabTestKitService(httpResponse).RegisterLabTestKitAsync(new PublicLabTestKit()).Result;
            Assert.True(actualResult.ResultStatus == ResultType.ActionRequired &&
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
            RequestResult<PublicLabTestKit> actualResult = this.GetLabTestKitService(httpResponse).RegisterLabTestKitAsync(new PublicLabTestKit()).Result;
            Assert.True(actualResult.ResultStatus == ResultType.ActionRequired &&
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
            RequestResult<PublicLabTestKit> actualResult = this.GetLabTestKitService(httpResponse).RegisterLabTestKitAsync(new PublicLabTestKit()).Result;
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
            RequestResult<PublicLabTestKit> actualResult = this.GetLabTestKitService(httpResponse).RegisterLabTestKitAsync(new PublicLabTestKit()).Result;
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
            RequestResult<PublicLabTestKit> actualResult = this.GetLabTestKitService(httpResponse).RegisterLabTestKitAsync(new PublicLabTestKit()).Result;
            Assert.True(actualResult.ResultStatus == ResultType.Error);
        }

        private LabTestKitService GetLabTestKitServiceException()
        {
            HttpRequestException httpRequestException = new("Error with HTTP Request");
            Mock<ILabTestKitClient> mockLabTestKitClient = new();
            mockLabTestKitClient.Setup(s => s.RegisterLabTest(It.IsAny<PublicLabTestKit>(), It.IsAny<string>()))
                .ThrowsAsync(httpRequestException);
            mockLabTestKitClient.Setup(s => s.RegisterLabTest(It.IsAny<string>(), It.IsAny<LabTestKit>(), It.IsAny<string>()))
                .ThrowsAsync(httpRequestException);

            Mock<IAuthenticationDelegate> mockAuthDelegate = new();
            mockAuthDelegate.Setup(s => s.AccessTokenAsUser()).Returns(this.accessToken);

            LabTestKitService labTestKitService = new(
                new Mock<ILogger<LabTestKitService>>().Object,
                mockAuthDelegate.Object,
                mockLabTestKitClient.Object);

            return labTestKitService;
        }

        private LabTestKitService GetLabTestKitService(HttpResponseMessage responseMessage, bool nullToken = false)
        {
            Mock<ILabTestKitClient> mockLabTestKitClient = new();
            mockLabTestKitClient.Setup(s => s.RegisterLabTest(It.IsAny<PublicLabTestKit>(), It.IsAny<string>()))
                .ReturnsAsync(responseMessage);
            mockLabTestKitClient.Setup(s => s.RegisterLabTest(It.IsAny<string>(), It.IsAny<LabTestKit>(), It.IsAny<string>()))
                .ReturnsAsync(responseMessage);

            Mock<IAuthenticationDelegate> mockAuthDelegate = new();
            if (!nullToken)
            {
                mockAuthDelegate.Setup(s => s.AccessTokenAsUser()).Returns(this.accessToken);
            }

            LabTestKitService labTestKitService = new(
                new Mock<ILogger<LabTestKitService>>().Object,
                mockAuthDelegate.Object,
                mockLabTestKitClient.Object);

            return labTestKitService;
        }
    }
}
