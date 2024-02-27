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

        private static readonly IClinicalDocumentMappingService MappingService = new ClinicalDocumentMappingService(InitializeAutoMapper());

        /// <summary>
        ///  Get clinical document records - happy path.
        /// </summary>
        /// <param name="canAccessDataSource">The value indicates whether the clinical document data source can be accessed or not.</param>
        /// <param name="personalAccountResultType">The result type for personal account response.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Theory]
        [InlineData(true, ResultType.Success)]
        [InlineData(true, ResultType.Error)]
        [InlineData(false, null)]
        public async Task ShouldGetClinicalDocumentRecords(bool canAccessDataSource, ResultType? personalAccountResultType)
        {
            // Arrange
            Guid id = Guid.NewGuid();
            PhsaHealthDataResponse expectedPhsaHealthDataResponse = GetPhsaHealthDataResponse(id);

            IClinicalDocumentService clinicalDocumentService = GetClinicalDocumentService(
                expectedPhsaHealthDataResponse,
                false,
                canAccessDataSource,
                personalAccountResultType: personalAccountResultType);

            // Act
            RequestResult<IEnumerable<ClinicalDocumentRecord>> actualResult = await clinicalDocumentService.GetRecordsAsync(It.IsAny<string>());

            // Assert
            if (canAccessDataSource)
            {
                Assert.Equal(personalAccountResultType, actualResult.ResultStatus);

                if (actualResult.ResultStatus == ResultType.Success)
                {
                    Assert.NotNull(actualResult.ResourcePayload);
                    Assert.Equal(id.ToString(), actualResult.ResourcePayload?.First().Id);
                }
                else
                {
                    Assert.Null(actualResult.ResourcePayload);
                }
            }
            else
            {
                Assert.Equal(ResultType.Success, actualResult.ResultStatus);
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
        /// <param name="canAccessDatasource">The value indicates whether the clinical document data source can be accessed or not.</param>
        /// <param name="personalAccountExists">
        /// The bool value indicating whether a pid exists or not to get the clinical document
        /// records.
        /// </param>
        /// <param name="personalAccountResultType">The result type for personal account response.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Theory]
        [InlineData(true, true, ResultType.Success)]
        [InlineData(true, true, ResultType.Error)]
        [InlineData(true, false, ResultType.Success)]
        [InlineData(false, true, null)]
        public async Task ShouldGetClinicalDocumentFile(bool canAccessDatasource, bool personalAccountExists, ResultType? personalAccountResultType)
        {
            // Arrange
            Guid id = Guid.NewGuid();
            PhsaHealthDataResponse expectedPhsaHealthDataResponse = GetPhsaHealthDataResponse(id);

            IClinicalDocumentService clinicalDocumentService = GetClinicalDocumentService(expectedPhsaHealthDataResponse, false, canAccessDatasource, personalAccountExists, personalAccountResultType);

            if (canAccessDatasource)
            {
                if (personalAccountExists)
                {
                    // Act
                    RequestResult<EncodedMedia> actualResult = await clinicalDocumentService.GetFileAsync(string.Empty, string.Empty);

                    // Assert
                    Assert.Equal(personalAccountResultType, actualResult.ResultStatus);

                    if (actualResult.ResultStatus == ResultType.Success)
                    {
                        Assert.NotNull(actualResult.ResourcePayload);
                    }
                    else
                    {
                        Assert.Null(actualResult.ResourcePayload);
                    }
                }
                else
                {
                    // Act and Assert
                    await Assert.ThrowsAsync<InvalidOperationException>(() => clinicalDocumentService.GetFileAsync(string.Empty, string.Empty));
                }
            }
            else
            {
                // Act
                RequestResult<EncodedMedia> actualResult = await clinicalDocumentService.GetFileAsync(string.Empty, string.Empty);

                // Assert
                Assert.Equal(ResultType.Success, actualResult.ResultStatus);
                Assert.Null(actualResult.ResourcePayload);
            }
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

        private static IClinicalDocumentService GetClinicalDocumentService(
            PhsaHealthDataResponse content,
            bool throwException,
            bool canAccessDataSource = true,
            bool personalAccountExists = true,
            ResultType? personalAccountResultType = ResultType.Success)
        {
            Mock<IClinicalDocumentsApi> clinicalDocumentApiMock = new();
            if (!throwException)
            {
                clinicalDocumentApiMock.Setup(c => c.GetClinicalDocumentRecordsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(content);
                clinicalDocumentApiMock.Setup(c => c.GetClinicalDocumentFileAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(new EncodedMedia());
            }
            else
            {
                clinicalDocumentApiMock.Setup(c => c.GetClinicalDocumentRecordsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                    .ThrowsAsync(new HttpRequestException("Unit Test HTTP Request Exception"));
                clinicalDocumentApiMock.Setup(c => c.GetClinicalDocumentFileAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                    .ThrowsAsync(new HttpRequestException("Unit Test HTTP Request Exception"));
            }

            PatientIdentity patientIdentity = new()
            {
                Pid = Guid.NewGuid(),
            };

            PersonalAccount? personalAccount = personalAccountExists
                ? new()
                {
                    PatientIdentity = patientIdentity,
                }
                : null;

            RequestResult<PersonalAccount> requestResult = new()
            {
                ResourcePayload = personalAccount,
                ResultStatus = personalAccountResultType ?? ResultType.Success,
            };
            Mock<IPersonalAccountsService> personalAccountServiceMock = new();
            personalAccountServiceMock.Setup(p => p.GetPatientAccountResultAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(requestResult);

            Mock<IPatientRepository> patientRepositoryMock = new();
            patientRepositoryMock.Setup(p => p.CanAccessDataSourceAsync(It.IsAny<string>(), It.IsAny<DataSource>(), It.IsAny<CancellationToken>())).ReturnsAsync(canAccessDataSource);

            return new ClinicalDocumentService(
                new Mock<ILogger<ClinicalDocumentService>>().Object,
                personalAccountServiceMock.Object,
                clinicalDocumentApiMock.Object,
                patientRepositoryMock.Object,
                MappingService);
        }

        private static IMapper InitializeAutoMapper()
        {
            MapperConfiguration config = new(cfg => { cfg.AddProfile(new ClinicalDocumentRecordProfile()); });

            return config.CreateMapper();
        }
    }
}
