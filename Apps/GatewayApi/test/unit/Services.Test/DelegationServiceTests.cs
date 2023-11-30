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
namespace HealthGateway.GatewayApiTests.Services.Test
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using HealthGateway.Common.Constants;
    using HealthGateway.Common.Data.Constants;
    using HealthGateway.Common.Data.ErrorHandling;
    using HealthGateway.Common.Data.Models;
    using HealthGateway.Common.Delegates;
    using HealthGateway.Common.Models.Cacheable;
    using HealthGateway.Common.Services;
    using HealthGateway.Database.Delegates;
    using HealthGateway.Database.Models;
    using HealthGateway.GatewayApi.Models;
    using HealthGateway.GatewayApi.Services;
    using HealthGateway.GatewayApiTests.Services.Test.Utils;
    using Microsoft.AspNetCore.DataProtection;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Moq;
    using Xunit;

    /// <summary>
    /// DelegationService's Unit Tests.
    /// </summary>
    public class DelegationServiceTests
    {
        private const string DelegationId = "a7673b82-16ee-4ce7-9467-a08bbd3217f7";
        private const string DelegateHdid = "DEV4FPEGCXG2NB5K2USBL52S66SC3GOUHWRP3GTXR2BTY5HEC4YA";
        private const string EncryptedDelegationId =
            "CfDJ8LLY0gwTcqdJoMgQs9OOAXd7dzOHlGh2YP-gemIPfyfqR4_7Igrj4B85s2bvgYSnrCgOPFHP8C0pG2oMTfrU2qxyT0BdMMET_MbEii514O4HtalNmPCVHl1dDLFyruuBA3RwRyC70uoVxZtpavV4tRRGd9lAq5VNwVWNez4kuvfC";
        private const string ProfileHdid = "GO4DOSMRJ7MFKPPADDZ3FK2MOJ45SFKONJWR67XNLMZQFNEHDKDA";
        private const string ResourceOwnerHdid = "P6FFO433A5WPMVTGM7T4ZVWBKCSVNAYGTWTU3J2LWMGUMERKI72A";

        private const string Hdid = "P6FFO433A5WPMVTGM7T4ZVWBKCSVNAYGTWTU3J2LWMGUMERKI72A";
        private const string Phn = "9735353315";
        private const string InviteKey = "ABCDEF";

        private const string InvalidEmailErrorMessage = "Email address is not valid";
        private const string InvalidDataSourceErrorMessage = "DataSources must have at least one item.";
        private const string InvalidNicknameErrorMessage = "Maximum length for Nickname has been exceeded.";
        private const string InvalidExpiryDateErrorMesage = "Invalid date";
        private const string EncryptedDelegationIdErrorMessage = "Encrypted Delegation Id must not be empty";
        private const string DelegationAlreadyAssociatedErrorMessage = "Delegation has already been associated with another profile.";
        private const string DelegationSelfAssociationErrorMessage = "The delegation cannot be associated with oneself.";
        private const string SharingLinkExpirationErrorMesage = "Sharing link has expired.";

        private const string ValidEmail = "delegator@gateway.ca";
        private const string InvalidEmail = "delegator@gateway";
        private const string ValidNickname = "12345678901234567890"; // 20 characters
        private const string InvalidNickname = "123456789012345678901"; // 21 characters

        private const string ExpiryHours = "48";

        private static readonly HashSet<DataSource> ValidDataSources =
        [
            DataSource.Immunization,
            DataSource.Medication,
        ];
        private static readonly HashSet<DataSource> InvalidDataSources = [];
        private static readonly IMapper Mapper = MapperUtil.InitializeAutoMapper();

        /// <summary>
        /// Returns Enumerable containing the test cases as arrays of objects.
        /// </summary>
        /// <returns>Test cases as arrays of objects.</returns>
        public static IEnumerable<object[]> TestCases()
        {
            yield return new object[] { ValidEmail, ValidNickname, 1, ValidDataSources, true, string.Empty };
            yield return new object[] { ValidEmail, ValidNickname, 0, ValidDataSources, true, string.Empty };
            yield return new object[] { InvalidEmail, ValidNickname, 1, ValidDataSources, false, InvalidEmailErrorMessage };
            yield return new object[] { ValidEmail, InvalidNickname, 1, ValidDataSources, false, InvalidNicknameErrorMessage };
            yield return new object[] { ValidEmail, ValidNickname, -1, ValidDataSources, false, InvalidExpiryDateErrorMesage };
            yield return new object[] { ValidEmail, ValidNickname, 1, InvalidDataSources, false, InvalidDataSourceErrorMessage };
        }

        /// <summary>
        /// Should associate a delegation.
        /// </summary>
        /// <param name="resourceOwnerHdid">The delegation's resource owner hdid.</param>
        /// <param name="delegateHdid">The delegate's hdid.</param>
        /// <param name="profileHdid">The profile hdid stored in Delegation.</param>
        /// <param name="secondsSinceCreation">The number of seconds since creation.</param>
        /// <param name="success">The value indicates whether the test should succeed or not.</param>
        /// <param name="errorMessage">The error message to display when success is false.</param>
        [Theory]
        [InlineData(ResourceOwnerHdid, DelegateHdid, null, 0, true, null)] // 0 Hours
        [InlineData(ResourceOwnerHdid, DelegateHdid, null, 172800, false, SharingLinkExpirationErrorMesage)] // 48 hours
        [InlineData(ResourceOwnerHdid, DelegateHdid, null, 172801, false, SharingLinkExpirationErrorMesage)] // 48 hours plus 1 second
        [InlineData(ResourceOwnerHdid, ResourceOwnerHdid, null, 0, false, DelegationSelfAssociationErrorMessage)]
        [InlineData(ResourceOwnerHdid, DelegateHdid, ProfileHdid, 0, false, DelegationAlreadyAssociatedErrorMessage)]
        public async Task AssociateDelegation(string resourceOwnerHdid, string delegateHdid, string? profileHdid, int secondsSinceCreation, bool success, string? errorMessage)
        {
            // Arrange
            Mock<IDelegationDelegate> delegationDelegate = new();
            PatientDetails patient = GetPatient();

            int expiryHours = int.Parse(ExpiryHours);
            DateTime createdDateTime = DateTime.UtcNow.AddSeconds(-secondsSinceCreation);

            Delegation delegation = new()
            {
                Id = Guid.NewGuid(),
                ResourceOwnerHdid = resourceOwnerHdid,
                ProfileHdid = profileHdid,
                CreatedDateTime = createdDateTime,

                ResourceOwnerIdentifier = $"{patient.PreferredName.GivenName} {patient.PreferredName.Surname[0]}",
                Nickname = ValidNickname,
                ExpiryDate = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(30),
                DataSources = ValidDataSources,
            };

            IDelegationService delegationService = GetDelegationService(delegationDelegate, delegation);

            if (success)
            {
                // Act
                await delegationService.AssociateDelegationAsync(delegateHdid, EncryptedDelegationId);

                // Verify
                delegationDelegate.Verify(
                    d => d.UpdateDelegationAsync(
                        It.Is<Delegation>(di => AssertDelegation(delegation, di)),
                        true));
            }
            else
            {
                // Act
                async Task Actual()
                {
                    await delegationService.AssociateDelegationAsync(delegateHdid, EncryptedDelegationId);
                }

                // Verify
                ProblemDetailsException exception = await Assert.ThrowsAsync<ProblemDetailsException>(Actual);
                Assert.Equal(errorMessage, exception.ProblemDetails!.Detail);
            }
        }

        /// <summary>
        /// Should throw problem details exception when no delegation is found.
        /// </summary>
        [Fact]
        public async Task AssociateDelegationThrowsDelegationNotFound()
        {
            // Arrange
            IDelegationService delegationService = GetDelegationService(); // will mock null delegation response

            // Act
            async Task Actual()
            {
                await delegationService.AssociateDelegationAsync(DelegateHdid, EncryptedDelegationId);
            }

            // Verify
            ProblemDetailsException exception = await Assert.ThrowsAsync<ProblemDetailsException>(Actual);
            Assert.Equal(ErrorMessages.DelegationNotFound, exception.ProblemDetails!.Detail);
        }

        /// <summary>
        /// Should create delegation.
        /// </summary>
        /// <param name="email">Email to validate.</param>
        /// <param name="nickname">Nickname to validate.</param>
        /// <param name="daysToAdd">Number of days to add to create expiry date from utc now.</param>
        /// <param name="dataSources">Data sources to validate.</param>
        /// <param name="success">The value indicates whether the test should succeed or not.</param>
        /// <param name="errorMessage">The error message to display when success is false.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Theory]
        [MemberData(nameof(TestCases))]
        public async Task CreateDelegation(string email, string nickname, int daysToAdd, HashSet<DataSource> dataSources, bool success, string errorMessage)
        {
            // Arrange
            Mock<IDelegationDelegate> delegationDelegate = new();
            PatientDetails patient = GetPatient();

            Delegation expectedDelegation = new()
            {
                ResourceOwnerHdid = Hdid,
                ResourceOwnerIdentifier = $"{patient.PreferredName.GivenName} {patient.PreferredName.Surname[0]}",
                Nickname = nickname,
                ExpiryDate = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(daysToAdd),
                DataSources = dataSources,
            };

            CreateDelegationRequest request = new()
            {
                Email = email,
                Nickname = nickname,
                ExpiryDate = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(daysToAdd),
                DataSources = dataSources,
            };

            IDelegationService delegationService = GetDelegationService(patient, GetHash(), delegationDelegate);

            if (success)
            {
                // Act
                await delegationService.CreateDelegationAsync(Hdid, request);

                // Verify
                delegationDelegate.Verify(
                    d => d.UpdateDelegationAsync(
                        It.Is<Delegation>(di => AssertDelegation(expectedDelegation, di)),
                        true));
            }
            else
            {
                // Act
                async Task Actual()
                {
                    await delegationService.CreateDelegationAsync(Hdid, request);
                }

                // Verify
                ProblemDetailsException exception = await Assert.ThrowsAsync<ProblemDetailsException>(Actual);
                Assert.Equal(errorMessage, exception.ProblemDetails!.Detail);
            }
        }

        private static bool AssertDelegation(Delegation expected, Delegation actual)
        {
            Assert.Equal(expected.ResourceOwnerHdid, actual.ResourceOwnerHdid);
            Assert.Equal(expected.ResourceOwnerIdentifier, actual.ResourceOwnerIdentifier);
            Assert.Equal(expected.Nickname, actual.Nickname);
            Assert.Equal(expected.ExpiryDate, actual.ExpiryDate);
            Assert.Equal(expected.DataSources, actual.DataSources);
            return true;
        }

        private static IDelegationService GetDelegationService(Mock<IDelegationDelegate> delegationDelegate = null, Delegation delegation = null)
        {
            delegationDelegate = delegationDelegate ?? new Mock<IDelegationDelegate>();
            delegationDelegate.Setup(d => d.GetDelegationAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(delegation);

            Mock<IDataProtectionProvider> dataProtectionProvider = new();
            Mock<IDataProtector> dataProtector = new();
            dataProtector.Setup(p => p.Unprotect(It.IsAny<byte[]>())).Returns(Encoding.UTF8.GetBytes(DelegationId));
            dataProtectionProvider.Setup(p => p.CreateProtector(It.IsAny<string>())).Returns(dataProtector.Object);

            return new DelegationService(
                GetConfiguration(),
                Mapper,
                new Mock<ILogger<DelegationService>>().Object,
                dataProtectionProvider.Object,
                delegationDelegate.Object,
                new Mock<IHashDelegate>().Object,
                new Mock<IEmailQueueService>().Object,
                new Mock<IPatientDetailsService>().Object);
        }

        private static IDelegationService GetDelegationService(PatientDetails patient, IHash hash, Mock<IDelegationDelegate> delegationDelegate)
        {
            Mock<IHashDelegate> hashDelegate = new();
            hashDelegate.Setup(d => d.Hash(It.IsAny<string>())).Returns(hash);

            Mock<IPatientDetailsService> patientDataService = new();
            patientDataService.Setup(s => s.GetPatientAsync(It.IsAny<string>(), PatientIdentifierType.Hdid, false, It.IsAny<CancellationToken>())).ReturnsAsync(patient);

            Mock<IDataProtectionProvider> dataProtectionProvider = new();
            Mock<IDataProtector> dataProtector = new();
            dataProtector.Setup(p => p.Protect(It.IsAny<byte[]>())).Returns(Encoding.UTF8.GetBytes(InviteKey));
            dataProtectionProvider.Setup(p => p.CreateProtector(It.IsAny<string>())).Returns(dataProtector.Object);

            Mock<IEmailQueueService> emailQueueService = new();
            emailQueueService.Setup(s => s.ProcessTemplate(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Dictionary<string, string>>())).Returns(GetEmail());
            emailQueueService.Setup(s => s.QueueNewEmail(It.IsAny<Email>(), true));

            return new DelegationService(
                GetConfiguration(),
                Mapper,
                new Mock<ILogger<DelegationService>>().Object,
                dataProtectionProvider.Object,
                delegationDelegate.Object,
                hashDelegate.Object,
                emailQueueService.Object,
                patientDataService.Object);
        }

        private static IConfigurationRoot GetConfiguration()
        {
            Dictionary<string, string?> configDictionary = new()
            {
                { "TimeZone:UnixTimeZoneId", "America/Vancouver" },
                { "TimeZone:WindowsTimeZoneId", "Pacific Standard Time" },
                { "DelegationInvitation:ExpiryHours", ExpiryHours },
            };

            return new ConfigurationBuilder()
                .AddInMemoryCollection(configDictionary.ToList())
                .Build();
        }

        private static Email GetEmail()
        {
            return new()
            {
                Id = Guid.NewGuid(),
                To = "somebody@gob.bc.ca",
                From = "healthgateway@gov.bc.ca",
            };
        }

        private static IHash GetHash()
        {
            return new HmacHash
            {
                Hash = "n08wp1vL8W0vvlnSQ1cJi/Yh49qnooYDzQd1W/HxZ/lq8LcaBxAf14yosd0FQIvMHEux7XLSn+4xwO40LH9How==",
                Salt = "knOnnxXtB08LUZ+fgN+Pdw==",
                Iterations = 21013,
                PseudoRandomFunction = HashFunction.HmacSha512,
            };
        }

        private static PatientDetails GetPatient()
        {
            return new()
            {
                Phn = Phn,
                HdId = Hdid,
                CommonName = new() { GivenName = "Bonny", Surname = "PROTERVITY" },
                LegalName = new() { GivenName = "Bonny", Surname = "PROTERVITY" },
                Birthdate = DateTime.UtcNow.AddYears(-30),
                Gender = "Female",
            };
        }
    }
}
