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
namespace HealthGateway.ClinicalDocumentTests.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;
    using AutoMapper;
    using HealthGateway.ClinicalDocument.Api;
    using HealthGateway.ClinicalDocument.MapProfiles;
    using HealthGateway.ClinicalDocument.Models;
    using HealthGateway.ClinicalDocument.Models.PHSA;
    using HealthGateway.ClinicalDocument.Services;
    using HealthGateway.Common.Data.Constants;
    using HealthGateway.Common.Data.ViewModels;
    using HealthGateway.Common.Models.PHSA;
    using HealthGateway.Common.Services;
    using Microsoft.Extensions.Logging;
    using Moq;
    using Refit;
    using Xunit;

    /// <summary>
    /// Tests for Clinical Documents.
    /// </summary>
    public class ClinicalDocumentServiceTests
    {
        private const string ClinicalDocumentRecordErrorMessage = "Error with HTTP Request for Clinical Documents";
        private const string ClinicalDocumentFileErrorMessage = "Error with HTTP Request for Clinical Document file";

        /// <summary>
        ///  Get clinical document records - happy path.
        /// </summary>
        [Fact]
        public void ShouldGetClinicalDocumentRecords()
        {
            // Arrange
            Guid id = Guid.NewGuid();
            PhsaHealthDataResponse expectedPhsaHealthDataResponse = GetPhsaHealthDataResponse(id);

            IClinicalDocumentService clinicalDocumentService = GetClinicalDocumentService(expectedPhsaHealthDataResponse, null, false);

            // Act
            RequestResult<IEnumerable<ClinicalDocumentRecord>> actualResult =
                Task.Run(async () => await clinicalDocumentService.GetRecordsAsync(It.IsAny<string>()).ConfigureAwait(true)).Result;

            // Assert
            Assert.Equal(ResultType.Success, actualResult.ResultStatus);
            Assert.NotNull(actualResult.ResourcePayload);
            Assert.Equal(id.ToString(), actualResult.ResourcePayload?.First().Id);
        }

        /// <summary>
        ///  Get clinical document records returns api exception.
        /// </summary>
        [Fact]
        public void ShouldGetClinicalDocumentRecordsReturnsApiException()
        {
            // Arrange
            Guid id = Guid.NewGuid();
            PhsaHealthDataResponse expectedPhsaHealthDataResponse = GetPhsaHealthDataResponse(id);

            HttpResponseMessage responseMessage = new(HttpStatusCode.NotFound);
            RefitSettings refitSettings = new();
            ApiException apiException = ApiException.Create(It.IsAny<HttpRequestMessage>(), It.IsAny<HttpMethod>(), responseMessage, refitSettings).Result;
            responseMessage.Dispose();

            IClinicalDocumentService clinicalDocumentService = GetClinicalDocumentService(expectedPhsaHealthDataResponse, apiException, false);

            // Act
            RequestResult<IEnumerable<ClinicalDocumentRecord>> actualResult =
                Task.Run(async () => await clinicalDocumentService.GetRecordsAsync(It.IsAny<string>()).ConfigureAwait(true)).Result;

            // Assert
            Assert.Equal(ResultType.Error, actualResult.ResultStatus);
            Assert.Null(actualResult.ResourcePayload);
        }

        /// <summary>
        ///  Get clinical document records throws exception.
        /// </summary>
        [Fact]
        public void ShouldGetClinicalDocumentRecordsThrowsException()
        {
            // Arrange
            Guid id = Guid.NewGuid();
            PhsaHealthDataResponse expectedPhsaHealthDataResponse = GetPhsaHealthDataResponse(id);

            IClinicalDocumentService clinicalDocumentService = GetClinicalDocumentService(expectedPhsaHealthDataResponse, null, true);

            // Act
            RequestResult<IEnumerable<ClinicalDocumentRecord>> actualResult =
                Task.Run(async () => await clinicalDocumentService.GetRecordsAsync(It.IsAny<string>()).ConfigureAwait(true)).Result;

            // Assert
            Assert.Equal(ResultType.Error, actualResult.ResultStatus);
            Assert.Equal(ClinicalDocumentRecordErrorMessage, actualResult.ResultError?.ResultMessage);
        }

        /// <summary>
        ///  Get clinical document file - happy path.
        /// </summary>
        [Fact]
        public void ShouldGetClinicalDocumentFile()
        {
            // Arrange
            Guid id = Guid.NewGuid();
            PhsaHealthDataResponse expectedPhsaHealthDataResponse = GetPhsaHealthDataResponse(id);

            IClinicalDocumentService clinicalDocumentService = GetClinicalDocumentService(expectedPhsaHealthDataResponse, null, false);

            // Act
            RequestResult<EncodedMedia> actualResult =
                Task.Run(async () => await clinicalDocumentService.GetFileAsync(It.IsAny<string>(), It.IsAny<string>()).ConfigureAwait(true)).Result;

            // Assert
            Assert.Equal(ResultType.Success, actualResult.ResultStatus);
            Assert.NotNull(actualResult.ResourcePayload);
        }

        /// <summary>
        ///  Get clinical document file returns api exception.
        /// </summary>
        [Fact]
        public void ShouldGetClinicalDocumentFileReturnsApiException()
        {
            // Arrange
            Guid id = Guid.NewGuid();
            PhsaHealthDataResponse expectedPhsaHealthDataResponse = GetPhsaHealthDataResponse(id);

            HttpResponseMessage responseMessage = new(HttpStatusCode.NotFound);
            RefitSettings refitSettings = new();
            ApiException apiException = ApiException.Create(It.IsAny<HttpRequestMessage>(), It.IsAny<HttpMethod>(), responseMessage, refitSettings).Result;
            responseMessage.Dispose();

            IClinicalDocumentService clinicalDocumentService = GetClinicalDocumentService(expectedPhsaHealthDataResponse, apiException, false);

            // Act
            RequestResult<EncodedMedia> actualResult =
                Task.Run(async () => await clinicalDocumentService.GetFileAsync(It.IsAny<string>(), It.IsAny<string>()).ConfigureAwait(true)).Result;

            // Assert
            Assert.Equal(ResultType.Error, actualResult.ResultStatus);
            Assert.Null(actualResult.ResourcePayload);
        }

        /// <summary>
        ///  Get clinical document records throws exception.
        /// </summary>
        [Fact]
        public void ShouldGetClinicalDocumentFileThrowsException()
        {
            // Arrange
            Guid id = Guid.NewGuid();
            PhsaHealthDataResponse expectedPhsaHealthDataResponse = GetPhsaHealthDataResponse(id);

            IClinicalDocumentService clinicalDocumentService = GetClinicalDocumentService(expectedPhsaHealthDataResponse, null, true);

            // Act
            RequestResult<EncodedMedia> actualResult =
                Task.Run(async () => await clinicalDocumentService.GetFileAsync(It.IsAny<string>(), It.IsAny<string>()).ConfigureAwait(true)).Result;

            // Assert
            Assert.Equal(ResultType.Error, actualResult.ResultStatus);
            Assert.Equal(ClinicalDocumentFileErrorMessage, actualResult.ResultError?.ResultMessage);
        }

        private static PhsaHealthDataResponse GetPhsaHealthDataResponse(Guid id)
        {
            PhsaClinicalDocumentRecord phsaClinicalDocumentRecord = new()
            {
                Id = id.ToString(),
                FileId = "14bac0b6-9e95-4a1b-b6fd-d354edfce4e7-710b28fa980440fd93c426e25c0ce52f",
                Name = "Discharge summary",
                Type = "Discharge summary",
                FacilityName = "Lions Gate Hospital",
                Discipline = "Cancer",
                ServiceDate = DateTime.Today,
            };

            return new()
            {
                Data = new List<PhsaClinicalDocumentRecord>
                    { phsaClinicalDocumentRecord },
            };
        }

        private static IClinicalDocumentService GetClinicalDocumentService(PhsaHealthDataResponse content, ApiException? error, bool throwException)
        {
            HttpResponseMessage responseMessage = new(error == null ? HttpStatusCode.OK : HttpStatusCode.NotFound);

            IApiResponse<PhsaHealthDataResponse> recordsApiResponse = new ApiResponse<PhsaHealthDataResponse>(
                responseMessage,
                content,
                It.IsAny<RefitSettings>(),
                error);

            IApiResponse<EncodedMedia> fileApiResponse = new ApiResponse<EncodedMedia>(
                responseMessage,
                new EncodedMedia(),
                It.IsAny<RefitSettings>(),
                error);

            Mock<IClinicalDocumentsApi> clinicalDocumentApiMock = new();
            if (!throwException)
            {
                clinicalDocumentApiMock.Setup(c => c.GetClinicalDocumentRecords(It.IsAny<string>())).ReturnsAsync(recordsApiResponse);
                clinicalDocumentApiMock.Setup(c => c.GetClinicalDocumentFile(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(fileApiResponse);
            }
            else
            {
                clinicalDocumentApiMock.Setup(c => c.GetClinicalDocumentRecords(It.IsAny<string>())).ThrowsAsync(new HttpRequestException("Unit Test HTTP Request Exception"));
                clinicalDocumentApiMock.Setup(c => c.GetClinicalDocumentFile(It.IsAny<string>(), It.IsAny<string>())).ThrowsAsync(new HttpRequestException("Unit Test HTTP Request Exception"));
            }

            PatientIdentity patientIdentity = new()
            {
                Pid = Guid.NewGuid(),
            };
            PersonalAccount personalAccount = new()
            {
                PatientIdentity = patientIdentity,
            };
            RequestResult<PersonalAccount?> requestResult = new()
            {
                ResourcePayload = personalAccount,
                ResultStatus = ResultType.Success,
            };
            Mock<IPersonalAccountsService> personalAccountServiceMock = new();
            personalAccountServiceMock.Setup(p => p.GetPatientAccountAsync(It.IsAny<string>())).ReturnsAsync(requestResult);

            recordsApiResponse.Dispose();
            fileApiResponse.Dispose();
            responseMessage.Dispose();

            return new ClinicalDocumentService(
                new Mock<ILogger<ClinicalDocumentService>>().Object,
                personalAccountServiceMock.Object,
                clinicalDocumentApiMock.Object,
                InitializeAutoMapper());
        }

        private static IMapper InitializeAutoMapper()
        {
            MapperConfiguration config = new(cfg => { cfg.AddProfile(new ClinicalDocumentRecordProfile()); });

            return config.CreateMapper();
        }
    }
}
