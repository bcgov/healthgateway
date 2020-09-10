
using DeepEqual.Syntax;
using HealthGateway.Common.Models;
using HealthGateway.Immunization.Delegates;
using HealthGateway.Immunization.Models;
using HealthGateway.Immunization.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace HealthGateway.Immunization.Test.Controller
{
    public class ImmsService_Test
    {
        private readonly IConfiguration configuration;

        public ImmsService_Test()
        {
            this.configuration = GetIConfigurationRoot(string.Empty);
        }

        [Fact]
        public void ValidateImmunization()
        {

            var mockDelegate = new Mock<IImmunizationDelegate>();
            RequestResult<IEnumerable<ImmunizationResponse>> delegateResult = new RequestResult<IEnumerable<ImmunizationResponse>>()
            {
                ResultStatus = Common.Constants.ResultType.Success,
                ResourcePayload = new List<ImmunizationResponse>()
                {
                    new ImmunizationResponse()
                    {
                        Id = Guid.NewGuid(),
                        Name = "MockImmunization",
                        OccurrenceDateTime = DateTime.Now,
                        SourceSystemId = "MockSourceID"
                    },
                },
                PageIndex = 0,
                PageSize = 5,
                TotalResultCount = 1,
            };
            RequestResult<IEnumerable<ImmunizationModel>> expectedResult = new RequestResult<IEnumerable<ImmunizationModel>>()
            {
                ResultStatus = delegateResult.ResultStatus,
                ResourcePayload = ImmunizationModel.FromPHSAModelList(delegateResult.ResourcePayload),
                PageIndex = delegateResult.PageIndex,
                PageSize = delegateResult.PageSize,
                TotalResultCount = delegateResult.TotalResultCount,
            };

            mockDelegate.Setup(s => s.GetImmunizations(It.IsAny<string>(), It.IsAny<int>())).Returns(Task.FromResult(delegateResult));
            IImmunizationService service = new ImmunizationService(new Mock<ILogger<ImmunizationService>>().Object, mockDelegate.Object);

            var actualResult = service.GetImmunizations(string.Empty, 0);
            Assert.True(expectedResult.IsDeepEqual(actualResult.Result));
        }

        [Fact]
        public void ValidateImmunizationError()
        {

            var mockDelegate = new Mock<IImmunizationDelegate>();
            RequestResult<IEnumerable<ImmunizationResponse>> delegateResult = new RequestResult<IEnumerable<ImmunizationResponse>>()
            {
                ResultStatus = Common.Constants.ResultType.Error,
                ResultError = new RequestResultError()
                {
                    ResultMessage = "Mock Error",
                    ErrorCode = "MOCK_BAD_ERROR",
                },
            };
            RequestResult<IEnumerable<ImmunizationModel>> expectedResult = new RequestResult<IEnumerable<ImmunizationModel>>()
            {
                ResultStatus = delegateResult.ResultStatus,
                ResultError = delegateResult.ResultError,
            };

            mockDelegate.Setup(s => s.GetImmunizations(It.IsAny<string>(), It.IsAny<int>())).Returns(Task.FromResult(delegateResult));
            IImmunizationService service = new ImmunizationService(new Mock<ILogger<ImmunizationService>>().Object, mockDelegate.Object);

            var actualResult = service.GetImmunizations(string.Empty, 0);
            Assert.True(expectedResult.IsDeepEqual(actualResult.Result));
        }

        private static IConfigurationRoot GetIConfigurationRoot(string outputPath)
        {
            return new ConfigurationBuilder()
                // .SetBasePath(outputPath)
                .AddJsonFile("appsettings.json", optional: true)
                .AddJsonFile("appsettings.Development.json", optional: true)
                .AddJsonFile("appsettings.local.json", optional: true)
                .Build();
        }
    }
}
