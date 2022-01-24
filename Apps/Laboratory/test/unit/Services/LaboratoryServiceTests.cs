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
namespace HealthGateway.LaboratoryTests
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Net;
    using System.Security.Claims;
    using System.Threading.Tasks;
    using DeepEqual.Syntax;
    using HealthGateway.Common.AccessManagement.Authentication;
    using HealthGateway.Common.AccessManagement.Authentication.Models;
    using HealthGateway.Common.Constants.PHSA;
    using HealthGateway.Common.Data.Constants;
    using HealthGateway.Common.Data.Models.ErrorHandling;
    using HealthGateway.Common.Data.ViewModels;
    using HealthGateway.Common.Models.PHSA;
    using HealthGateway.Laboratory.Delegates;
    using HealthGateway.Laboratory.Factories;
    using HealthGateway.Laboratory.Models;
    using HealthGateway.Laboratory.Models.PHSA;
    using HealthGateway.Laboratory.Services;
    using HealthGateway.LaboratoryTests.Mock;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Authentication.JwtBearer;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Caching.Memory;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Moq;
    using Xunit;

    /// <summary>
    /// Unit Tests for LaboratoryService.
    /// </summary>
    public class LaboratoryServiceTests
    {
        private const string HDID = "EXTRIOYFPNX35TWEBUAJ3DNFDFXSYTBC6J4M76GYE3HC5ER2NKWQ";
        private const string TOKEN = "Fake Access Token";
        private const string USERID = "1001";
        private const string MockedMessageID = "mockedMessageID";
        private const string MockedReportContent = "mockedReportContent";
        private readonly string phn = "9735353315";
        private readonly DateOnly dateOfBirth = new(1967, 06, 02);
        private readonly DateOnly collectionDate = new(2021, 07, 04);

        private readonly ClaimsPrincipal claimsPrincipal = new(
            new ClaimsIdentity(
                new List<Claim>
                {
                    new Claim(ClaimTypes.Name, "username"),
                    new Claim(ClaimTypes.NameIdentifier, USERID),
                    new Claim("hdid", HDID),
                },
                "TestAuth"));

        /// <summary>
        /// ShouldGetCovid19Orders test.
        /// </summary>
        /// <param name="expectedResultType"> result type from service.</param>
        [Theory]
        [InlineData(ResultType.Success)]
        [InlineData(ResultType.Error)]
        public void ShouldGetCovid19Orders(ResultType expectedResultType)
        {
            List<PhsaCovid19Order> labOrders = new()
            {
                new PhsaCovid19Order()
                {
                    Id = Guid.NewGuid(),
                    Location = "Vancouver",
                    PHN = "001",
                    MessageDateTime = DateTime.Now,
                    MessageID = MockedMessageID + "1",
                    ReportAvailable = true,
                },
                new PhsaCovid19Order()
                {
                    Id = Guid.NewGuid(),
                    Location = "Vancouver",
                    PHN = "002",
                    MessageDateTime = DateTime.Now,
                    MessageID = MockedMessageID + "2",
                    ReportAvailable = false,
                },
            };

            RequestResult<IEnumerable<PhsaCovid19Order>> delegateResult = new()
            {
                ResultStatus = expectedResultType,
                PageSize = 100,
                PageIndex = 1,
                ResourcePayload = labOrders,
            };

            Mock<IHttpContextAccessor> mockHttpContextAccessor = new HttpContextAccessorMock(TOKEN, this.claimsPrincipal);

            ILaboratoryService service = new LaboratoryServiceMock(delegateResult, mockHttpContextAccessor, TOKEN, this.claimsPrincipal).LaboratoryServiceMockInstance();

            Task<RequestResult<IEnumerable<Covid19Order>>> actualResult = service.GetCovid19Orders(HDID, 0);

            if (expectedResultType == ResultType.Success)
            {
                Assert.Equal(ResultType.Success, actualResult.Result.ResultStatus);
                int count = 0;
                foreach (Covid19Order model in actualResult.Result!.ResourcePayload!)
                {
                    count++;
                    Assert.True(model.MessageID.Equals(MockedMessageID + count, StringComparison.Ordinal));
                }

                Assert.Equal(2, count);
            }
            else
            {
                Assert.Equal(ResultType.Error, actualResult.Result.ResultStatus);
            }
        }

        /// <summary>
        /// GetLabReport test.
        /// </summary>
        [Fact]
        public void ShouldGetLabReport()
        {
            LaboratoryReport labReport = new()
            {
                Report = MockedReportContent,
                MediaType = "mockedMediaType",
                Encoding = "mockedEncoding",
            };
            RequestResult<LaboratoryReport> delegateResult = new()
            {
                ResultStatus = ResultType.Success,
                PageSize = 100,
                PageIndex = 1,
                ResourcePayload = labReport,
            };

            Mock<IHttpContextAccessor> mockHttpContextAccessor = new HttpContextAccessorMock(TOKEN, this.claimsPrincipal);

            ILaboratoryService service = new LaboratoryServiceMock(delegateResult, mockHttpContextAccessor, TOKEN, this.claimsPrincipal).LaboratoryServiceMockInstance();

            Task<RequestResult<LaboratoryReport>> actualResult = service.GetLabReport(Guid.NewGuid(), string.Empty);

            Assert.Equal(ResultType.Success, actualResult.Result.ResultStatus);
            Assert.Equal(MockedReportContent, actualResult.Result.ResourcePayload!.Report);
        }

        /// <summary>
        /// GetPublicTestResults - Happy Path.
        /// </summary>
        [Fact]
        public void ShouldGetCovidTests()
        {
            RequestResult<PublicCovidTestResponse> expectedResult = new()
            {
                ResultStatus = ResultType.Success,
                ResultError = null,
                ResourcePayload = new PublicCovidTestResponse(new List<PublicCovidTestRecord> { new(), new() })
                {
                    Loaded = true,
                },
            };

            RequestResult<PHSAResult<IEnumerable<CovidTestResult>>> delegateResult = new()
            {
                ResultStatus = ResultType.Success,
                ResourcePayload = new()
                {
                    LoadState = new(),
                    Result = new List<CovidTestResult>
                    {
                        new CovidTestResult { StatusIndicator = nameof(LabIndicatorType.Found) },
                        new CovidTestResult { StatusIndicator = nameof(LabIndicatorType.Found) },
                    },
                },
            };

            Mock<IHttpContextAccessor> mockHttpContextAccessor = new HttpContextAccessorMock(TOKEN, this.claimsPrincipal);

            ILaboratoryService service = new LaboratoryServiceMock(delegateResult, mockHttpContextAccessor, TOKEN, this.claimsPrincipal).LaboratoryServiceMockInstance();

            string dateOfBirthString = this.dateOfBirth.ToString("yyyy-MM-dd", CultureInfo.CurrentCulture);
            string collectionDateString = this.collectionDate.ToString("yyyy-MM-dd", CultureInfo.CurrentCulture);

            RequestResult<PublicCovidTestResponse> actualResult = Task.Run(async () => await service.GetPublicCovidTestsAsync(this.phn, dateOfBirthString, collectionDateString).ConfigureAwait(true)).Result;

            expectedResult.ShouldDeepEqual(actualResult);
        }

        /// <summary>
        /// GetPublicTestResults - should return an error code for a data mismatch when the status indicator is DataMismatch or NotFound.
        /// </summary>
        /// <param name="statusIndicator">Status indicator returned from delegate.</param>
        [Theory]
        [InlineData(nameof(LabIndicatorType.DataMismatch))]
        [InlineData(nameof(LabIndicatorType.NotFound))]
        public void ShouldGetCovidTestsWithValidError(string statusIndicator)
        {
            RequestResult<PHSAResult<IEnumerable<CovidTestResult>>> delegateResult = new()
            {
                ResultStatus = ResultType.Success,
                ResourcePayload = new()
                {
                    LoadState = new(),
                    Result = new List<CovidTestResult>
                    {
                        new CovidTestResult { StatusIndicator = statusIndicator },
                    },
                },
            };

            Mock<IHttpContextAccessor> mockHttpContextAccessor = new HttpContextAccessorMock(TOKEN, this.claimsPrincipal);

            ILaboratoryService service = new LaboratoryServiceMock(delegateResult, mockHttpContextAccessor, TOKEN, this.claimsPrincipal).LaboratoryServiceMockInstance();

            string dateOfBirthString = this.dateOfBirth.ToString("yyyy-MM-dd", CultureInfo.CurrentCulture);
            string collectionDateString = this.collectionDate.ToString("yyyy-MM-dd", CultureInfo.CurrentCulture);

            RequestResult<PublicCovidTestResponse> actualResult = Task.Run(async () => await service.GetPublicCovidTestsAsync(this.phn, dateOfBirthString, collectionDateString).ConfigureAwait(true)).Result;

            Assert.Equal(ResultType.ActionRequired, actualResult.ResultStatus);
            Assert.Equal(ActionType.DataMismatch, actualResult.ResultError?.ActionCode);
            Assert.Empty(actualResult.ResourcePayload!.Records);
        }

        /// <summary>
        /// GetPublicTestResults - should return an error code for an invalid result when the status indicator is Threshold or Blocked.
        /// </summary>
        /// <param name="statusIndicator">Status indicator returned from delegate.</param>
        [Theory]
        [InlineData(nameof(LabIndicatorType.Threshold))]
        [InlineData(nameof(LabIndicatorType.Blocked))]
        public void ShouldGetCovidTestsWithInvalidError(string statusIndicator)
        {
            RequestResult<PHSAResult<IEnumerable<CovidTestResult>>> delegateResult = new()
            {
                ResultStatus = ResultType.Success,
                ResourcePayload = new()
                {
                    LoadState = new(),
                    Result = new List<CovidTestResult>
                    {
                        new CovidTestResult { StatusIndicator = statusIndicator },
                    },
                },
            };

            Mock<IHttpContextAccessor> mockHttpContextAccessor = new HttpContextAccessorMock(TOKEN, this.claimsPrincipal);

            ILaboratoryService service = new LaboratoryServiceMock(delegateResult, mockHttpContextAccessor, TOKEN, this.claimsPrincipal).LaboratoryServiceMockInstance();

            string dateOfBirthString = this.dateOfBirth.ToString("yyyy-MM-dd", CultureInfo.CurrentCulture);
            string collectionDateString = this.collectionDate.ToString("yyyy-MM-dd", CultureInfo.CurrentCulture);

            RequestResult<PublicCovidTestResponse> actualResult = Task.Run(async () => await service.GetPublicCovidTestsAsync(this.phn, dateOfBirthString, collectionDateString).ConfigureAwait(true)).Result;

            Assert.Equal(ResultType.ActionRequired, actualResult.ResultStatus);
            Assert.Equal(ActionType.Invalid, actualResult.ResultError?.ActionCode);
            Assert.Empty(actualResult.ResourcePayload!.Records);
        }

        /// <summary>
        /// GetPublicTestResults - should return an error code for a refresh in progress when that load state is returned by the delegate.
        /// </summary>
        [Fact]
        public void ShouldGetCovidTestsWithRefreshInProgress()
        {
            const int backOffMiliseconds = 500;

            RequestResult<PHSAResult<IEnumerable<CovidTestResult>>> delegateResult = new()
            {
                ResultStatus = ResultType.Success,
                ResourcePayload = new()
                {
                    LoadState = new() { RefreshInProgress = true, BackOffMilliseconds = backOffMiliseconds },
                    Result = new List<CovidTestResult>(),
                },
            };

            Mock<IHttpContextAccessor> mockHttpContextAccessor = new HttpContextAccessorMock(TOKEN, this.claimsPrincipal);

            ILaboratoryService service = new LaboratoryServiceMock(delegateResult, mockHttpContextAccessor, TOKEN, this.claimsPrincipal).LaboratoryServiceMockInstance();

            string dateOfBirthString = this.dateOfBirth.ToString("yyyy-MM-dd", CultureInfo.CurrentCulture);
            string collectionDateString = this.collectionDate.ToString("yyyy-MM-dd", CultureInfo.CurrentCulture);

            RequestResult<PublicCovidTestResponse> actualResult = Task.Run(async () => await service.GetPublicCovidTestsAsync(this.phn, dateOfBirthString, collectionDateString).ConfigureAwait(true)).Result;

            Assert.Equal(ResultType.ActionRequired, actualResult.ResultStatus);
            Assert.Equal(ActionType.Refresh, actualResult.ResultError?.ActionCode);
            Assert.Equal(backOffMiliseconds, actualResult.ResourcePayload?.RetryIn);
        }

        /// <summary>
        /// GetPublicTestResults - Invalid PHN.
        /// </summary>
        [Fact]
        public void ShouldGetCovidTestsWithInvalidPhn()
        {
            ILaboratoryService service = new LaboratoryServiceMock().LaboratoryServiceMockInstance();

            string invalidPhn = "123";
            string dateOfBirthString = this.dateOfBirth.ToString("yyyy-MM-dd", CultureInfo.CurrentCulture);
            string collectionDateString = this.collectionDate.ToString("yyyy-MM-dd", CultureInfo.CurrentCulture);

            RequestResult<PublicCovidTestResponse> actualResult = Task.Run(async () => await service.GetPublicCovidTestsAsync(invalidPhn, dateOfBirthString, collectionDateString).ConfigureAwait(true)).Result;

            Assert.Equal(ResultType.Error, actualResult.ResultStatus);
        }

        /// <summary>
        /// GetPublicTestResults - Invalid date of birth.
        /// </summary>
        /// <param name="dateFormat">Custom date format string.</param>
        [Theory]
        [InlineData("yyyyMMdd")]
        [InlineData("yyyy-MMM-dd")]
        [InlineData("dd/MM/yyyy")]
        public void ShouldGetCovidTestsWithInvalidDateOfBirth(string dateFormat)
        {
            ILaboratoryService service = new LaboratoryServiceMock().LaboratoryServiceMockInstance();

            string invalidDateOfBirthString = this.dateOfBirth.ToString(dateFormat, CultureInfo.CurrentCulture);
            string collectionDateString = this.collectionDate.ToString("yyyy-MM-dd", CultureInfo.CurrentCulture);

            RequestResult<PublicCovidTestResponse> actualResult = Task.Run(async () => await service.GetPublicCovidTestsAsync(this.phn, invalidDateOfBirthString, collectionDateString).ConfigureAwait(true)).Result;

            Assert.Equal(ResultType.Error, actualResult.ResultStatus);
        }

        /// <summary>
        /// GetPublicTestResults - Invalid collection date.
        /// </summary>
        /// <param name="dateFormat">Custom date format string.</param>
        [Theory]
        [InlineData("yyyyMMdd")]
        [InlineData("yyyy-MMM-dd")]
        [InlineData("dd/MM/yyyy")]
        public void ShouldGetCovidTestsWithInvalidCollectionDate(string dateFormat)
        {
            ILaboratoryService service = new LaboratoryServiceMock().LaboratoryServiceMockInstance();

            string dateOfBirthString = this.dateOfBirth.ToString("yyyy-MM-dd", CultureInfo.CurrentCulture);
            string invalidCollectionDateString = this.collectionDate.ToString(dateFormat, CultureInfo.CurrentCulture);

            RequestResult<PublicCovidTestResponse>? actualResult = Task.Run(async () => await service.GetPublicCovidTestsAsync(this.phn, dateOfBirthString, invalidCollectionDateString).ConfigureAwait(true)).Result;

            Assert.Equal(ResultType.Error, actualResult.ResultStatus);
        }
    }
}
