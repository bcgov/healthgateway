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
namespace HealthGateway.Immunization.Test.Services
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using HealthGateway.Common.Constants;
    using HealthGateway.Common.Delegates;
    using HealthGateway.Common.Models;
    using HealthGateway.Database.Constants;
    using HealthGateway.Database.Delegates;
    using HealthGateway.Immunization.Services;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Moq;
    using Xunit;

    /// <summary>
    /// VaccineProofService Unit Tests.
    /// </summary>
    public class VaccineProofServiceTests
    {
        /// <summary>
        /// Get a Vaccine Record Proof - Happy Path.
        /// </summary>
        [Fact]
        public void GetProofOk()
        {
            var myConfiguration = new Dictionary<string, string>
            {
                { "BCMailPlus:Endpoint", "https://${HOST}/${ENV}/auth=${TOKEN}/JSON/" },
                { "BCMailPlus:Host", "Host" },
                { "BCMailPlus:JobEnvironment", "JobEnvironment" },
                { "BCMailPlus:JobClass", "JobClass" },
                { "BCMailPlus:SchemaVersion", "SchemaVersion" },
                { "BCMailPlus:BackOffMilliseconds", "10" },
                { "BCMailPlus:MaxRetries", "2" },
            };

            VaccineProofRequest request = new()
            {
                SmartHealthCardQr = "SHC QR Image",
                Status = VaccinationStatus.Fully,
            };

            string hdid = "mock hdid";

            var mockVaccineProofDelegate = new Mock<IVaccineProofDelegate>();

            string id = "id";
            RequestResult<VaccineProofResponse> generateResult = new()
            {
                ResultStatus = ResultType.Success,
                ResourcePayload = new()
                {
                    Id = id,
                    Status = VaccineProofRequestStatus.Started,
                },
            };
            mockVaccineProofDelegate.Setup(s => s.GenerateAsync(It.IsAny<VaccineProofTemplate>(), request)).Returns(Task.FromResult(generateResult));

            RequestResult<VaccineProofResponse> statusResultStarted = new()
            {
                ResultStatus = ResultType.Success,
                ResourcePayload = new()
                {
                    Id = id,
                    Status = VaccineProofRequestStatus.Started,
                },
            };
            RequestResult<VaccineProofResponse> statusResultCompleted = new()
            {
                ResultStatus = ResultType.Success,
                ResourcePayload = new()
                {
                    Id = id,
                    Status = VaccineProofRequestStatus.Completed,
                },
            };
            mockVaccineProofDelegate.SetupSequence(s => s.GetStatusAsync(id))
                    .Returns(Task.FromResult(statusResultStarted))
                    .Returns(Task.FromResult(statusResultCompleted));

            RequestResult<ReportModel> assetResult = new()
            {
                ResultStatus = ResultType.Success,
                ResourcePayload = new()
                {
                    Data = "Base64 Encoded PDF",
                    FileName = "Filename.pdf",
                },
            };
            mockVaccineProofDelegate.Setup(s => s.GetAssetAsync(id)).Returns(Task.FromResult(assetResult));

            IVaccineProofService service = new VaccineProofService(
                GetConfiguration(myConfiguration),
                new Mock<ILogger<VaccineProofService>>().Object,
                mockVaccineProofDelegate.Object,
                new Mock<IVaccineProofRequestCacheDelegate>().Object);

            RequestResult<ReportModel> actualResult = Task.Run(async () => await service.GetVaccineProof(hdid, request, VaccineProofTemplate.Provincial).ConfigureAwait(true)).Result;
            Assert.True(actualResult.ResultStatus == ResultType.Success);
            Assert.True(actualResult.ResourcePayload != null &&
                        actualResult.ResourcePayload.Data == assetResult.ResourcePayload.Data);
        }

        /// <summary>
        /// The generate request on the Vaccine Proof delegate failed.
        /// </summary>
        [Fact]
        public void GetProofGenerateFailed()
        {
            var myConfiguration = new Dictionary<string, string>
            {
                { "BCMailPlus:Endpoint", "https://${HOST}/${ENV}/auth=${TOKEN}/JSON/" },
                { "BCMailPlus:Host", "Host" },
                { "BCMailPlus:JobEnvironment", "JobEnvironment" },
                { "BCMailPlus:JobClass", "JobClass" },
                { "BCMailPlus:SchemaVersion", "SchemaVersion" },
                { "BCMailPlus:BackOffMilliseconds", "10" },
                { "BCMailPlus:MaxRetries", "2" },
            };

            VaccineProofRequest request = new()
            {
                SmartHealthCardQr = "SHC QR Image",
                Status = VaccinationStatus.Fully,
            };

            string hdid = "mock hdid";

            var mockVaccineProofDelegate = new Mock<IVaccineProofDelegate>();

            RequestResult<VaccineProofResponse> generateResult = new()
            {
                ResultStatus = ResultType.Error,
            };
            mockVaccineProofDelegate.Setup(s => s.GenerateAsync(It.IsAny<VaccineProofTemplate>(), request)).Returns(Task.FromResult(generateResult));

            IVaccineProofService service = new VaccineProofService(
                GetConfiguration(myConfiguration),
                new Mock<ILogger<VaccineProofService>>().Object,
                mockVaccineProofDelegate.Object,
                new Mock<IVaccineProofRequestCacheDelegate>().Object);

            RequestResult<ReportModel> actualResult = Task.Run(async () => await service.GetVaccineProof(hdid, request, VaccineProofTemplate.Provincial).ConfigureAwait(true)).Result;
            Assert.True(actualResult.ResultStatus == Common.Constants.ResultType.Error);
        }

        /// <summary>
        /// The status request on the Vaccine Proof delegate failed.
        /// </summary>
        [Fact]
        public void GetProofStatusFailed()
        {
            var myConfiguration = new Dictionary<string, string>
            {
                { "BCMailPlus:Endpoint", "https://${HOST}/${ENV}/auth=${TOKEN}/JSON/" },
                { "BCMailPlus:Host", "Host" },
                { "BCMailPlus:JobEnvironment", "JobEnvironment" },
                { "BCMailPlus:JobClass", "JobClass" },
                { "BCMailPlus:SchemaVersion", "SchemaVersion" },
                { "BCMailPlus:BackOffMilliseconds", "10" },
                { "BCMailPlus:MaxRetries", "2" },
            };

            VaccineProofRequest request = new()
            {
                SmartHealthCardQr = "SHC QR Image",
                Status = VaccinationStatus.Fully,
            };

            string hdid = "mock hdid";

            var mockVaccineProofDelegate = new Mock<IVaccineProofDelegate>();

            string id = "id";
            RequestResult<VaccineProofResponse> generateResult = new()
            {
                ResultStatus = ResultType.Success,
                ResourcePayload = new()
                {
                    Id = id,
                    Status = VaccineProofRequestStatus.Started,
                },
            };
            mockVaccineProofDelegate.Setup(s => s.GenerateAsync(It.IsAny<VaccineProofTemplate>(), request)).Returns(Task.FromResult(generateResult));

            RequestResult<VaccineProofResponse> statusResult = new()
            {
                ResultStatus = ResultType.Error,
            };
            mockVaccineProofDelegate.Setup(s => s.GetStatusAsync(id)).Returns(Task.FromResult(statusResult));

            IVaccineProofService service = new VaccineProofService(
                GetConfiguration(myConfiguration),
                new Mock<ILogger<VaccineProofService>>().Object,
                mockVaccineProofDelegate.Object,
                new Mock<IVaccineProofRequestCacheDelegate>().Object);

            RequestResult<ReportModel> actualResult = Task.Run(async () => await service.GetVaccineProof(hdid, request, VaccineProofTemplate.Provincial).ConfigureAwait(true)).Result;
            Assert.True(actualResult.ResultStatus == ResultType.Error);
        }

        /// <summary>
        /// The get asset request on the Vaccine Proof delegate failed.
        /// </summary>
        [Fact]
        public void GetProofAssetFailed()
        {
            var myConfiguration = new Dictionary<string, string>
            {
                { "BCMailPlus:Endpoint", "https://${HOST}/${ENV}/auth=${TOKEN}/JSON/" },
                { "BCMailPlus:Host", "Host" },
                { "BCMailPlus:JobEnvironment", "JobEnvironment" },
                { "BCMailPlus:JobClass", "JobClass" },
                { "BCMailPlus:SchemaVersion", "SchemaVersion" },
                { "BCMailPlus:BackOffMilliseconds", "10" },
                { "BCMailPlus:MaxRetries", "2" },
            };

            VaccineProofRequest request = new()
            {
                SmartHealthCardQr = "SHC QR Image",
                Status = VaccinationStatus.Fully,
            };

            string hdid = "mock hdid";

            var mockVaccineProofDelegate = new Mock<IVaccineProofDelegate>();

            string id = "id";
            RequestResult<VaccineProofResponse> generateResult = new()
            {
                ResultStatus = ResultType.Success,
                ResourcePayload = new()
                {
                    Id = id,
                    Status = VaccineProofRequestStatus.Started,
                },
            };
            mockVaccineProofDelegate.Setup(s => s.GenerateAsync(It.IsAny<VaccineProofTemplate>(), request)).Returns(Task.FromResult(generateResult));

            RequestResult<VaccineProofResponse> statusResult = new()
            {
                ResultStatus = ResultType.Success,
                ResourcePayload = new()
                {
                    Id = id,
                    Status = VaccineProofRequestStatus.Completed,
                },
            };
            mockVaccineProofDelegate.Setup(s => s.GetStatusAsync(id)).Returns(Task.FromResult(statusResult));

            RequestResult<ReportModel> assetResult = new()
            {
                ResultStatus = ResultType.Error,
            };
            mockVaccineProofDelegate.Setup(s => s.GetAssetAsync(id)).Returns(Task.FromResult(assetResult));

            IVaccineProofService service = new VaccineProofService(
                GetConfiguration(myConfiguration),
                new Mock<ILogger<VaccineProofService>>().Object,
                mockVaccineProofDelegate.Object,
                new Mock<IVaccineProofRequestCacheDelegate>().Object);

            RequestResult<ReportModel> actualResult = Task.Run(async () => await service.GetVaccineProof(hdid, request, VaccineProofTemplate.Provincial).ConfigureAwait(true)).Result;
            Assert.True(actualResult.ResultStatus == ResultType.Error);
        }

        private static IConfiguration GetConfiguration(Dictionary<string, string>? keyValuePairs = null)
        {
            keyValuePairs ??= new();
            IConfiguration configuration = new ConfigurationBuilder()
                        .AddInMemoryCollection(keyValuePairs)
                        .Build();
            return configuration;
        }
    }
}
