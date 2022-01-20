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
        private const string HdId = "123";
        private const string IpAddress = "127.0.0.1";
        private const string MockedMessageID = "mockedMessageID";
        private const string MockedReportContent = "mockedReportContent";

        private readonly IConfiguration configuration = GetIConfigurationRoot();
        private readonly string phn = "9735353315";
        private readonly DateOnly dateOfBirth = new(1967, 06, 02);
        private readonly DateOnly collectionDate = new(2021, 07, 04);
        private readonly string accessToken = "XXDDXX";

        /// <summary>
        /// GetCovid19Orders test.
        /// </summary>
        [Fact]
        public void GetCovid19Orders()
        {
            ILaboratoryService service = this.GetLabServiceForCovid19Orders(ResultType.Success);

            Task<RequestResult<IEnumerable<Covid19Model>>> actualResult = service.GetCovid19Orders(HdId, 0);

            Assert.Equal(ResultType.Success, actualResult.Result.ResultStatus);
            int count = 0;
            foreach (Covid19Model model in actualResult.Result!.ResourcePayload!)
            {
                count++;
                Assert.True(model.MessageID.Equals(MockedMessageID + count, StringComparison.Ordinal));
            }

            Assert.Equal(2, count);
        }

        /// <summary>
        /// GetCovid19OrdersWithError test.
        /// </summary>
        [Fact]
        public void GetCovid19OrdersWithError()
        {
            ILaboratoryService service = this.GetLabServiceForCovid19Orders(ResultType.Error);

            Task<RequestResult<IEnumerable<Covid19Model>>> actualResult = service.GetCovid19Orders(HdId, 0);

            Assert.Equal(ResultType.Error, actualResult.Result.ResultStatus);
        }

        /// <summary>
        /// GetLabReport test.
        /// </summary>
        [Fact]
        public void GetLabReport()
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

            Mock<ILaboratoryDelegate> mockLaboratoryDelegate = new();
            mockLaboratoryDelegate.Setup(s => s.GetLabReport(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>())).Returns(Task.FromResult(delegateResult));

            Mock<ILaboratoryDelegateFactory> mockLaboratoryDelegateFactory = new();
            mockLaboratoryDelegateFactory.Setup(s => s.CreateInstance()).Returns(mockLaboratoryDelegate.Object);

            Mock<IHttpContextAccessor> mockHttpContextAccessor = new();
            DefaultHttpContext context = new()
            {
                Connection =
                {
                    RemoteIpAddress = IPAddress.Parse(IpAddress),
                },
            };
            context.Request.Headers.Add("Authorization", "MockJWTHeader");
            mockHttpContextAccessor.Setup(_ => _.HttpContext).Returns(context);

            ILaboratoryService service = new LaboratoryService(
                this.configuration,
                new Mock<ILogger<LaboratoryService>>().Object,
                mockLaboratoryDelegateFactory.Object,
                null!,
                null!,
                mockHttpContextAccessor.Object);

            Task<RequestResult<LaboratoryReport>> actualResult = service.GetLabReport(Guid.NewGuid(), string.Empty);

            Assert.Equal(ResultType.Success, actualResult.Result.ResultStatus);
            Assert.Equal(MockedReportContent, actualResult.Result.ResourcePayload!.Report);
        }

        /// <summary>
        /// GetPublicTestResults - Happy Path.
        /// </summary>
        [Fact]
        public void GetCovidTests()
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

            ILaboratoryService service = this.GetLabServiceForPublicCovidTests(delegateResult);

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
        public void GetCovidTestsWithDataMismatchError(string statusIndicator)
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

            ILaboratoryService service = this.GetLabServiceForPublicCovidTests(delegateResult);

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
        public void GetCovidTestsWithInvalidError(string statusIndicator)
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

            ILaboratoryService service = this.GetLabServiceForPublicCovidTests(delegateResult);

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
        public void GetCovidTestsWithRefreshInProgress()
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

            ILaboratoryService service = this.GetLabServiceForPublicCovidTests(delegateResult);

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
        public void GetCovidTestsWithInvalidPhn()
        {
            ILaboratoryService service = new LaboratoryService(
                this.configuration,
                new Mock<ILogger<LaboratoryService>>().Object,
                new Mock<ILaboratoryDelegateFactory>().Object,
                new Mock<IAuthenticationDelegate>().Object,
                GetMemoryCache(),
                new Mock<IHttpContextAccessor>().Object);

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
        public void GetCovidTestsWithInvalidDateOfBirth(string dateFormat)
        {
            ILaboratoryService service = new LaboratoryService(
                this.configuration,
                new Mock<ILogger<LaboratoryService>>().Object,
                new Mock<ILaboratoryDelegateFactory>().Object,
                new Mock<IAuthenticationDelegate>().Object,
                GetMemoryCache(),
                new Mock<IHttpContextAccessor>().Object);

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
        public void GetCovidTestsWithInvalidCollectionDate(string dateFormat)
        {
            ILaboratoryService service = new LaboratoryService(
                this.configuration,
                new Mock<ILogger<LaboratoryService>>().Object,
                new Mock<ILaboratoryDelegateFactory>().Object,
                new Mock<IAuthenticationDelegate>().Object,
                GetMemoryCache(),
                new Mock<IHttpContextAccessor>().Object);

            string dateOfBirthString = this.dateOfBirth.ToString("yyyy-MM-dd", CultureInfo.CurrentCulture);
            string invalidCollectionDateString = this.collectionDate.ToString(dateFormat, CultureInfo.CurrentCulture);

            RequestResult<PublicCovidTestResponse>? actualResult = Task.Run(async () => await service.GetPublicCovidTestsAsync(this.phn, dateOfBirthString, invalidCollectionDateString).ConfigureAwait(true)).Result;

            Assert.Equal(ResultType.Error, actualResult.ResultStatus);
        }

        private static IConfigurationRoot GetIConfigurationRoot()
        {
            Dictionary<string, string>? myConfiguration = new()
            {
                { "Laboratory:BackOffMilliseconds", "0" },
            };

            return new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: true)
                .AddJsonFile("appsettings.Development.json", optional: true)
                .AddJsonFile("appsettings.local.json", optional: true)
                .AddInMemoryCollection(myConfiguration)
                .Build();
        }

        private static IMemoryCache? GetMemoryCache()
        {
            ServiceCollection services = new();
            services.AddMemoryCache();
            ServiceProvider serviceProvider = services.BuildServiceProvider();

            return serviceProvider.GetService<IMemoryCache>();
        }

        private ILaboratoryService GetLabServiceForCovid19Orders(ResultType expectedResultType)
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

            Mock<ILaboratoryDelegate> mockLaboratoryDelegate = new();
            mockLaboratoryDelegate.Setup(s => s.GetCovid19Orders(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>())).Returns(Task.FromResult(delegateResult));

            Mock<ILaboratoryDelegateFactory> mockLaboratoryDelegateFactory = new();
            mockLaboratoryDelegateFactory.Setup(s => s.CreateInstance()).Returns(mockLaboratoryDelegate.Object);

            Mock<IHttpContextAccessor> mockHttpContextAccessor = new();
            DefaultHttpContext? context = new()
            {
                Connection =
                {
                    RemoteIpAddress = IPAddress.Parse(IpAddress),
                },
            };
            context.Request.Headers.Add("Authorization", "MockJWTHeader");
            mockHttpContextAccessor.Setup(_ => _.HttpContext).Returns(context);

            ILaboratoryService service = new LaboratoryService(
                this.configuration,
                new Mock<ILogger<LaboratoryService>>().Object,
                mockLaboratoryDelegateFactory.Object,
                null!,
                null!,
                mockHttpContextAccessor.Object);

            return service;
        }

        private ILaboratoryService GetLabServiceForPublicCovidTests(RequestResult<PHSAResult<IEnumerable<CovidTestResult>>> delegateResult)
        {
            JWTModel jwtModel = new()
            {
                AccessToken = this.accessToken,
            };

            Mock<ILaboratoryDelegate> mockLaboratoryDelegate = new();
            mockLaboratoryDelegate.Setup(s => s.GetPublicTestResults(this.accessToken, It.IsAny<string>(), It.IsAny<DateOnly>(), It.IsAny<DateOnly>())).Returns(Task.FromResult(delegateResult));

            Mock<ILaboratoryDelegateFactory> mockLaboratoryDelegateFactory = new();
            mockLaboratoryDelegateFactory.Setup(s => s.CreateInstance()).Returns(mockLaboratoryDelegate.Object);

            Mock<IAuthenticationDelegate> mockAuthDelegate = new();
            mockAuthDelegate.Setup(s => s.AuthenticateAsUser(It.IsAny<Uri>(), It.IsAny<ClientCredentialsTokenRequest>(), false)).Returns(jwtModel);

            Mock<IHttpContextAccessor> mockHttpContextAccessor = new();
            DefaultHttpContext? context = new()
            {
                Connection =
                {
                    RemoteIpAddress = IPAddress.Parse(IpAddress),
                },
            };
            context.Request.Headers.Add("Authorization", "MockJWTHeader");
            mockHttpContextAccessor.Setup(_ => _.HttpContext).Returns(context);

            ILaboratoryService service = new LaboratoryService(
                this.configuration,
                new Mock<ILogger<LaboratoryService>>().Object,
                mockLaboratoryDelegateFactory.Object,
                mockAuthDelegate.Object,
                GetMemoryCache(),
                mockHttpContextAccessor.Object);

            return service;
        }
    }
}
