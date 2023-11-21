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
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using HealthGateway.Common.Constants;
    using HealthGateway.Common.Data.Constants;
    using HealthGateway.Common.Data.ErrorHandling;
    using HealthGateway.Common.Data.Utils;
    using HealthGateway.Common.Delegates;
    using HealthGateway.Common.Models.Cacheable;
    using HealthGateway.Database.Delegates;
    using HealthGateway.Database.Models;
    using HealthGateway.GatewayApi.Models;
    using HealthGateway.GatewayApi.Services;
    using HealthGateway.GatewayApiTests.Services.Test.Utils;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Moq;
    using Xunit;

    /// <summary>
    /// DelegateService's Unit Tests.
    /// </summary>
    public class DelegateServiceTests
    {
        private const string Hdid = "P6FFO433A5WPMVTGM7T4ZVWBKCSVNAYGTWTU3J2LWMGUMERKI72A";
        private const string Phn = "9735353315";

        private const string ValidEmail = "delegator@gateway.ca";
        private const string InvalidEmail = "delegator@gateway";
        private const string ValidNickname = "12345678901234567890"; // 20 characters
        private const string InvalidNickname = "123456789012345678901"; // 21 characters
        private static readonly HashSet<DataSource> ValidDataSources = new()
        {
            DataSource.Immunization,
            DataSource.Medication,
        };

        private static readonly HashSet<DataSource> InvalidDataSources = new();
        private static readonly IMapper Mapper = MapperUtil.InitializeAutoMapper();

        /// <summary>
        /// Returns Enumerable containing the test cases as arrays of objects.
        /// </summary>
        /// <returns>Test cases as arrays of objects.</returns>
        public static IEnumerable<object[]> TestCases()
        {
            yield return new object[] { ValidEmail, ValidNickname, 1, ValidDataSources, true };
            yield return new object[] { ValidEmail, ValidNickname, 0, ValidDataSources, true };
            yield return new object[] { InvalidEmail, ValidNickname, 1, ValidDataSources, false };
            yield return new object[] { ValidEmail, InvalidNickname, 1, ValidDataSources, false };
            yield return new object[] { ValidEmail, ValidNickname, -1, ValidDataSources, false };
            yield return new object[] { ValidEmail, ValidNickname, 1, InvalidDataSources, false };
        }

        /// <summary>
        /// Should create delegate invitation.
        /// </summary>
        /// <param name="email">Email to validate.</param>
        /// <param name="nickname">Nickname to validate.</param>
        /// <param name="daysToAdd">Number of days to add to create expiry date from utc now.</param>
        /// <param name="dataSources">Data sources to validate.</param>
        /// <param name="success">The value indicates whether the test should succeed or not.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Theory]
        [MemberData(nameof(TestCases))]
        public async Task CreateDelegateInvitation(string email, string nickname, int daysToAdd, HashSet<DataSource> dataSources, bool success)
        {
            TimeZoneInfo localTimezone = DateFormatter.GetLocalTimeZone(GetConfiguration());
            Mock<IDelegateInvitationDelegate> delegateInvitationDelegate = new();

            // Setup
            PatientDetails patient = GetPatient(localTimezone);

            DelegateInvitation expectedDelegateInvitation = new()
            {
                ResourceOwnerHdid = Hdid,
                ResourceOwnerIdentifier = $"{patient.PreferredName.GivenName} {patient.PreferredName.Surname[0]}",
                Nickname = nickname,
                ExpiryDate = DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, localTimezone)).AddDays(daysToAdd),
                DataSources = dataSources,
            };

            CreateDelegateInvitationRequest request = new()
            {
                Email = email,
                Nickname = nickname,
                ExpiryDate = DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, localTimezone)).AddDays(daysToAdd),
                DataSources = dataSources,
            };

            IDelegateService delegateService = GetDelegateService(patient, GetHash(), delegateInvitationDelegate);

            if (success)
            {
                // Act
                await delegateService.CreateDelegateInvitationAsync(Hdid, request);

                // Verify
                delegateInvitationDelegate.Verify(
                    d => d.UpdateDelegateInvitationAsync(
                        It.Is<DelegateInvitation>(di => AssertDelegateInvitation(expectedDelegateInvitation, di)),
                        true));
            }
            else
            {
                // Act
                async Task Actual()
                {
                    await delegateService.CreateDelegateInvitationAsync(Hdid, request);
                }

                // Verify
                ProblemDetailsException exception = await Assert.ThrowsAsync<ProblemDetailsException>(Actual);
                Assert.Equal(ErrorMessages.InvalidDelegateInvitationRequest, exception.ProblemDetails!.Detail);
            }
        }

        private static bool AssertDelegateInvitation(DelegateInvitation expected, DelegateInvitation actual)
        {
            Assert.Equal(expected.ResourceOwnerHdid, actual.ResourceOwnerHdid);
            Assert.Equal(expected.ResourceOwnerIdentifier, actual.ResourceOwnerIdentifier);
            Assert.Equal(expected.Nickname, actual.Nickname);
            Assert.Equal(expected.ExpiryDate, actual.ExpiryDate);
            Assert.Equal(expected.DataSources, actual.DataSources);
            return true;
        }

        private static IDelegateService GetDelegateService(PatientDetails patient, IHash hash, Mock<IDelegateInvitationDelegate> delegateInvitationDelegate)
        {
            Mock<IHashDelegate> hashDelegate = new();
            hashDelegate.Setup(d => d.Hash(It.IsAny<string>())).Returns(hash);

            Mock<IPatientDetailsService> patientDataService = new();
            patientDataService.Setup(s => s.GetPatientAsync(It.IsAny<string>(), PatientIdentifierType.Hdid, false, It.IsAny<CancellationToken>())).ReturnsAsync(patient);

            return new DelegateService(
                GetConfiguration(),
                Mapper,
                new Mock<ILogger<DelegateService>>().Object,
                delegateInvitationDelegate.Object,
                hashDelegate.Object,
                patientDataService.Object);
        }

        private static IConfigurationRoot GetConfiguration()
        {
            Dictionary<string, string?> configDictionary = new()
            {
                { "TimeZone:UnixTimeZoneId", "America/Vancouver" },
                { "TimeZone:WindowsTimeZoneId", "Pacific Standard Time" },
            };

            return new ConfigurationBuilder()
                .AddInMemoryCollection(configDictionary.ToList())
                .Build();
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

        private static PatientDetails GetPatient(TimeZoneInfo localTimezone)
        {
            return new()
            {
                Phn = Phn,
                HdId = Hdid,
                CommonName = new() { GivenName = "Bonny", Surname = "PROTERVITY" },
                LegalName = new() { GivenName = "Bonny", Surname = "PROTERVITY" },
                Birthdate = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, localTimezone).AddYears(-30),
                Gender = "Female",
            };
        }
    }
}
