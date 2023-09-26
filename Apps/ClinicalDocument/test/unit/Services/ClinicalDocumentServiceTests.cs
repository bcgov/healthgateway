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
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using HealthGateway.AccountDataAccess.Patient;
    using HealthGateway.ClinicalDocument.Api;
    using HealthGateway.ClinicalDocument.MapProfiles;
    using HealthGateway.ClinicalDocument.Models;
    using HealthGateway.ClinicalDocument.Models.PHSA;
    using HealthGateway.ClinicalDocument.Services;
    using HealthGateway.Common.Data.Constants;
    using HealthGateway.Common.Data.Models.PHSA;
    using HealthGateway.Common.Data.ViewModels;
    using HealthGateway.Common.Models.PHSA;
    using HealthGateway.Common.Services;
    using Microsoft.Extensions.Logging;
    using Moq;
    using Xunit;

    /// <summary>
    /// Tests for Clinical Documents.
    /// </summary>
    public class ClinicalDocumentServiceTests
    {
        private const string ClinicalDocumentRecordErrorMessage = "Error while retrieving Clinical Documents";
        private const string ClinicalDocumentFileErrorMessage = "Error while retrieving Clinical Document file";

        /// <summary>
        ///  Get clinical document records - happy path.
        /// </summary>
        /// <param name="canAccessDataSource">The value indicates whether the clinical document data source can be accessed or not.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task ShouldGetClinicalDocumentRecords(bool canAccessDataSource)
        {
            // Arrange
            Guid id = Guid.NewGuid();
            PhsaHealthDataResponse expectedPhsaHealthDataResponse = GetPhsaHealthDataResponse(id);

            IClinicalDocumentService clinicalDocumentService = GetClinicalDocumentService(expectedPhsaHealthDataResponse, false, canAccessDataSource);

            // Act
            RequestResult<IEnumerable<ClinicalDocumentRecord>> actualResult = await clinicalDocumentService.GetRecordsAsync(It.IsAny<string>());

            // Assert
            Assert.Equal(ResultType.Success, actualResult.ResultStatus);

            if (canAccessDataSource)
            {
                Assert.NotNull(actualResult.ResourcePayload);
                Assert.Equal(id.ToString(), actualResult.ResourcePayload?.First().Id);
            }
            else
            {
                Assert.Empty(actualResult.ResourcePayload);
            }
        }

        /// <summary>
        ///  Get clinical document records throws exception.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ShouldGetClinicalDocumentRecordsThrowsException()
        {
            // Arrange
            Guid id = Guid.NewGuid();
            PhsaHealthDataResponse expectedPhsaHealthDataResponse = GetPhsaHealthDataResponse(id);

            IClinicalDocumentService clinicalDocumentService = GetClinicalDocumentService(expectedPhsaHealthDataResponse, true);

            // Act
            RequestResult<IEnumerable<ClinicalDocumentRecord>> actualResult = await clinicalDocumentService.GetRecordsAsync(It.IsAny<string>());

            // Assert
            Assert.Equal(ResultType.Error, actualResult.ResultStatus);
            Assert.Equal(ClinicalDocumentRecordErrorMessage, actualResult.ResultError?.ResultMessage);
        }

        /// <summary>
        ///  Get clinical document file - happy path.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ShouldGetClinicalDocumentFile()
        {
            // Arrange
            Guid id = Guid.NewGuid();
            PhsaHealthDataResponse expectedPhsaHealthDataResponse = GetPhsaHealthDataResponse(id);

            IClinicalDocumentService clinicalDocumentService = GetClinicalDocumentService(expectedPhsaHealthDataResponse, false);

            // Act
            RequestResult<EncodedMedia> actualResult = await clinicalDocumentService.GetFileAsync(It.IsAny<string>(), It.IsAny<string>());

            // Assert
            Assert.Equal(ResultType.Success, actualResult.ResultStatus);
            Assert.NotNull(actualResult.ResourcePayload);
        }

        /// <summary>
        ///  Get clinical document records throws exception.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ShouldGetClinicalDocumentFileThrowsException()
        {
            // Arrange
            Guid id = Guid.NewGuid();
            PhsaHealthDataResponse expectedPhsaHealthDataResponse = GetPhsaHealthDataResponse(id);

            IClinicalDocumentService clinicalDocumentService = GetClinicalDocumentService(expectedPhsaHealthDataResponse, true);

            // Act
            RequestResult<EncodedMedia> actualResult = await clinicalDocumentService.GetFileAsync(It.IsAny<string>(), It.IsAny<string>());

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

        private static IClinicalDocumentService GetClinicalDocumentService(PhsaHealthDataResponse content, bool throwException, bool canAccessDataSource = true)
        {
            Mock<IClinicalDocumentsApi> clinicalDocumentApiMock = new();
            if (!throwException)
            {
                clinicalDocumentApiMock.Setup(c => c.GetClinicalDocumentRecordsAsync(It.IsAny<string>())).ReturnsAsync(content);
                clinicalDocumentApiMock.Setup(c => c.GetClinicalDocumentFileAsync(It.IsAny<Guid>(), It.IsAny<string>())).ReturnsAsync(new EncodedMedia());
            }
            else
            {
                clinicalDocumentApiMock.Setup(c => c.GetClinicalDocumentRecordsAsync(It.IsAny<string>())).ThrowsAsync(new HttpRequestException("Unit Test HTTP Request Exception"));
                clinicalDocumentApiMock.Setup(c => c.GetClinicalDocumentFileAsync(It.IsAny<Guid>(), It.IsAny<string>())).ThrowsAsync(new HttpRequestException("Unit Test HTTP Request Exception"));
            }

            PatientIdentity patientIdentity = new()
            {
                Pid = Guid.NewGuid(),
            };
            PersonalAccount personalAccount = new()
            {
                PatientIdentity = patientIdentity,
            };
            RequestResult<PersonalAccount> requestResult = new()
            {
                ResourcePayload = personalAccount,
                ResultStatus = ResultType.Success,
            };
            Mock<IPersonalAccountsService> personalAccountServiceMock = new();
            personalAccountServiceMock.Setup(p => p.GetPatientAccountResultAsync(It.IsAny<string>())).ReturnsAsync(requestResult);

            Mock<IPatientRepository> patientRepositoryMock = new();
            patientRepositoryMock.Setup(p => p.CanAccessDataSourceAsync(It.IsAny<string>(), It.IsAny<DataSource>(), It.IsAny<CancellationToken>())).ReturnsAsync(canAccessDataSource);

            return new ClinicalDocumentService(
                new Mock<ILogger<ClinicalDocumentService>>().Object,
                personalAccountServiceMock.Object,
                clinicalDocumentApiMock.Object,
                patientRepositoryMock.Object,
                InitializeAutoMapper());
        }

        private static IMapper InitializeAutoMapper()
        {
            MapperConfiguration config = new(cfg => { cfg.AddProfile(new ClinicalDocumentRecordProfile()); });

            return config.CreateMapper();
        }
    }
}
